using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using HLRegionChecker.Const;

namespace HLRegionChecker.Droid.Geofences
{
    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    /// <summary>
    /// ブート時にジオフェンスを登録するブロードキャストレシーバ
    /// </summary>
    public class BootReceiver : BroadcastReceiver, IOnCompleteListener
    {
        #region メンバ
        protected string TAG = typeof(BootReceiver).Name + "hykwlabtest";

        /// <summary>
        /// ジオフェンスの初期トリガー打ち消し用
        /// </summary>
        public int NO_INITIAL_TRIGGER = 0;

        private GeofencingClient mGeofencingClient;
        private IList<IGeofence> mGeofenceList;
        private PendingIntent mGeofencePendingIntent;
        private Android.Content.Context mContext;
        #endregion

        /// <summary>
        /// GeofencingRequestを生成して返します。
        /// </summary>
        /// <returns>The geofencing request.</returns>
        GeofencingRequest GetGeofencingRequest()
        {
            GeofencingRequest.Builder builder = new GeofencingRequest.Builder();
            builder.SetInitialTrigger(NO_INITIAL_TRIGGER);
            //builder.SetInitialTrigger(GeofencingRequest.InitialTriggerEnter);
            builder.AddGeofences(mGeofenceList);
            return builder.Build();
        }

        /// <summary>
        /// GeofencePendingIntentを生成して返します。
        /// </summary>
        /// <returns>The geofence pending intent.</returns>
        PendingIntent GetGeofencePendingIntent()
        {
            // Reuse the PendingIntent if we already have it.
            if (mGeofencePendingIntent != null)
            {
                return mGeofencePendingIntent;
            }
            var intent = new Intent(mContext, typeof(Geofences.GeofenceTransitionsIntentService));
            intent.SetAction("org.hykwlab.hlregionchecker_droid.geofence.ACTION_RECEIVE_GEOFENCE");
            mContext.SendBroadcast(intent);
            return PendingIntent.GetBroadcast(mContext, 0, intent, PendingIntentFlags.UpdateCurrent);
        }

        /// <summary>
        /// ジオフェンスのリストを設定します。
        /// </summary>
        void PopulateGeofenceList()
        {
            mGeofenceList.Add(new GeofenceBuilder()
                .SetRequestId(Region.学内.GetIdentifier())
                .SetCircularRegion(
                    35.817187,
                    139.424551,
                    200
                )
                .SetExpirationDuration(Geofence.NeverExpire)
                .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
                .Build());
        }

        /// <summary>
        /// ジオフェンスを追加します。
        /// </summary>
        void AddGeofences()
        {
            mGeofencingClient.AddGeofences(GetGeofencingRequest(), GetGeofencePendingIntent())
                .AddOnCompleteListener(this);
        }

        /// <summary>
        /// レシーバ起動
        /// </summary>
        /// <param name="context"></param>
        /// <param name="intent"></param>
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Boot intent received.");

            //ジオフェンスの初期化
            mContext = context;
            mGeofenceList = new List<IGeofence>();
            mGeofencePendingIntent = null;
            PopulateGeofenceList();
            mGeofencingClient = LocationServices.GetGeofencingClient(context);

            AddGeofences();
        }

        /// <summary>
        /// ジオフェンスの追加完了コールバック
        /// </summary>
        /// <param name="task"></param>
        public void OnComplete(Task task)
        {
            if (task.IsSuccessful)
            {
                var message = mContext.GetString(Resource.String.complete_add_geofence);
                Log.Info(TAG, message);
            }
            else
            {
                // Get the status code for the error and log it using a user-friendly message.
                var errorMessage = Geofences.GeofenceErrorMessages.GetErrorString(mContext, task.Exception);
                Log.Info(TAG, errorMessage);
            }
        }
    }
}