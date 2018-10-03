using Firebase.CloudMessaging;
using Foundation;
using HLRegionChecker.Interfaces;
using HLRegionChecker.iOS.DependencyServices;
using HLRegionChecker.iOS.Manager;
using HLRegionChecker.Models;
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
            Messaging.SharedInstance.Delegate = new FcmDelegate();

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
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
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

            //デバイス識別子登録
            var devId = UIKit.UIDevice.CurrentDevice.IdentifierForVendor.ToString();
            if (UserDataModel.Instance.DeviceId == null || UserDataModel.Instance.DeviceId != devId)
                UserDataModel.Instance.DeviceId = devId;

            //位置情報利用の許可
            LocationManager.GetInstance().RequestAlwaysAuthorization();
            //プッシュ通知の許可
            RegisterForNotifications();

            return base.FinishedLaunching(app, options);
        }
    }

    public class FcmDelegate : MessagingDelegate
    {
        public override void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            System.Diagnostics.Debug.WriteLine("Refreshed token: " + fcmToken);
            SendRegistrationToServer(fcmToken);
        }

        private void SendRegistrationToServer(string token)
        {
            IDbAdapter dbAdapter = new DbAdapter_iOS();
            dbAdapter.UpdateDeviceInfo(fcmToken: token);
        }
    }

    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {

        }
    }
}
