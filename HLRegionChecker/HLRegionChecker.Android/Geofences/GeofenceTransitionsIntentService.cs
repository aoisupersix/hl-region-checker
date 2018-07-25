using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Locations;
using Android.Support.V4.App;
using Java.Lang;
using Android.Util;

namespace HLRegionChecker.Droid.Geofences
{
    [Service]
    /// <summary>
    /// ジオフェンスに侵入/退出した際の処理クラス
    /// </summary>
    public class GeofenceTransitionsIntentService : IntentService
    {
        private const string TAG = "GeofenceTransitionsIS";

        public GeofenceTransitionsIntentService() : base(TAG)
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            var geofencingEvent = GeofencingEvent.FromIntent(intent);
            if (geofencingEvent.HasError)
            {
                var errorMessage = GeofenceErrorMessages.GetErrorString(this, geofencingEvent.ErrorCode);
                Log.Error(TAG, errorMessage);
                return;
            }

            int geofenceTransition = geofencingEvent.GeofenceTransition;

            if (geofenceTransition == Geofence.GeofenceTransitionEnter ||
                geofenceTransition == Geofence.GeofenceTransitionExit)
            {

                IList<IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;

                string geofenceTransitionDetails = GetGeofenceTransitionDetails(this, geofenceTransition, triggeringGeofences);

                //SendNotification(geofenceTransitionDetails);
                Log.Info(TAG, geofenceTransitionDetails);
            }
            else
            {
                // Log the error.
                Log.Error(TAG, GetString(Resource.String.geofence_transition_invalid_type, new[] { new Java.Lang.Integer(geofenceTransition) }));
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
                    return GetString(Resource.String.geofence_transition_entered);
                case Geofence.GeofenceTransitionExit:
                    return GetString(Resource.String.geofence_transition_exited);
                default:
                    return GetString(Resource.String.unknown_geofence_transition);
            }
        }
    }
}
