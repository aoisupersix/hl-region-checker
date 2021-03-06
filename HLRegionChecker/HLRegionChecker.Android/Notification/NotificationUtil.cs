﻿using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Firebase.Database;
using HLRegionChecker.Const;
using HLRegionChecker.Droid.DependencyServices;
using HLRegionChecker.Droid.Geofences;
using HLRegionChecker.Models;

namespace HLRegionChecker.Droid.Notification
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
        /// ステータス通知チャンネルID
        /// </summary>
        public const string STATUS_NOTIFICATION_CHANNEL_ID = "hlregionchecker_statusNotification_channel";

        /// <summary>
        /// フォアグラウンド通知チャンネルID
        /// </summary>
        public const string SERVICE_NOTIFICATION_CHANNEL_ID = "hlregionchecker_serviceNotification_channel";

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

        private PendingIntent GetPendingIntent(Android.Content.Context context)
        {
            var intent = new Intent(context, typeof(MainActivity));
            PendingIntent pendingIntent = PendingIntent.GetActivity(
                context, 0, intent, PendingIntentFlags.UpdateCurrent);

            return pendingIntent;
        }

        /// <summary>
        /// 通知チャンネルを生成します。
        /// </summary>
        /// <param name="notificationManager"></param>
        public void CreateNotificationChannel(NotificationManager notificationManager, Android.Content.Context context)
        {
            this.notificationManager = notificationManager;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // ステータス通知チャンネル
                var statusNotificationChannel = new NotificationChannel(
                    STATUS_NOTIFICATION_CHANNEL_ID,
                    context.GetString(Resource.String.notificationchannel_description),
                    NotificationImportance.High);
                statusNotificationChannel.Description = "Update notification of occupancy status";
                statusNotificationChannel.EnableLights(true);
                statusNotificationChannel.LightColor = Color.Red;
                //notificationChannel.SetVibrationPattern(new long[]{ 0, 1000, 500, 1000});
                //notificationChannel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(statusNotificationChannel);

                // フォアグラウンド通知チャンネル
                var serviceNotificationChannel = new NotificationChannel(
                    SERVICE_NOTIFICATION_CHANNEL_ID,
                    context.GetString(Resource.String.notificationchannel_description),
                    NotificationImportance.High);

                // Configure the notification channel.
                serviceNotificationChannel.Description = "Update notification of occupancy status";
                serviceNotificationChannel.EnableLights(true);
                serviceNotificationChannel.LightColor = Color.Red;
                //notificationChannel.SetVibrationPattern(new long[]{ 0, 1000, 500, 1000});
                //notificationChannel.EnableVibration(true);
                notificationManager.CreateNotificationChannel(serviceNotificationChannel);
            }
        }

        /// <summary>
        /// 通知を生成して返します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notificationChannelId"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public Android.App.Notification CreateNotification(Android.Content.Context context, string notificationChannelId, string title, string body, string info)
        {
            var notificationBuilder = new NotificationCompat.Builder(context, notificationChannelId);

            notificationBuilder.SetAutoCancel(true)
                .SetDefaults(1)
                .SetWhen((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds) //タイムスタンプ
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetTicker(body)
                //.SetPriority(Notification.PRIORITY_MAX)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetContentIntent(GetPendingIntent(context))
                .SetContentInfo(info);

            return notificationBuilder.Build();
        }

        /// <summary>
        /// プッシュ通知を送信します。
        /// </summary>
        /// <param name="title">通知タイトル</param>
        /// <param name="body">通知本文</param>
        public void SendNotification(Android.Content.Context context, string notificationChannelId,string title, string body, string info)
        {
            if (notificationManager == null)
                return;

            notificationManager.Notify(NOTIFICATION_ID, CreateNotification(context, notificationChannelId, title, body, info));
        }

        internal void CreateNotificationChannel(NotificationManager notificationManager, BootReceiver bootReceiver)
        {
            throw new NotImplementedException();
        }
    }
}