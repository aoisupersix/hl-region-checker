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

namespace HLRegionChecker.Droid.Services
{
    [Service(Name = "org.hykwlab.hlregionchecker.Services.BleService", Process = ":TestProcess")]
    public class BleService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}