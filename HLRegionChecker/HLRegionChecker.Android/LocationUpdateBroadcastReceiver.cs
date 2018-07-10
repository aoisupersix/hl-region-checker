using System;
using Android.App;
using Android.Content;

namespace HLRegionChecker.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] {
        LocationUpdateBroadcastReceiver.ACTION_PROCESS_UPDATES
    })]
    public class LocationUpdateBroadcastReceiver: BroadcastReceiver
    {
        public const string ACTION_PROCESS_UPDATES =
            "org.hykwlab.hlregionchecker_droid.action" +
                    ".PROCESS_UPDATES";

        public override void OnReceive(Context context, Intent intent)
        {
            throw new NotImplementedException();
        }
    }
}
