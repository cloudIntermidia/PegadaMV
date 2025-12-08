using Android.App;
using Android.Content.PM;
using Android.OS;
using MobiliVendas.Core.Services.Contracts;
using Prism;
using Prism.Ioc;
using MobiliVendas.Core.Infra.DataContext;
using FFImageLoading.Forms.Platform;
using Android.Runtime;
using Plugin.Permissions;
using MobiliVendas.Core.Lib.Android.Sqlite;
using MobiliVendas.Core.Lib.Android.Services;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using CarouselView.FormsPlugin.Android;

namespace Pegada.Droid
{
    [Activity(Label = "Pegada",
              Icon = "@drawable/icon", Theme = "@style/MainTheme",
              MainLauncher = false,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            InitPlugins(bundle);
            LoadApplication(new Pegada.Core.App(new AndroidInitializer()));
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var newExc = new System.Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(newExc);
        }

        private static void CurrentDomainOnUnhandledException(object sender, System.UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var newExc = new System.Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as System.Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(System.Exception exception)
        {
            try
            {
                const string errorFileName = "Fatal.log";
                var libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // iOS: Environment.SpecialFolder.Resources
                var errorFilePath = Path.Combine(libraryPath, errorFileName);
                var errorMessage = System.String.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}",
                System.DateTime.Now, exception.ToString());
                File.WriteAllText(errorFilePath, errorMessage);

                // Log to Android Device Logging.
                Android.Util.Log.Error("Crash Report", errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }

        /// <summary>
        // If there is an unhandled exception, the exception information is diplayed 
        // on screen the next time the app is started (only in debug configuration)
        /// </summary>
        [Conditional("DEBUG")]
        private void DisplayCrashReport()
        {
            const string errorFilename = "Fatal.log";
            var libraryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var errorFilePath = Path.Combine(libraryPath, errorFilename);

            if (!File.Exists(errorFilePath))
            {
                return;
            }

            var errorText = File.ReadAllText(errorFilePath);
            new AlertDialog.Builder(this)
                .SetPositiveButton("Clear", (sender, args) =>
                {
                    File.Delete(errorFilePath);
                })
                .SetNegativeButton("Close", (sender, args) =>
                {
                    // User pressed Close.
                })
                .SetMessage(errorText)
                .SetTitle("Crash Report")
                .Show();
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void InitPlugins(Bundle bundle)
        {
            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.FormsMaps.Init(this, bundle);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            CarouselViewRenderer.Init();
            CachedImageRenderer.Init(true);
            Acr.UserDialogs.UserDialogs.Init(this);
            ZXing.Net.Mobile.Forms.Android.ZXingBarcodeImageViewRenderer.Init();
            Rg.Plugins.Popup.Popup.Init(this, bundle);
        }

    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
            containerRegistry.RegisterSingleton(typeof(ISqliteConnection), typeof(SQLiteDbconnection));
            containerRegistry.Register(typeof(IDeviceInformation), typeof(DeviceInformation));
            containerRegistry.Register(typeof(IPrintService), typeof(PrintService));
            containerRegistry.Register(typeof(IAndroidDonwloader), typeof(AndroidDonwloader));
        }
    }
}

