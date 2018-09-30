using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Firebase;
using Firebase.Database;
using HLRegionChecker.Const;
using HLRegionChecker.Droid.DependencyServices;
using HLRegionChecker.Droid.Notification;
using Org.Altbeacon.Beacon;
using Org.Altbeacon.Beacon.Powersave;
using Org.Altbeacon.Beacon.Startup;
using HLRegionChecker.Interfaces;
using HLRegionChecker.Models;

namespace HLRegionChecker.Droid
{
    [Application]
    /// <summary>
    /// iBeaconのモニタリングを行うアプリケーションクラス
    /// </summary>
    public class BackgroundMonitoringBeaconApplication: Application, IBootstrapNotifier
    {
        #region メンバ
        protected string TAG = typeof(BackgroundMonitoringBeaconApplication).Name;

        private BeaconManager _beaconManager;
        private RegionBootstrap _regionBootstrap;
        private BackgroundPowerSaver _backgroundPowerSaver;
        #endregion

        /// <summary>
        /// BeaconManagerの初期化処理
        /// </summary>
        private void InitBeaconManager()
        {
            _beaconManager = BeaconManager.GetInstanceForApplication(this);
            _beaconManager.BeaconParsers.Clear();
            _beaconManager.BeaconParsers.Add(new BeaconParser().SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24"));

            // 以下フォアグラウンドサービス用
            //Android.App.Notification.Builder builder = new Android.App.Notification.Builder(this);
            //builder.SetSmallIcon(Resource.Mipmap.appicon);
            //builder.SetContentTitle("Scanning for Beacons");
            //var intent = new Intent(this, typeof(MainActivity));
            //PendingIntent pendingIntent = PendingIntent.GetActivity(
            //    this, 0, intent, PendingIntentFlags.UpdateCurrent
            //);
            //builder.SetContentIntent(pendingIntent);
            //_beaconManager.EnableForegroundServiceScanning(builder.Build(), 456);
            //_beaconManager.SetEnableScheduledScanJobs(false);

            // スケジューラ用
            _beaconManager.SetEnableScheduledScanJobs(true);
        }

        /// <summary>
        /// Beacon領域の初期化処理
        /// </summary>
        private void InitBeaconRegion()
        {
            var uuid = Identifier.Parse(Regions.RegionList.研究室.Uuid);
            var major = Identifier.Parse(Regions.RegionList.研究室.Major.ToString());
            var minor = Identifier.Parse(Regions.RegionList.研究室.Minor.ToString());
            var region = new Org.Altbeacon.Beacon.Region(Regions.RegionList.研究室.Identifier, uuid, major, minor);
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

            //通知チャンネル登録
            NotificationUtil.Instance.CreateNotificationChannel((NotificationManager)GetSystemService(NotificationService), this);

            //Firebase初期化
            FirebaseApp.InitializeApp(this);
            var db = FirebaseDatabase.Instance;

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
            Log.Info(TAG, "Enter [{0}] Region", p0.UniqueId);
            Firebase.FirebaseApp.InitializeApp(this.ApplicationContext);

            if (p0.UniqueId.Equals(Regions.RegionList.研究室.Identifier))
            {
                //研究室に侵入
                var dbAdapter = new DbAdapter_Droid();
                dbAdapter.UpdateStatus(UserDataModel.Instance.MemberId, Status.在室.GetStatusId(), true);
            }
        }

        /// <summary>
        /// ビーコン領域から退出した際のコールバック
        /// </summary>
        /// <param name="p0"></param>
        public void DidExitRegion(Org.Altbeacon.Beacon.Region p0)
        {
            Log.Info(TAG, "Exit [{0}] Region", p0.UniqueId);

            if (p0.UniqueId.Equals(Regions.RegionList.研究室.Identifier))
            {
                //研究室から退出
                var dbAdapter = new DbAdapter_Droid();
                dbAdapter.UpdateStatus(UserDataModel.Instance.MemberId, Status.学内.GetStatusId(), true);
            }
        }
    }
}