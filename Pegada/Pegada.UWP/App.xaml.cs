using CarouselView.FormsPlugin.UWP;
using FFImageLoading.Forms;
using FFImageLoading.Forms.Platform;
using MobiliVendas.Core.Lib.UWP.CustomView;
using MobiliVendas.Core.Lib.UWP.CustomView.Controls.SegmentedControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Syncfusion.ListView.XForms.UWP;
using Syncfusion.SfNumericTextBox.XForms.UWP;
using Syncfusion.XForms.UWP.ComboBox;
using Syncfusion.XForms.UWP.TextInputLayout;
using Xamarin.Forms.Platform.UWP;
using ZXing.Net.Mobile.Forms.WindowsUniversal;
using Syncfusion.XForms.UWP.Border;
using Syncfusion.XForms.UWP.Buttons;

namespace Pegada.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {


            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                List<Assembly> assembliesToInclude = new List<Assembly>();


                /*
                    Adicionar os assemblies dos PLUGINS
                 */
                CachedImageRenderer.Init();
                assembliesToInclude.Add(typeof(CarouselView.FormsPlugin.UWP.CarouselViewRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(CachedImage).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(CachedImageRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(ZXingBarcodeImageViewRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Rg.Plugins.Popup.Popup).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Flex.Controls.FlexButton).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Flex.Controls.GestureFrame).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Flex.UWP.CustomRenderers.GestureFrameRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Flex.UWP.Effects.ColorOverlayEffectUWP).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SfListViewRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SfComboBoxRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SfTextInputLayoutRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SfNumericTextBoxRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SfButtonRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SfBorderRenderer).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(Syncfusion.SfDataGrid.XForms.UWP.SfDataGridRenderer).GetTypeInfo().Assembly);
                Rg.Plugins.Popup.Popup.Init();

               
                
                /*
                    Adicionar os assemblies dos CustomRenderes
                 */
                assembliesToInclude.Add(typeof(CustomEntry).GetTypeInfo().Assembly);
                assembliesToInclude.Add(typeof(SegmentedControlRenderer).GetTypeInfo().Assembly);



                Xamarin.Forms.Forms.Init(e, assembliesToInclude);

                //((Style)this.Resources["TabbedPageStyle"]).Setters[0] = ((Style)this.Resources["TabbedPageStyle2"]).Setters[0];
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
