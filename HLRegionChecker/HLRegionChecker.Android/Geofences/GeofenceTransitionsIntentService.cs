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
            var geofencingEvent = GeofencingEvent.FromIntent(intent);
            var dbAdapter = new DbAdapter_Droid();
            mContext = context;
            if (geofencingEvent.HasError)
            {
                var errorMessage = GeofenceErrorMessages.GetErrorString(context, geofencingEvent.ErrorCode);
                Log.Error(TAG, errorMessage);
                NotificationUtil.Instance.SendNotification(context, NotificationUtil.STATUS_NOTIFICATION_CHANNEL_ID, "GeofenceError", "エラーです。", errorMessage);
                return;
            }

            int geofenceTransition = geofencingEvent.GeofenceTransition;

            if (geofenceTransition == Geofence.GeofenceTransitionEnter ||
                geofenceTransition == Geofence.GeofenceTransitionExit)
            {

                IList<IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;
                var updateGeofenceStatus = geofenceTransition == Geofence.GeofenceTransitionEnter;
                string geofenceTransitionDetails = GetGeofenceTransitionDetails(context, geofenceTransition, triggeringGeofences);
                Log.Info(TAG, geofenceTransitionDetails);

                var triggerRegions = triggeringGeofences
                    .Select(g => Regions.RegionList.GetRegionFromIdentifier(g.RequestId))
                    .Where(r => r != null)
                    .Select(r => (Regions.GeofenceRegion)r)
                    .ToList()
                    ;

                // 更新
                foreach (var region in triggerRegions)
                {
                    var statusText = geofenceTransition == Geofence.GeofenceTransitionEnter ? "侵入" : "退出";
                    dbAdapter.AddDeviceLog($"ジオフェンス[{region.DbIdentifierName}]の状態を[{statusText}]に更新");
                    dbAdapter.UpdateGeofenceStatus(UserDataModel.Instance.DeviceId, region.DbIdentifierName, updateGeofenceStatus);
                }
            }
            else
            {
                // Log the error.
                Log.Error(TAG, context.GetString(Resource.String.geofence_transition_invalid_type, new[] { new Java.Lang.Integer(geofenceTransition) }));
                dbAdapter.AddDeviceLog($"ジオフェンスエラー：{context.GetString(Resource.String.geofence_transition_invalid_type, new[] { new Java.Lang.Integer(geofenceTransition) })}");
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
