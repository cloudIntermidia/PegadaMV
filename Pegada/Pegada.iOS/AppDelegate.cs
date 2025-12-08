using Foundation;
using UIKit;
using Prism;
using Prism.Ioc;
using MobiliVendas.Core.Lib.IOS;
using KeyboardOverlap.Forms.Plugin.iOSUnified;
using CarouselView.FormsPlugin.iOS;
using MobiliVendas.Core.Lib.IOS.Sqlite;
using MobiliVendas.Core.Lib.IOS.Services;
using Pegada.Core;
using MobiliVendas.Core.Infra.DataContext;
using MobiliVendas.Core.Services.Contracts;
using Syncfusion.ListView.XForms.iOS;
using Syncfusion.XForms.iOS.Buttons;
using Syncfusion.XForms.iOS.Border;
using Syncfusion.SfNumericTextBox.XForms.iOS;

namespace Pegada.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            new SfNumericTextBoxRenderer();
            InitPlugins();

            //var r = new GridViewRenderer();
            Syncfusion.SfDataGrid.XForms.iOS.SfDataGridRenderer.Init();
            LoadApplication(new App(new iOSInitializer()));

            return base.FinishedLaunching(app, options);
        }

        public void InitPlugins(){
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            global::Xamarin.FormsMaps.Init();
            KeyboardOverlapRenderer.Init();
            CarouselViewRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            Rg.Plugins.Popup.Popup.Init();
            Flex.FlexButton.Init();
            SfListViewRenderer.Init();            
            new Syncfusion.XForms.iOS.ComboBox.SfComboBoxRenderer();
            SfButtonRenderer.Init();
            SfBorderRenderer.Init();

        }

    }

    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(ISqliteConnection), typeof(SQLiteDbconnection));
            containerRegistry.Register(typeof(IDeviceInformation), typeof(DeviceInformation));
            containerRegistry.Register(typeof(IPrintService), typeof(PrintService));
        }
    }
}
