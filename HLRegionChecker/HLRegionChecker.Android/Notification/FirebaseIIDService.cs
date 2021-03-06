﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using Firebase.Iid;
using HLRegionChecker.Droid.DependencyServices;
using HLRegionChecker.Interfaces;
using Xamarin.Forms;

namespace HLRegionChecker.Droid.Notification
{
    [Service]
    [IntentFilter( new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    /// <summary>
    /// FCMのトークン関係
    /// </summary>
    public class FirebaseIIDService : FirebaseInstanceIdService
    {
        protected string TAG = typeof(FirebaseIIDService).Name;

        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "Refreshed token: " + refreshedToken);
            SendRegistrationToServer(refreshedToken);
        }

        private void SendRegistrationToServer(string token)
        {
            IDbAdapter dbAdapter = new DbAdapter_Droid();
            dbAdapter.UpdateDeviceInfo(fcmToken: token);
        }
    }
}