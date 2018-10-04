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
using HLRegionChecker.Const;
using HLRegionChecker.Droid.DependencyServices;
using HLRegionChecker.Droid.Notification;
using HLRegionChecker.Models;
using Org.Altbeacon.Beacon;

namespace HLRegionChecker.Droid.Beacons
{
    [Service]
    public class BeaconMonitoringService : Service, IBeaconConsumer, IMonitorNotifier
    {
        protected readonly string TAG = typeof(BeaconMonitoringService).Name;

        private BeaconManager _beaconManager;

        public bool BindService(Intent p0, IServiceConnection p1, int p2)
        {
            throw new NotImplementedException();
        }

        public void DidDetermineStateForRegion(int p0, Org.Altbeacon.Beacon.Region p1)
        {
        }

        public void DidEnterRegion(Org.Altbeacon.Beacon.Region p0)
        {
            throw new NotImplementedException();
        }

        public void DidExitRegion(Org.Altbeacon.Beacon.Region p0)
        {
            Log.Info(TAG, "Exit [{0}] Region", p0.UniqueId);

            if (p0.UniqueId.Equals(Regions.RegionList.研究室.Identifier))
            {
                //研究室から退出
                var dbAdapter = new DbAdapter_Droid();
                dbAdapter.UpdateStatus(UserDataModel.Instance.MemberId, Status.学内.GetStatusId(), true);

                _beaconManager.Unbind(this);
                StopForeground(true);
            }
        }

        public void OnBeaconServiceConnect()
        {
            _beaconManager.AddMonitorNotifier(this);

            var uuid = Identifier.Parse(Regions.RegionList.研究室.Uuid);
            var major = Identifier.Parse(Regions.RegionList.研究室.Major.ToString());
            var minor = Identifier.Parse(Regions.RegionList.研究室.Minor.ToString());
            var region = new Org.Altbeacon.Beacon.Region(Regions.RegionList.研究室.Identifier, uuid, major, minor);
            _beaconManager.StartMonitoringBeaconsInRegion(region);
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

            _beaconManager = BeaconManager.GetInstanceForApplication(this.ApplicationContext);
            _beaconManager.Bind(this);

            return base.OnStartCommand(intent, flags, startId);
        }
    }
}