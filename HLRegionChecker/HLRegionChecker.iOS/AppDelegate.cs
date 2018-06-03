using Foundation;
using HLRegionChecker.iOS.Manager;
using Prism;
using Prism.Ioc;
using System;
using UIKit;
using UserNotifications;

namespace HLRegionChecker.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {

        /// <summary>
        /// プッシュ通知の初期化処理を行います。
        /// </summary>
        private void RegisterForNotifications()
        {
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert |
                UNAuthorizationOptions.Badge |
                UNAuthorizationOptions.Sound,
                (approved, err) =>
                {
                    Console.WriteLine("RequestAuthorization:{0}", approved.ToString());
                });
            UNUserNotificationCenter.Current.GetNotificationSettings((settings) =>
            {
                var alertsAllowed = (settings.AlertSetting == UNNotificationSetting.Enabled);
            });
            UNUserNotificationCenter.Current.Delegate = new Notification.UserNotificationCenterDelegate();
        }

        public override bool WillFinishLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            //Firebaseの初期化
            Firebase.Core.App.Configure();

            return base.WillFinishLaunching(uiApplication, launchOptions);
        }

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
            LoadApplication(new App(new iOSInitializer()));

            //位置情報利用の許可
            LocationManager.GetInstance().RequestAlwaysAuthorization();
            //プッシュ通知の許可
            RegisterForNotifications();

            return base.FinishedLaunching(app, options);
        }
    }

    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {

        }
    }
}
