using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Util;

using HLRegionChecker.Models;
using HLRegionChecker.Droid.Notification;
using HLRegionChecker.Droid.DependencyServices;
using System;

namespace HLRegionChecker.Droid.Geofences
{
    [Service]
    /// <summary>
    /// ジオフェンスに侵入/退出した際の処理クラス
    /// </summary>
    public class GeofenceTransitionsIntentService : IntentService
    {
        private const string TAG = "GeofenceTransitionsIS";

        public GeofenceTransitionsIntentService()
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            var geofencingEvent = GeofencingEvent.FromIntent(intent);
            var dbAdapter = new DbAdapter_Droid();

            if (geofencingEvent.HasError)
            {
                var errorMessage = GeofenceErrorMessages.GetErrorString(this, geofencingEvent.ErrorCode);
                dbAdapter.AddDeviceLog("ジオフェンスエラー", errorMessage);
                NotificationUtil.Instance.SendNotification(this, NotificationUtil.STATUS_NOTIFICATION_CHANNEL_ID, "GeofenceError", "エラーです。", errorMessage);
                return;
            }

            int geofenceTransition = geofencingEvent.GeofenceTransition;

            if (geofenceTransition == Geofence.GeofenceTransitionEnter ||
                geofenceTransition == Geofence.GeofenceTransitionExit)
            {

                IList<IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;
                var updateGeofenceStatus = geofenceTransition == Geofence.GeofenceTransitionEnter;
                string geofenceTransitionDetails = GetGeofenceTransitionDetails(this, geofenceTransition, triggeringGeofences);
                Log.Info(TAG, geofenceTransitionDetails);

                var triggerRegions = triggeringGeofences
                    .Select(g => Regions.RegionList.GetRegionFromIdentifier(g.RequestId))
                    .Where(r => r != null)
                    .Select(r => (Regions.GeofenceRegion)r)
                    .ToList()
                    ;

                //GPS情報
                var lat = geofencingEvent.TriggeringLocation.Latitude;
                var lng = geofencingEvent.TriggeringLocation.Longitude;
                var accuracy = geofencingEvent.TriggeringLocation.Accuracy;
                var date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                // 更新
                foreach (var region in triggerRegions)
                {
                    var statusText = geofenceTransition == Geofence.GeofenceTransitionEnter ? "侵入" : "退出";
                    dbAdapter.AddDeviceLog($"ジオフェンス[{region.DbIdentifierName}]の状態を[{statusText}]に更新", $"{date},Droid,{lat},{lng},{accuracy},{statusText}");
                    dbAdapter.UpdateGeofenceStatus(UserDataModel.Instance.DeviceId, region.DbIdentifierName, updateGeofenceStatus);
                }
            }
            else
            {
                // Log the error.
                Log.Error(TAG, this.GetString(Resource.String.geofence_transition_invalid_type, new[] { new Java.Lang.Integer(geofenceTransition) }));
                dbAdapter.AddDeviceLog("ジオフェンスエラー", this.GetString(Resource.String.geofence_transition_invalid_type, new[] { new Java.Lang.Integer(geofenceTransition) }));
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
                    return this.GetString(Resource.String.geofence_transition_entered);
                case Geofence.GeofenceTransitionExit:
                    return this.GetString(Resource.String.geofence_transition_exited);
                default:
                    return this.GetString(Resource.String.unknown_geofence_transition);
            }
        }
    }
}
