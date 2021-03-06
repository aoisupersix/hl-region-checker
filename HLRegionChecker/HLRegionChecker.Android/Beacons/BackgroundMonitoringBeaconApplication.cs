﻿using System;
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

            if (UserDataModel.Instance.IsUseForegroundService) /* フォアグラウンドサービス */
            {
                var intent = new Intent(this, typeof(MainActivity));
                PendingIntent pendingIntent = PendingIntent.GetActivity(
                    this, 0, intent, PendingIntentFlags.UpdateCurrent
                );

                var notificationBuilder = new NotificationCompat.Builder(this, NotificationUtil.SERVICE_NOTIFICATION_CHANNEL_ID);
                notificationBuilder.SetAutoCancel(true)
                    .SetDefaults(1)
                    .SetWhen((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds) //タイムスタンプ
                    .SetSmallIcon(Resource.Mipmap.appicon)
                    .SetContentTitle("Scanning for Beacons")
                    .SetContentIntent(pendingIntent);

                _beaconManager.EnableForegroundServiceScanning(notificationBuilder.Build(), 456);
                _beaconManager.BackgroundBetweenScanPeriod = 1000;
                _beaconManager.BackgroundScanPeriod = 1100;
                _beaconManager.SetEnableScheduledScanJobs(false);
            }
            else /* スキャンジョブ */
            {
                _beaconManager.SetEnableScheduledScanJobs(true);
            }
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
            FirebaseDatabase.Instance.SetPersistenceEnabled(true); //オフライン機能の有効化

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
            var adapter = new DbAdapter_Droid();
            adapter.AddDeviceLog("ビーコンに侵入：在室状況を[在室]に更新", p0.UniqueId);

            if (p0.UniqueId.Equals(Regions.RegionList.研究室.Identifier))
            {
                //研究室に侵入
                var dbAdapter = new DbAdapter_Droid();
                dbAdapter.UpdateStatus(UserDataModel.Instance.MemberId, Status.在室.GetStatusId(), true);

                //var intent = new Intent(this, typeof(Beacons.BeaconMonitoringService));
                //StartForegroundService(intent);
            }
        }

        /// <summary>
        /// ビーコン領域から退出した際のコールバック
        /// </summary>
        /// <param name="p0"></param>
        public void DidExitRegion(Org.Altbeacon.Beacon.Region p0)
        {
            Log.Info(TAG, "Exit [{0}] Region", p0.UniqueId);
            var adapter = new DbAdapter_Droid();
            adapter.AddDeviceLog("ビーコンから退出：在室状況を[学内]に更新", p0.UniqueId);

            if (p0.UniqueId.Equals(Regions.RegionList.研究室.Identifier))
            {
                //研究室から退出
                var dbAdapter = new DbAdapter_Droid();
                dbAdapter.UpdateStatus(UserDataModel.Instance.MemberId, Status.学内.GetStatusId(), true);
            }
        }
    }
}