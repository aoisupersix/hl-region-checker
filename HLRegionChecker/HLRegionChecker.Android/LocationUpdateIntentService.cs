using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Locations;

namespace HLRegionChecker.Droid
{
    [Service]
    public class LocationUpdateIntentService: IntentService
    {

        public const string ACTION_PROCESS_UPDATES =
            "org.hykwlab.hlregionchecker_droid.action" +
                    ".PROCESS_UPDATES";
        
        public LocationUpdateIntentService()
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            System.Diagnostics.Debug.WriteLine("OnHandleIntent!");
            if (intent != null)
            {
                string action = intent.Action;
                if (ACTION_PROCESS_UPDATES.Equals(action))
                {
                    LocationResult result = LocationResult.ExtractResult(intent);
                    if (result != null)
                    {
                        List<Location> locations = result.Locations.ToList();
                        foreach(var l in locations) {
                            System.Diagnostics.Debug.WriteLine(l.ToString());
                        }
                        LocationResultHelper locationResultHelper = new LocationResultHelper(this, locations);
                        // Save the location data to SharedPreferences.
                        //locationResultHelper.saveResults();
                        // Show notification with the location data.
                        //locationResultHelper.showNotification();
                    }
                }
            }        
        }
    }
}
