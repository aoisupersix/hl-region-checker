using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using HLRegionChecker.Const;
using Org.Altbeacon.Beacon;
using Org.Altbeacon.Beacon.Client;
using Firebase;
using Firebase.Database;
using Prism;
using Prism.Ioc;
using Android.Gms.Location;
using Android.Gms.Common.Apis;
using Java.Lang;
using Android.Gms.Common;
using Android.Views;
using System.Collections.Generic;
using Android.Support.V4.App;
using Android;
using Android.Support.V4.Content;
using Android.Gms.Tasks;
using Xamarin.Forms;

namespace HLRegionChecker.Droid
{
    [Activity(Label = "HLRegionChecker", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, Android.Gms.Tasks.IOnCompleteListener
    {
        protected string TAG = typeof(MainActivity).Name;
        public int REQUEST_PERMISSIONS_REQUEST_CODE = 34;

        private enum PendingGeofenceTask
        {
            ADD, REMOVE, NONE
        }

        private GeofencingClient mGeofencingClient;
        private IList<IGeofence> mGeofenceList;
        private PendingIntent mGeofencePendingIntent;
        private PendingGeofenceTask mPendingGeofenceTask = PendingGeofenceTask.NONE;

        public const string PROPERTY_KEY_LOCATION_UPDATES_REQUESTED = "location-updates-requested";

        public bool? GeofenceAdded
        {
            get
            {
                if (Xamarin.Forms.Application.Current.Properties.ContainsKey(PROPERTY_KEY_LOCATION_UPDATES_REQUESTED))
                    return Xamarin.Forms.Application.Current.Properties[PROPERTY_KEY_LOCATION_UPDATES_REQUESTED] as bool?;
                return null;
            }
            set
            {
                Xamarin.Forms.Application.Current.Properties[PROPERTY_KEY_LOCATION_UPDATES_REQUESTED] = value;
                Xamarin.Forms.Application.Current.SavePropertiesAsync();
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            FirebaseApp.InitializeApp(this);
            var db = FirebaseDatabase.Instance;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));

            mGeofenceList = new List<IGeofence>();
            mGeofencePendingIntent = null;
            PopulateGeofenceList();
            mGeofencingClient = LocationServices.GetGeofencingClient(this);

            var beaconManager = BeaconManager.GetInstanceForApplication(this.ApplicationContext);
            beaconManager.BeaconParsers.Add(new BeaconParser().SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24"));
            beaconManager.Bind(new MyBeaconConsumer(beaconManager));
        }

        protected override void OnStart()
        {
            base.OnStart();

            AddGeofences();

            //if (CheckPermission() && GeofenceAdded.HasValue && !GeofenceAdded.Value)
            //    AddGeofences();
        }

        /// <summary>
        /// GeofencingRequestを生成して返します。
        /// </summary>
        /// <returns>The geofencing request.</returns>
        GeofencingRequest GetGeofencingRequest()
        {
            GeofencingRequest.Builder builder = new GeofencingRequest.Builder();
            builder.SetInitialTrigger(GeofencingRequest.InitialTriggerEnter);
            builder.AddGeofences(mGeofenceList);
            return builder.Build();
        }

        /// <summary>
        /// Adds the geofences.
        /// </summary>
        void AddGeofences()
        {
            if (!CheckPermissions())
            {
                System.Diagnostics.Debug.WriteLine("Permission Denied");
                return;
            }

            mGeofencingClient.AddGeofences(GetGeofencingRequest(), GetGeofencePendingIntent())
                    .AddOnCompleteListener(this);
        }

        /// <summary>
        /// Removes the geofences.
        /// </summary>
        void RemoveGeofences()
        {
            if (!CheckPermissions())
            {
                System.Diagnostics.Debug.WriteLine("Permission Denied");
                return;
            }

            mGeofencingClient.RemoveGeofences(GetGeofencePendingIntent()).AddOnCompleteListener(this);
        }

        /// <summary>
        /// GeofencePendingIntentを生成して返します。
        /// </summary>
        /// <returns>The geofence pending intent.</returns>
        PendingIntent GetGeofencePendingIntent()
        {
            // Reuse the PendingIntent if we already have it.
            if (mGeofencePendingIntent != null)
            {
                return mGeofencePendingIntent;
            }
            Intent intent = new Intent(this, typeof(GeofenceTransitionsIntentService));
            mGeofencePendingIntent = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.UpdateCurrent);
            return mGeofencePendingIntent;
        }

        void PopulateGeofenceList()
        {
            foreach (var entry in Constants.BAY_AREA_LANDMARKS)
            {

                mGeofenceList.Add(new GeofenceBuilder()
                    .SetRequestId(entry.Key)
                    .SetCircularRegion(
                        entry.Value.Latitude,
                        entry.Value.Longitude,
                        Constants.GEOFENCE_RADIUS_IN_METERS
                    )
                    .SetExpirationDuration(Constants.GEOFENCE_EXPIRATION_IN_MILLISECONDS)
                    .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
                    .Build());
            }
        }

        /// <summary>
        /// 位置情報の利用が許可されているのか確認します。
        /// </summary>
        /// <returns><c>true</c>, if permissions was checked, <c>false</c> otherwise.</returns>
        bool CheckPermissions()
        {
            //TODO
            return true;
        }

        public void OnComplete(Task task)
        {
            mPendingGeofenceTask = PendingGeofenceTask.NONE;
            if (task.IsSuccessful)
            {
                GeofenceAdded = !GeofenceAdded;

                string message = GeofenceAdded.HasValue && GeofenceAdded.Value ? "Geofence Added" : "Geofence Removed";
                System.Diagnostics.Debug.WriteLine(message);
            }
            else
            {
                // Get the status code for the error and log it using a user-friendly message.
                string errorMessage = GeofenceErrorMessages.GetErrorString(this, task.Exception);
                System.Diagnostics.Debug.WriteLine(errorMessage);
            }
        }
    }
    public class MyBeaconConsumer : Java.Lang.Object, IBeaconConsumer
    {
        private BeaconManager _beaconManager;

        public Context ApplicationContext => throw new System.NotImplementedException();

        public MyBeaconConsumer(BeaconManager manager)
        {
            _beaconManager = manager;
        }

        public bool BindService(Intent p0, IServiceConnection p1, int p2)
        {
            throw new System.NotImplementedException();
        }

        public void OnBeaconServiceConnect()
        {
            var uuid = Identifier.Parse(RegionConst.BEACON_UUID);
            var major = Identifier.Parse(RegionConst.BEACON_MAJOR.ToString());
            var minor = Identifier.Parse(RegionConst.BEACON_MINOR.ToString());
            var region = new Region("aa", uuid, major, minor);

            _beaconManager.AddMonitorNotifier(new BeaconNotifier());
            try
            {
                _beaconManager.StartMonitoringBeaconsInRegion(region);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public void UnbindService(IServiceConnection p0)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BeaconNotifier : Java.Lang.Object, IMonitorNotifier
    {
        //public new IntPtr Handle;

        public void DidDetermineStateForRegion(int p0, Region p1)
        {
        }

        public void DidEnterRegion(Region p0)
        {
            System.Diagnostics.Debug.WriteLine("Enter");
        }

        public void DidExitRegion(Region p0)
        {
            System.Diagnostics.Debug.WriteLine("Exit");
        }

        public new void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("Dispose");
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            // Register any platform specific implementations
        }
    }
}

