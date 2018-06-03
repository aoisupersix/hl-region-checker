using System;
using UserNotifications;

namespace HLRegionChecker.iOS.Notification
{
    /// <summary>
    /// プッシュ通知の送信を行うマネージャクラスです。
    /// </summary>
    public static class PushNotificationManager
    {
        /// <summary>
        /// ローカル通知を送信します。
        /// </summary>
        /// <param name="title">通知のタイトル</param>
        /// <param name="body">通知のメッセージ</param>
        public static void Send(String title, String body)
        {
            var content = new UNMutableNotificationContent()
            {
                Title = title,
                Body = body,
                Sound = UNNotificationSound.Default,
                Badge = 0,
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.1, false);
            var request = UNNotificationRequest.FromIdentifier("LocationNotification", content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, (error) =>
            {
                if (error != null)
                    Console.Error.WriteLine("プッシュ通知送信に失敗しました。\n{0}", error.ToString());
            });
        }
    }
}