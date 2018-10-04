using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using HLRegionChecker.Droid.Notification;
using Org.Altbeacon.Beacon;

namespace HLRegionChecker.Droid.Beacons
{
    [Service]
    public class BeaconMonitoringService : Service, IBeaconConsumer
    {
        public bool BindService(Intent p0, IServiceConnection p1, int p2)
        {
            throw new NotImplementedException();
        }

        public void OnBeaconServiceConnect()
        {
            throw new NotImplementedException();
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var notification = new Android.App.Notification.Builder(this)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText("ビーコン領域を監視中")
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetChannelId(NotificationUtil.SERVICE_NOTIFICATION_CHANNEL_ID)
                .SetOngoing(true)
                .Build();

            StartForeground(1, notification);

            return base.OnStartCommand(intent, flags, startId);
        }
    }
}