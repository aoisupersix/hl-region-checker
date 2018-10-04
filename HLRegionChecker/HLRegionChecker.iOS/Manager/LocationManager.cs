using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Foundation;
using CoreLocation;

using HLRegionChecker.Const;
using HLRegionChecker.iOS.DependencyServices;
using HLRegionChecker.Models;
using HLRegionChecker.Interfaces;
using HLRegionChecker.iOS.Notification;
using Firebase.Database;
using HLRegionChecker.Regions;

namespace HLRegionChecker.iOS.Manager
{
    /// <summary>
    /// 位置情報関連の処理を行うクラス
    /// </summary>
    public sealed class LocationManager : CLLocationManager
    {
        #region メンバ
        /// <summary>
        /// クラスのインスタンス
        /// </summary>
        private static readonly LocationManager _instance = new LocationManager();

        /// <summary>
        /// HLRegionCheckerで利用するビーコンのUUID
        /// </summary>
        public static readonly NSUuid BEACON_UUID = new NSUuid(RegionConst.BEACON_UUID);

        /// <summary>
        /// 工学部棟の中心緯度/経度
        /// </summary>
        public static readonly CLLocationCoordinate2D CAMPUS_CENTER_COORDINATE = new CLLocationCoordinate2D(RegionConst.CAMPUS_LATITUDE, RegionConst.CAMPUS_LONGITUDE);

        /// <summary>
        /// 研究室のビーコン領域
        /// </summary>
        public static readonly CLBeaconRegion REGION_LABORATORY = new CLBeaconRegion(BEACON_UUID, RegionConst.BEACON_MAJOR, RegionConst.BEACON_MINOR, Region.研究室.GetIdentifier());

        public static readonly CLCircularRegion REGION_CAMPUS = new CLCircularRegion(CAMPUS_CENTER_COORDINATE, RegionConst.CAMPUS_RADIUS, Region.学内.GetIdentifier());
        #endregion

        #region メソッド
        /// <summary>
        /// LocationManagerのインスタンスを取得します。
        /// </summary>
        /// <returns>LocationManagerのインスタンス</returns>
        public static LocationManager GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// 位置情報利用の許可を要求します。
        /// </summary>
        public override void RequestAlwaysAuthorization()
        {
            _instance.AllowsBackgroundLocationUpdates = true;
            _instance.Delegate = new LocationDelegate();
            base.RequestAlwaysAuthorization();
        }

        #endregion
    }

    public class LocationDelegate : CLLocationManagerDelegate
    {
        /// <summary>
        /// 研究室のビーコン領域定義
        /// </summary>
        public CLBeaconRegion 研究室領域;

        /// <summary>
        /// 学内のジオフェンス領域定義
        /// </summary>
        public IEnumerable<CLCircularRegion> 学内領域;

        /// <summary>
        /// ステータス情報を更新します。
        /// </summary>
        /// <param name="stateId">更新するステータスID</param>
        private void UpdateStatus(int stateId)
        {
            var memId = UserDataModel.Instance.MemberId;
            if (memId == UserDataModel.DefaultMemberId)
                return;

            //ステータスの更新処理
            var adapter = new DbAdapter_iOS();
            adapter.UpdateStatus(memId, stateId, true);
        }

        public LocationDelegate()
        {
            // 領域定義の初期化
            研究室領域 = new CLBeaconRegion(new NSUuid(RegionList.研究室.Uuid), (ushort)RegionList.研究室.Major, (ushort)RegionList.研究室.Minor, RegionList.研究室.Identifier);
            学内領域 = RegionList.CampusAllRegions
                .Select(r => new CLCircularRegion(new CLLocationCoordinate2D(r.Latitude, r.Longitude), r.Radius, r.Identifier));
        }

        /// <summary>
        /// 位置情報利用の認証状態が変わった際に、位置情報のモニタリングを開始します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="status"></param>
        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            if (status == CLAuthorizationStatus.AuthorizedAlways || status == CLAuthorizationStatus.AuthorizedWhenInUse)
            {
                //iBeacon領域判定の有効化
                if (CLLocationManager.IsMonitoringAvailable(typeof(CLBeaconRegion)))
                {
                    研究室領域.NotifyEntryStateOnDisplay = false;
                    研究室領域.NotifyOnEntry = true;
                    研究室領域.NotifyOnExit = true;

                    manager.StartMonitoring(研究室領域);
                }

                //ジオフェンス領域の有効化
                if (CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
                {
                    foreach(var gr in 学内領域)
                    {
                        gr.NotifyOnEntry = true;
                        gr.NotifyOnExit = true;
                        manager.StartMonitoring(gr);
                    }
                }
            }
            else
            {
                //位置情報利用の許可を貰う
                manager.RequestAlwaysAuthorization();
            }
        }

        /// <summary>
        /// 位置情報の観測が開始された際に、利用の認証を要求します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="region"></param>
        public override void DidStartMonitoringForRegion(CLLocationManager manager, CLRegion region)
        {
            Console.WriteLine("Start monitoring for {0}", region.Identifier);
            manager.RequestState(region);
        }

        /// <summary>
        /// 領域に侵入した際にステータスを更新します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="region"></param>
        public override void RegionEntered(CLLocationManager manager, CLRegion region)
        {
            Console.WriteLine("Enter [{0}] Region", region.Identifier);

            if (region.Identifier.Equals(RegionList.研究室.Identifier))
            {
                //研究室領域に侵入
                UpdateStatus(Status.在室.GetStatusId());
                PushNotificationManager.Send("研究室領域に侵入", "ステータスを「在室」に更新しました。"); // TODO: プッシュ通知対応後に消す
            }
            else
            {
                //学内領域に侵入
                var gregion = RegionList.CampusAllRegions.Where(r => r.Identifier.Equals(region.Identifier)).First();
                var dbAdapter = new DbAdapter_iOS();
                dbAdapter.UpdateGeofenceStatus(UserDataModel.Instance.DeviceId, gregion.DbIdentifierName, true);
                PushNotificationManager.Send($"学内領域({gregion.DbIdentifierName})に侵入", "ジオフェンスステータスを更新しました。"); // TODO: プッシュ通知対応後に消す
            }
        }

        /// <summary>
        /// 領域から出た際にステータスを更新します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="region"></param>
        public override void RegionLeft(CLLocationManager manager, CLRegion region)
        {
            Console.WriteLine("Exit [{0}] Region", region.Identifier);

            if (region.Identifier.Equals(RegionList.研究室.Identifier))
            {
                //研究室領域から退出
                UpdateStatus(Status.学内.GetStatusId());
                PushNotificationManager.Send("研究室領域から退出", "ステータスを「学内」に更新しました。"); // TODO: プッシュ通知対応後に消す
            }
            else
            {
                //学内領域から退出
                var gregion = RegionList.CampusAllRegions.Where(r => r.Identifier.Equals(region.Identifier)).First();
                var dbAdapter = new DbAdapter_iOS();
                dbAdapter.UpdateGeofenceStatus(UserDataModel.Instance.DeviceId, gregion.DbIdentifierName, false);
                PushNotificationManager.Send($"学内領域({gregion.DbIdentifierName})から退出", "ジオフェンスステータスを更新しました。"); // TODO: プッシュ通知対応後に消す
            }
        }
    }
}