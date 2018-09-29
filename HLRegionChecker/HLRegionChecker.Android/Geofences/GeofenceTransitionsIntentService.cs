using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Locations;
using Android.Support.V4.App;
using Java.Lang;
using Android.Util;
using HLRegionChecker.Models;
using Android.Support.V4.Content;
using Android.Content.PM;
using Android;
using Firebase.Database;
using HLRegionChecker.Const;
using HLRegionChecker.Droid.Notification;
using HLRegionChecker.Interfaces;
using HLRegionChecker.Droid.DependencyServices;

namespace HLRegionChecker.Droid.Geofences
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter( new[]{ "org.hykwlab.hlregionchecker_droid.geofence.ACTION_RECEIVE_GEOFENCE" })]
    /// <summary>
    /// ジオフェンスに侵入/退出した際の処理クラス
    /// </summary>
    public class GeofenceTransitionsIntentService : BroadcastReceiver
    {
        private const string TAG = "GeofenceTransitionsIS";

        private Android.Content.Context mContext;

        public GeofenceTransitionsIntentService()
        {
        }

        public override void OnReceive(Context context, Intent intent)
        {
            //NotificationUtil.Instance.CreateNotificationChannel((NotificationManager)context.GetSystemService(NotificationService), context);
            var geofencingEvent = GeofencingEvent.FromIntent(intent);
            mContext = context;
            if (geofencingEvent.HasError)
            {
                var errorMessage = GeofenceErrorMessages.GetErrorString(context, geofencingEvent.ErrorCode);
                Log.Error(TAG, errorMessage);
                NotificationUtil.Instance.SendNotification(context, "GeofenceError", "エラーです。", errorMessage);
                return;
            }

            int geofenceTransition = geofencingEvent.GeofenceTransition;

            if (geofenceTransition == Geofence.GeofenceTransitionEnter ||
                geofenceTransition == Geofence.GeofenceTransitionExit)
            {

                IList<IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;

                string geofenceTransitionDetails = GetGeofenceTransitionDetails(context, geofenceTransition, triggeringGeofences);

                Log.Info(TAG, geofenceTransitionDetails);

                if (geofenceTransition == Geofence.GeofenceTransitionEnter)
                {
                    NotificationUtil.Instance.SendNotification(context, "学内領域に侵入", "ステータスを「学内」に更新しました。", "ステータス自動更新");
                    var dbAdapter = new DbAdapter_Droid();
                    dbAdapter.UpdateStatus(UserDataModel.Instance.MemberId, Status.学内.GetStatusId(), true);
                }
                else
                {
                    NotificationUtil.Instance.SendNotification(context, "学内領域から退出", "ステータスを「帰宅」に更新しました。", "ステータス自動更新");
                    var dbAdapter = new DbAdapter_Droid();
                    dbAdapter.UpdateStatus(UserDataModel.Instance.MemberId, Status.帰宅.GetStatusId(), true);
                }
            }
            else
            {
                // Log the error.
                Log.Error(TAG, context.GetString(Resource.String.geofence_transition_invalid_type, new[] { new Java.Lang.Integer(geofenceTransition) }));
            }
        }

        string GetGeofenceTransitionDetails(Context context, int geofenceTransition, IList<IGeofence> triggeringGeofences)
        {
            string geofenceTransitionString = GetTransitionString(geofenceTransition);

            var triggeringGeofencesIdsList = new List<string>();
            foreach (IGeofence geofence in triggeringGeofences)
            {
                triggeringGeofencesIdsList.Add(geofence.RequestId);
            }
            var triggeringGeofencesIdsString = string.Join(", ", triggeringGeofencesIdsList);

            return geofenceTransitionString + ": " + triggeringGeofencesIdsString;
        }

        string GetTransitionString(int transitionType)
        {
            switch (transitionType)
            {
                case Geofence.GeofenceTransitionEnter:
                    return mContext.GetString(Resource.String.geofence_transition_entered);
                case Geofence.GeofenceTransitionExit:
                    return mContext.GetString(Resource.String.geofence_transition_exited);
                default:
                    return mContext.GetString(Resource.String.unknown_geofence_transition);
            }
        }
    }
}
