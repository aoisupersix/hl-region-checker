using System;
using System.ComponentModel;
using System.Reflection;
using Foundation;
using CoreLocation;

using HLRegionChecker.iOS.DependencyServices;
using HLRegionChecker.Models;
using HLRegionChecker.iOS.Notification;

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
        /// 仮想領域の識別子
        /// </summary>
        public enum Region
        {
            [Description("tokyo.aoisupersix.region-laboratory")]
            研究室,
            [Description("tokyo.aoisupersix.region-campus")]
            学内,
        }

        /// <summary>
        /// HLRegionCheckerで利用するビーコンのUUID
        /// </summary>
        public static readonly NSUuid BEACON_UUID = new NSUuid("2F0B0D9B-B52C-47BF-B5B8-2BFBCE094653");

        /// <summary>
        /// 工学部棟の中心緯度/経度
        /// </summary>
        public static readonly CLLocationCoordinate2D CAMPUS_CENTER_COORDINATE = new CLLocationCoordinate2D(35.626514, 139.279283);

        /// <summary>
        /// 研究室のビーコン領域
        /// </summary>
        public static readonly CLBeaconRegion REGION_LABORATORY = new CLBeaconRegion(BEACON_UUID, 1, 1, GetRegionIdentifier(Region.研究室));

        public static readonly CLCircularRegion REGION_CAMPUS = new CLCircularRegion(CAMPUS_CENTER_COORDINATE, 400, GetRegionIdentifier(Region.学内));
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
        /// 仮想領域の識別子をアトリビュートから取得します。
        /// </summary>
        /// <param name="val">仮想領域</param>
        /// <returns>仮想領域の識別子</returns>
        public static string GetRegionIdentifier(Region val)
        {
            FieldInfo fi = val.GetType().GetField(val.ToString());
            var attribute = (DescriptionAttribute)fi.GetCustomAttribute(typeof(DescriptionAttribute), false);
            if (attribute != null)
                return attribute.Description;
            return val.ToString();
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
            if (memId == null)
                return;

            //ステータスの更新処理
            var adapter = (IDbAdapter)(new DbAdapter_iOS());
            adapter.UpdateStatus(memId.Value, stateId);
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
            PushNotificationManager.Send("Log表示", "Enter " + region.Identifier.GetType() + " Region");

            if (region.Identifier == LocationManager.GetRegionIdentifier(LocationManager.Region.研究室))
            {
                //研究室領域に侵入
                UpdateStatus(2);
                PushNotificationManager.Send("研究室領域に侵入", "ステータスを「在室」に更新しました。");
            }
            else if (region.Identifier == LocationManager.GetRegionIdentifier(LocationManager.Region.学内))
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
            PushNotificationManager.Send("Log表示", "Exit " + region.Identifier.GetType() + " Region");

            if (region.Identifier == LocationManager.GetRegionIdentifier(LocationManager.Region.研究室))
            {
                //研究室領域から退出
                UpdateStatus(1);
                PushNotificationManager.Send("研究室領域から退出", "ステータスを「学内」に更新しました。");
            }
            else if (region.Identifier == LocationManager.GetRegionIdentifier(LocationManager.Region.学内))
            {
                //学内領域から退出
                UpdateStatus(0);
                PushNotificationManager.Send("学内領域から退出", "ステータスを「帰宅」に更新しました。");
            }
        }
    }
}