﻿using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Firebase.Database;
using HLRegionChecker.Const;
using HLRegionChecker.Droid.DependencyServices;
using HLRegionChecker.Models;

namespace HLRegionChecker.Droid
{
    /// <summary>
    /// 通知用クラス
    /// </summary>
    public class NotificationUtil
    {
        #region メンバ
        /// <summary>
        /// クラスのインスタンス
        /// </summary>
        private static NotificationUtil _instance;

        /// <summary>
        /// 通知用
        /// </summary>
        private NotificationManager notificationManager;

        /// <summary>
        /// チャンネルID
        /// </summary>
        public const string NOTIFICATION_CHANNEL_ID = "hlregionchecker_notification_channel";

        /// <summary>
        /// 通知ID
        /// </summary>
        public const int NOTIFICATION_ID = 1000;
        #endregion

        #region プロパティ
        /// <summary>
        /// クラスのインスタンスを取得します。
        /// </summary>
        public static NotificationUtil Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NotificationUtil();

                return _instance;
            }
        }
        #endregion

        private NotificationUtil()
        {
        }

        /// <summary>
        /// 通知チャンネルを生成します。
        /// </summary>
        /// <param name="notificationManager"></param>
        public void CreateNotificationChannel(NotificationManager notificationManager)
        {
            this.notificationManager = notificationManager;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, "Update notification of occupancy status", NotificationImportance.High);

                // Configure the notification channel.
                notificationChannel.Description = "Update notification of occupancy status";
                notificationChannel.EnableLights(true);
                notificationChannel.LightColor = Color.Red;
                //notificationChannel.SetVibrationPattern(new long[]{ 0, 1000, 500, 1000});
                //notificationChannel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(notificationChannel);
            }
        }

        /// <summary>
        /// プッシュ通知を送信します。
        /// </summary>
        /// <param name="title">通知タイトル</param>
        /// <param name="body">通知本文</param>
        public void SendNotification(Android.Content.Context context, string title, string body, string info)
        {
            if (notificationManager == null)
                return;

            var notificationBuilder = new NotificationCompat.Builder(context, NOTIFICATION_CHANNEL_ID);

            notificationBuilder.SetAutoCancel(true)
                .SetDefaults(1)
                .SetWhen((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds) //タイムスタンプ
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetTicker(body)
                //.SetPriority(Notification.PRIORITY_MAX)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetContentInfo(info);

            notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
        }
    }
}