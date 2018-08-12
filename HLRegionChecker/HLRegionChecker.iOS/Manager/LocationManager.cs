using System;
using System.ComponentModel;
using System.Reflection;
using Foundation;
using CoreLocation;

using HLRegionChecker.Const;
using HLRegionChecker.iOS.DependencyServices;
using HLRegionChecker.Models;
using HLRegionChecker.iOS.Notification;
using Firebase.Database;

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
        public static readonly CLBeaconRegion REGION_LABORATORY = new CLBeaconRegion(BEACON_UUID, RegionConst.BEACON_MAJOR, RegionConst.BEACON_MINOR, RegionConst.GetRegionIdentifier(RegionConst.Region.研究室));

        public static readonly CLCircularRegion REGION_CAMPUS = new CLCircularRegion(CAMPUS_CENTER_COORDINATE, RegionConst.CAMPUS_RADIUS, RegionConst.GetRegionIdentifier(RegionConst.Region.学内));
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
        /// ステータス情報を更新します。
        /// </summary>
        /// <param name="stateId">更新するステータスID</param>
        private void UpdateStatus(int stateId)
        {
            var memId = UserDataModel.Instance.MemberId;
            if (memId == UserDataModel.DefaultMemberId)
                return;

            //ステータスの更新処理
            //var childDict = new NSDictionary("status", stateId);

            //var rootRef = Database.DefaultInstance.GetRootReference();
            //var memRef = rootRef.GetChild("members");
            //memRef.GetChild(memId.Value.ToString()).UpdateChildValues(childDict);
            var adapter = (IDbAdapter)(new DbAdapter_iOS());
            adapter.UpdateStatus(memId, stateId, true);
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
                    LocationManager.REGION_LABORATORY.NotifyEntryStateOnDisplay = false;
                    LocationManager.REGION_LABORATORY.NotifyOnEntry = true;
                    LocationManager.REGION_LABORATORY.NotifyOnExit = true;

                    manager.StartMonitoring(LocationManager.REGION_LABORATORY);
                }

                //ジオフェンス領域の有効化
                if (CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
                {
                    LocationManager.REGION_CAMPUS.NotifyOnEntry = true;
                    LocationManager.REGION_CAMPUS.NotifyOnExit = true;
                    manager.StartMonitoring(LocationManager.REGION_CAMPUS);
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

            if (region.Identifier == RegionConst.GetRegionIdentifier(RegionConst.Region.研究室))
            {
                //研究室領域に侵入
                UpdateStatus(2);
                PushNotificationManager.Send("研究室領域に侵入", "ステータスを「在室」に更新しました。");
            }
            else if (region.Identifier == RegionConst.GetRegionIdentifier(RegionConst.Region.学内))
            {
                //学内領域に侵入
                UpdateStatus(1);
                PushNotificationManager.Send("学内領域に侵入", "ステータスを「学内」に更新しました。");
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

            if (region.Identifier == RegionConst.GetRegionIdentifier(RegionConst.Region.研究室))
            {
                //研究室領域から退出
                UpdateStatus(1);
                PushNotificationManager.Send("研究室領域から退出", "ステータスを「学内」に更新しました。");
            }
            else if (region.Identifier == RegionConst.GetRegionIdentifier(RegionConst.Region.学内))
            {
                //学内領域から退出
                UpdateStatus(0);
                PushNotificationManager.Send("学内領域から退出", "ステータスを「帰宅」に更新しました。");
            }
        }
    }
}