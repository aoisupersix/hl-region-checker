﻿using System.Linq;
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

        /// <summary>
        /// ステータス情報を更新します。
        /// </summary>
        /// <param name="stateId">更新するステータスID</param>
        private void UpdateStatus(int stateId)
        {
            //パーミッション確認
            var permissionWriteState = ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage);
            if (permissionWriteState != (int)Permission.Granted)
                return;

            var memId = UserDataModel.Instance.MemberId;
            if (memId == UserDataModel.DefaultMemberId)
                return;

            //ステータスの更新処理
            var childDict = new Dictionary<string, Java.Lang.Object>();
            childDict.Add("status", stateId);
            childDict.Add("last_update_is_auto", true);

            //更新
            var memRef = FirebaseDatabase.Instance.GetReference("members");
            memRef.Child(memId.ToString()).UpdateChildren(childDict);

            //var adapter = (IDbAdapter)(new DbAdapter_Droid());
            //adapter.UpdateStatus(memId.Value, stateId, true);
        }

        protected override void OnHandleIntent(Intent intent)
        {
            var geofencingEvent = GeofencingEvent.FromIntent(intent);
            if (geofencingEvent.HasError)
            {
                var errorMessage = GeofenceErrorMessages.GetErrorString(this, geofencingEvent.ErrorCode);
                Log.Error(TAG, errorMessage);
                NotificationUtil.Instance.SendNotification(this, "GeofenceError", "エラーです。", errorMessage);
                return;
            }

            int geofenceTransition = geofencingEvent.GeofenceTransition;

            if (geofenceTransition == Geofence.GeofenceTransitionEnter ||
                geofenceTransition == Geofence.GeofenceTransitionExit)
            {

                IList<IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;

                string geofenceTransitionDetails = GetGeofenceTransitionDetails(this, geofenceTransition, triggeringGeofences);

                Log.Info(TAG, geofenceTransitionDetails);

                if (geofenceTransition == Geofence.GeofenceTransitionEnter)
                {
                    NotificationUtil.Instance.SendNotification(this, "Geofence:侵入", geofenceTransitionDetails, geofenceTransitionDetails);
                    UpdateStatus(Status.学内.GetStatusId());
                }
                else
                {
                    NotificationUtil.Instance.SendNotification(this, "Geofence:退出", geofenceTransitionDetails, geofenceTransitionDetails);
                    UpdateStatus(Status.帰宅.GetStatusId());
                }
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
