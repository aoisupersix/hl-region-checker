using System;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using HLRegionChecker.Const;
using Org.Altbeacon.Beacon;
using Org.Altbeacon.Beacon.Powersave;
using Org.Altbeacon.Beacon.Startup;

namespace HLRegionChecker.Droid
{
    [Application]
    /// <summary>
    /// iBeaconのモニタリングを行うアプリケーションクラス
    /// </summary>
    public class BackgroundMonitoringBeaconApplication: Application, IBootstrapNotifier
    {
        private BeaconManager _beaconManager;
        private RegionBootstrap _regionBootstrap;
        private BackgroundPowerSaver _backgroundPowerSaver;

        private void SendNotification(string title, string body, string info)
        {
            var notificationManager = GetSystemService(NotificationService) as NotificationManager;
            var NOTIFICATION_CHANNEL_ID = "my_channel_id_01";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, "My Notifications", NotificationImportance.High);

                // Configure the notification channel.
                notificationChannel.Description = "Channel description";
                notificationChannel.EnableLights(true);
                notificationChannel.LightColor = Color.Red;
                notificationChannel.SetVibrationPattern(new long[]{ 0, 1000, 500, 1000});
                notificationChannel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(notificationChannel);
            }


            var notificationBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID);

            notificationBuilder.SetAutoCancel(true)
                .SetDefaults(1)
                .SetWhen(100)
                //.SetSmallIcon(R.drawable.ic_launcher_background)
                .SetTicker("Hearty365")
                //.SetPriority(Notification.PRIORITY_MAX)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetContentInfo(info);

            notificationManager.Notify(/*notification id*/1, notificationBuilder.Build());
        }

        // このコンストラクタを明示的に override する必要があるらしい
        public BackgroundMonitoringBeaconApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            _beaconManager = BeaconManager.GetInstanceForApplication(this);
            _beaconManager.BeaconParsers.Clear();
            _beaconManager.BeaconParsers.Add(new BeaconParser().SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24"));

            var uuid = Identifier.Parse(RegionConst.BEACON_UUID);
            var major = Identifier.Parse(RegionConst.BEACON_MAJOR.ToString());
            var minor = Identifier.Parse(RegionConst.BEACON_MINOR.ToString());
            var region = new Org.Altbeacon.Beacon.Region("test", uuid, major, minor);
            _regionBootstrap = new RegionBootstrap(this, region);
            _backgroundPowerSaver = new BackgroundPowerSaver(this);
        }

        public void DidDetermineStateForRegion(int p0, Org.Altbeacon.Beacon.Region p1)
        {
        }

        public void DidEnterRegion(Org.Altbeacon.Beacon.Region p0)
        {
            SendNotification("EnterRegion", "領域に侵入", "test");
        }

        public void DidExitRegion(Org.Altbeacon.Beacon.Region p0)
        {
        }
    }
}