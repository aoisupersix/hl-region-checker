using System;
using System.Collections.Generic;
using Android.Content;
using Android.Locations;

namespace HLRegionChecker.Droid
{
    public class LocationResultHelper
    {
        
        private Context mContext;
        private List<Location> mLocations;

        public LocationResultHelper(Context context, List<Location> locations)
        {
            mContext = context;
            mLocations = locations;
        }
    }
}
