using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using CoreLocation;

using HLRegionChecker.Const;
using HLRegionChecker.iOS.DependencyServices;
using HLRegionChecker.Models;
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
        /// 位置情報のログを送信します。
        /// </summary>
        /// <param name="location"></param>
        private void AddLocationLog(CLLocation location)
        {
            var adapter = new DbAdapter_iOS();

            var formatter = new NSDateFormatter();
            formatter.DateFormat = "yyyy-MM-dd HH:mm:ss";
            formatter.TimeZone = NSTimeZone.SystemTimeZone;
            var dateString = formatter.StringFor(location.Timestamp);
            adapter.AddDeviceLog($"位置情報取得：{dateString}", $"lat:{ location.Coordinate.Latitude},lng: { location.Coordinate.Longitude}");
        }

        /// <summary>
        /// 位置情報利用の認証状態が変わった際に、位置情報のモニタリングを開始します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="status"></param>
        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            var adapter = new DbAdapter_iOS();
            adapter.AddDeviceLog($"位置情報の認証状態が更新", status.ToString()); // TODO ステータス名表示に

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
            var adapter = new DbAdapter_iOS();
            if (region.Identifier.Equals(RegionList.研究室.Identifier))
            {
                //研究室領域に侵入
                UpdateStatus(Status.在室.GetStatusId());
                adapter.AddDeviceLog("在室状況を「在室」に更新", $"領域[{RegionList.研究室.Name}]に侵入");
            }
            else
            {
                //学内領域に侵入
                var gregion = RegionList.CampusAllRegions.Where(r => r.Identifier.Equals(region.Identifier)).First();
                var dbAdapter = new DbAdapter_iOS();
                dbAdapter.UpdateGeofenceStatus(UserDataModel.Instance.DeviceId, gregion.DbIdentifierName, true);
                adapter.AddDeviceLog("ジオフェンス状態を更新", $"領域[{gregion.Name}]に侵入");
                AddLocationLog(manager.Location);
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
            var adapter = new DbAdapter_iOS();

            if (region.Identifier.Equals(RegionList.研究室.Identifier))
            {
                //研究室領域から退出
                UpdateStatus(Status.学内.GetStatusId());
                adapter.AddDeviceLog("在室状況を「学内」に更新", $"領域[{RegionList.研究室.Name}]から退出");
            }
            else
            {
                //学内領域から退出
                var gregion = RegionList.CampusAllRegions.Where(r => r.Identifier.Equals(region.Identifier)).First();
                adapter.UpdateGeofenceStatus(UserDataModel.Instance.DeviceId, gregion.DbIdentifierName, false);
                adapter.AddDeviceLog("ジオフェンス状態を更新", $"領域[{gregion.Name}]から退出");
                AddLocationLog(manager.Location);
            }
        }

        /// <summary>
        /// 位置情報取得失敗時にログを送信します。
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="error"></param>
        public override void Failed(CLLocationManager manager, NSError error)
        {
            var adapter = new DbAdapter_iOS();
            adapter.AddDeviceLog($"位置情報の取得に失敗", error.ToString());
        }
    }
}