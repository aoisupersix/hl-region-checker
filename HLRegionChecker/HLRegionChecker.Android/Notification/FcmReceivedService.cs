using System;
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
using Firebase.Messaging;

namespace HLRegionChecker.Droid.Notification
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    /// <summary>
    /// FCMの通知表示クラス
    /// </summary>
    public class FcmReceivedService: FirebaseMessagingService
    {

        protected string TAG = typeof(FcmReceivedService).Name;

        public override void OnMessageReceived(RemoteMessage message)
        {
            var notify = message.GetNotification();
            NotificationUtil.Instance.SendNotification(this, notify.Title, notify.Body, "FCM通知");
        }
    }
}