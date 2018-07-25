using System;
using Android.Content;
using Xamarin.Forms;

namespace HLRegionChecker.Droid.Geofences
{
    /// <summary>
    /// 位置情報の取得状態を保持するクラス
    /// </summary>
    public class LocationRequestModel
    {
        public const string PROPERTY_KEY_LOCATION_UPDATES_REQUESTED = "location-updates-requested";

        /// <summary>
        /// インスタンス
        /// </summary>
        private static LocationRequestModel _instance;

        /// <summary>
        /// 位置情報の取得状態
        /// </summary>
        /// <value><c>true</c> if location requesting; otherwise, <c>false</c>.</value>
        public bool? LocationRequesting
        {
            get
            {
                if (Application.Current.Properties.ContainsKey(PROPERTY_KEY_LOCATION_UPDATES_REQUESTED))
                    return Application.Current.Properties[PROPERTY_KEY_LOCATION_UPDATES_REQUESTED] as bool?;
                return null;
            }
            set
            {
                Application.Current.Properties[PROPERTY_KEY_LOCATION_UPDATES_REQUESTED] = value;
                Application.Current.SavePropertiesAsync();
            }
        }

        /// <summary>
        /// クラスのインスタンスを取得します。
        /// </summary>
        public static LocationRequestModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LocationRequestModel();

                return _instance;
            }
        }
    }
}
