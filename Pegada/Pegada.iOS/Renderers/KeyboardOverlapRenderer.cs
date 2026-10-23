using System;
using System.Diagnostics;
using CoreGraphics;
using Foundation;
using Pegada.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Page), typeof(KeyboardOverlapRenderer))]
namespace Pegada.iOS.Renderers
{
    // Substitui o pacote Xam.Plugins.Forms.KeyboardOverlap (mesma lógica: sobe a página quando o teclado cobre o campo).
    // O plugin só deixava de observar o teclado no ViewWillDisappear; quando o Forms descartava o renderer sem passar
    // por ele, o próximo teclado chamava um objeto descartado (ObjectDisposedException em callback nativo → app fechava).
    public class KeyboardOverlapRenderer : PageRenderer
    {
        NSObject _keyboardShowObserver;
        NSObject _keyboardHideObserver;
        bool _pageWasShiftedUp;
        double _activeViewBottom;
        bool _isKeyboardShown;
        bool _disposed;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var page = Element as ContentPage;
            if (page == null || page.Content is ScrollView)
                return;

            RegisterForKeyboardNotifications();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            UnregisterForKeyboardNotifications();
        }

        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            UnregisterForKeyboardNotifications();
            base.Dispose(disposing);
        }

        void RegisterForKeyboardNotifications()
        {
            if (_disposed)
                return;

            if (_keyboardShowObserver == null)
                _keyboardShowObserver = UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShow);
            if (_keyboardHideObserver == null)
                _keyboardHideObserver = UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
        }

        void UnregisterForKeyboardNotifications()
        {
            _isKeyboardShown = false;

            if (_keyboardShowObserver != null)
            {
                _keyboardShowObserver.Dispose();
                _keyboardShowObserver = null;
            }

            if (_keyboardHideObserver != null)
            {
                _keyboardHideObserver.Dispose();
                _keyboardHideObserver = null;
            }
        }

        bool PodeTratarTeclado()
        {
            return !_disposed && Handle != IntPtr.Zero && Element != null && IsViewLoaded;
        }

        void OnKeyboardShow(object sender, UIKeyboardEventArgs args)
        {
            try
            {
                if (!PodeTratarTeclado() || _isKeyboardShown)
                    return;

                _isKeyboardShown = true;

                var activeView = FindFirstResponder(View);
                if (activeView == null)
                    return;

                var keyboardFrame = UIKeyboard.FrameEndFromNotification(args.Notification);
                if (!IsKeyboardOverlapping(activeView, View, keyboardFrame))
                    return;

                _activeViewBottom = GetViewRelativeBottom(activeView, View);
                ShiftPageUp(keyboardFrame.Height, _activeViewBottom);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KeyboardOverlapRenderer.OnKeyboardShow: {ex}");
            }
        }

        void OnKeyboardHide(object sender, UIKeyboardEventArgs args)
        {
            try
            {
                if (!PodeTratarTeclado())
                    return;

                _isKeyboardShown = false;

                var keyboardFrame = UIKeyboard.FrameEndFromNotification(args.Notification);
                if (_pageWasShiftedUp)
                    ShiftPageDown(keyboardFrame.Height, _activeViewBottom);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KeyboardOverlapRenderer.OnKeyboardHide: {ex}");
            }
        }

        void ShiftPageUp(nfloat keyboardHeight, double activeViewBottom)
        {
            var pageFrame = Element.Bounds;
            var newY = pageFrame.Y + CalculateShiftByAmount(pageFrame.Height, keyboardHeight, activeViewBottom);

            Element.LayoutTo(new Rectangle(pageFrame.X, newY, pageFrame.Width, pageFrame.Height));
            _pageWasShiftedUp = true;
        }

        void ShiftPageDown(nfloat keyboardHeight, double activeViewBottom)
        {
            var pageFrame = Element.Bounds;
            var newY = pageFrame.Y - CalculateShiftByAmount(pageFrame.Height, keyboardHeight, activeViewBottom);

            Element.LayoutTo(new Rectangle(pageFrame.X, newY, pageFrame.Width, pageFrame.Height));
            _pageWasShiftedUp = false;
        }

        static double CalculateShiftByAmount(double pageHeight, nfloat keyboardHeight, double activeViewBottom)
        {
            return (pageHeight - activeViewBottom) - keyboardHeight;
        }

        static UIView FindFirstResponder(UIView view)
        {
            if (view.IsFirstResponder)
                return view;

            foreach (var subView in view.Subviews)
            {
                var firstResponder = FindFirstResponder(subView);
                if (firstResponder != null)
                    return firstResponder;
            }

            return null;
        }

        static double GetViewRelativeBottom(UIView view, UIView rootView)
        {
            var viewRelativeCoordinates = rootView.ConvertPointFromView(view.Frame.Location, view);
            var activeViewRoundedY = Math.Round(viewRelativeCoordinates.Y, 2);

            return activeViewRoundedY + view.Frame.Height;
        }

        static bool IsKeyboardOverlapping(UIView activeView, UIView rootView, CGRect keyboardFrame)
        {
            var activeViewBottom = GetViewRelativeBottom(activeView, rootView);
            var pageHeight = rootView.Frame.Height;

            return activeViewBottom >= (pageHeight - keyboardFrame.Height);
        }
    }
}
