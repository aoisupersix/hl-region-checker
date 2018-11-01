using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Gms.Location;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using HLRegionChecker.Const;

namespace HLRegionChecker.Droid.Geofences
{
    
    /// <summary>
    /// ジオフェンスの登録を行う
    /// </summary>
    public class RegisterGeofences
    {
        #region メンバ
        protected string TAG = typeof(RegisterGeofences).Name;

        /// <summary>
        /// ジオフェンスの初期トリガー打ち消し用
        /// </summary>
        public const int NO_INITIAL_TRIGGER = 0;

        /// <summary>
        /// GoogleAPIClient
        /// </summary>
        private GeofencingClient _geofencingClient;

        /// <summary>
        /// AppContext
        /// </summary>
        private Context _context;

        public const string PROPERTY_KEY_LOCATION_UPDATES_REQUESTED = "location-updates-requested";
        #endregion

        #region プロパティ
        /// <summary>
        /// ジオフェンスが追加されているか？
        /// </summary>
        public static bool GeofenceAdded
        {
            get
            {
                if (Xamarin.Forms.Application.Current.Properties.ContainsKey(PROPERTY_KEY_LOCATION_UPDATES_REQUESTED))
                    return Xamarin.Forms.Application.Current.Properties[PROPERTY_KEY_LOCATION_UPDATES_REQUESTED] is bool;
                return false;
            }
            set
            {
                Xamarin.Forms.Application.Current.Properties[PROPERTY_KEY_LOCATION_UPDATES_REQUESTED] = value;
                Xamarin.Forms.Application.Current.SavePropertiesAsync();
            }
        }

        /// <summary>
        /// 登録するジオフェンスリスト
        /// </summary>
        public IList<IGeofence> Geofences { get; set; }

        /// <summary>
        /// ジオフェンスのコールバッククラス
        /// </summary>
        public PendingIntent GeofencePendingIntent { get; set; }

        /// <summary>
        /// ジオフェンス登録のコールバック
        /// </summary>
        public IOnCompleteListener GeofencesRegisterCompleteListener { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// クラスをインスタンス化します。
        /// </summary>
        /// <param name="context">AppContext</param>
        public RegisterGeofences(Context context)
        {
            _context = context;
            _geofencingClient = LocationServices.GetGeofencingClient(context);
            PopulateGeofenceList();
        }

        /// <summary>
        /// 登録後のリスナーを指定してクラスをインスタンス化します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="completeListener"></param>
        public RegisterGeofences(Context context, IOnCompleteListener completeListener)
        {
            _context = context;
            _geofencingClient = LocationServices.GetGeofencingClient(context);
            GeofencesRegisterCompleteListener = completeListener;
            PopulateGeofenceList();
        }
        #endregion

        /// <summary>
        /// ジオフェンスを追加します。
        /// </summary>
        public void AddGeofences()
        {
            if (GeofencesRegisterCompleteListener != null)
            {
                _geofencingClient.AddGeofences(GetGeofencingRequest(), GetGeofencePendingIntent())
                    .AddOnCompleteListener(GeofencesRegisterCompleteListener);
            }
            else
            {
                _geofencingClient.AddGeofences(GetGeofencingRequest(), GetGeofencePendingIntent());
            }
        }

        /// <summary>
        /// GeofencingRequestを生成して返します。
        /// </summary>
        /// <returns>The geofencing request.</returns>
        private GeofencingRequest GetGeofencingRequest()
        {
            GeofencingRequest.Builder builder = new GeofencingRequest.Builder();
            //builder.SetInitialTrigger(NO_INITIAL_TRIGGER);
            builder.SetInitialTrigger(GeofencingRequest.InitialTriggerEnter);
            builder.AddGeofences(Geofences);
            return builder.Build();
        }

        /// <summary>
        /// GeofencePendingIntentを生成して返します。
        /// </summary>
        /// <returns>The geofence pending intent.</returns>
        private PendingIntent GetGeofencePendingIntent()
        {
            if (GeofencePendingIntent == null)
            {
                //指定されてないのでとりあえずPendingIntentを作る
                var intent = new Intent(_context, typeof(Geofences.GeofenceTransitionsIntentService));
                intent.SetAction("org.hykwlab.hlregionchecker_droid.geofence.ACTION_RECEIVE_GEOFENCE");
                _context.SendBroadcast(intent);
                GeofencePendingIntent = PendingIntent.GetBroadcast(_context, 0, intent, PendingIntentFlags.UpdateCurrent);
            }

            return GeofencePendingIntent;
        }

        /// <summary>
        /// ジオフェンスのリストを設定します。
        /// </summary>
        private void PopulateGeofenceList()
        {
            Geofences = new List<IGeofence>();

            foreach(var region in Regions.RegionList.CampusAllRegions)
            {
                Geofences.Add(new GeofenceBuilder()
                    .SetRequestId(region.Identifier)
                    .SetCircularRegion(
                        region.Latitude,
                        region.Longitude,
                        (float)region.Radius
                    )
                    .SetExpirationDuration(Geofence.NeverExpire)
                    .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
                    .Build());
            }
        }
    }
}