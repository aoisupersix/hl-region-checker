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

        /// <summary>
        /// プッシュ通知を送信します。
        /// </summary>
        /// <param name="title">通知タイトル</param>
        /// <param name="body">通知本文</param>
        private void SendNotification(string title, string body, string info)
        {
            var notificationManager = GetSystemService(NotificationService) as NotificationManager;
            var NOTIFICATION_CHANNEL_ID = "my_channel_id_01";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, "Update notification of occupancy status", NotificationImportance.High);

                // Configure the notification channel.
                notificationChannel.Description = "Update notification of occupancy status";
                notificationChannel.EnableLights(true);
                notificationChannel.LightColor = Color.Red;
                //notificationChannel.SetVibrationPattern(new long[]{ 0, 1000, 500, 1000});
                //notificationChannel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(notificationChannel);
            }


            var notificationBuilder = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID);

            notificationBuilder.SetAutoCancel(true)
                .SetDefaults(1)
                .SetWhen((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds) //タイムスタンプ
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetTicker(body)
                //.SetPriority(Notification.PRIORITY_MAX)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetContentInfo(info);

            notificationManager.Notify(/*notification id*/1, notificationBuilder.Build());
        }

        /// <summary>
        /// BeaconManagerの初期化処理
        /// </summary>
        private void InitBeaconManager()
        {
            _beaconManager = BeaconManager.GetInstanceForApplication(this);
            _beaconManager.BeaconParsers.Clear();
            _beaconManager.BeaconParsers.Add(new BeaconParser().SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24"));
            _beaconManager.SetEnableScheduledScanJobs(true);
        }

        /// <summary>
        /// Beacon領域の初期化処理
        /// </summary>
        private void InitBeaconRegion()
        {
            var uuid = Identifier.Parse(RegionConst.BEACON_UUID);
            var major = Identifier.Parse(RegionConst.BEACON_MAJOR.ToString());
            var minor = Identifier.Parse(RegionConst.BEACON_MINOR.ToString());
            var region = new Org.Altbeacon.Beacon.Region(RegionConst.GetRegionIdentifier(RegionConst.Region.研究室), uuid, major, minor);
            _regionBootstrap = new RegionBootstrap(this, region);
        }

        // このコンストラクタを明示的に override する必要があるらしい
        public BackgroundMonitoringBeaconApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        /// <summary>
        /// 生成時の処理
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();

            InitBeaconManager();
            InitBeaconRegion();
            _backgroundPowerSaver = new BackgroundPowerSaver(this);
        }

        /// <summary>
        /// ビーコンのステータス更新時のコールバック
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        public void DidDetermineStateForRegion(int p0, Org.Altbeacon.Beacon.Region p1)
        {
        }

        /// <summary>
        /// ビーコン領域に侵入した際のコールバック
        /// </summary>
        /// <param name="p0"></param>
        public void DidEnterRegion(Org.Altbeacon.Beacon.Region p0)
        {
            Console.WriteLine("Enter [{0}] Region", p0.UniqueId);

            if(p0.UniqueId.Equals(RegionConst.GetRegionIdentifier(RegionConst.Region.研究室)))
            {
                //研究室に侵入
                SendNotification("研究室領域に侵入", "ステータスを「在室」に更新しました。", "ステータス自動更新");
            }
        }

        /// <summary>
        /// ビーコン領域から退出した際のコールバック
        /// </summary>
        /// <param name="p0"></param>
        public void DidExitRegion(Org.Altbeacon.Beacon.Region p0)
        {
            Console.WriteLine("Exit [{0}] Region", p0.UniqueId);

            if (p0.UniqueId.Equals(RegionConst.GetRegionIdentifier(RegionConst.Region.研究室)))
            {
                //研究室から退出
                SendNotification("研究室領域から退出", "ステータスを「学内」に更新しました。", "ステータス自動更新");
            }
        }
    }
}