using System;
using Prism;
using Prism.Ioc;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Syncfusion.ListView.XForms.UWP;

namespace Pegada.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            try
            {
                this.InitializeComponent();
                Xamarin.FormsMaps.Init("qLse5jll7vRvn0TBDikc~yQ1SENqb0JvvQSUWnwDU2g~AoCCytc-NyunUUWPl47YhbWePTm-9P9U3NjntV5p71XeeS4t3WUCxDPwlhZBh31G");

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    // Get how big the window can be in epx.
                    var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                    //To get Screen Measurements e.g. height, width, X,Y...
                    ApplicationView view = ApplicationView.GetForCurrentView();
                    //Getting the Window Title Bar height(In my case I get :Top=32,Bottom=860)
                    double titleBarHeight = view.VisibleBounds.Top;
                    //Getting the TaskBar Height
                    double taskBarHeight = view.VisibleBounds.Top + (view.VisibleBounds.Top / 4);
                    //Getting the workable Height of the screen ( excluding Task Bar Height)
                    //double availableheight = GridTimelineContent.ActualHeight - taskBarHeight;
                    //double availablewidth = GridTimelineContent.ActualWidth;


                    ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(1024, 768));
                    ApplicationView.PreferredLaunchViewSize = new Size(1024, 768);
                    ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
                    ApplicationView.GetForCurrentView().TryResizeView(new Size(1024, 768));
                }
                SfListViewRenderer.Init();
                Syncfusion.SfDataGrid.XForms.UWP.SfDataGridRenderer.Init();

                LoadApplication(new Pegada.Core.App(new UwpInitializer()));
            }
            catch (Exception ex)
            {
            }
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
        }
    }

    public class UwpInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(MobiliVendas.Core.Infra.DataContext.ISqliteConnection), typeof(MobiliVendas.Core.Lib.UWP.Sqlite.SQLiteDbconnection));
            containerRegistry.Register(typeof(MobiliVendas.Core.Services.Contracts.IDeviceInformation), typeof(MobiliVendas.Core.Lib.UWP.Services.DeviceInformation));
            containerRegistry.Register(typeof(MobiliVendas.Core.Services.Contracts.IPrintService), typeof(MobiliVendas.Core.Lib.UWP.Services.PrintService));
        }
    }
}
