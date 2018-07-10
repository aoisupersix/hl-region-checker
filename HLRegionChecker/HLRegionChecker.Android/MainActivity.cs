using System;
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

namespace HLRegionChecker.Droid
{
    [Activity(Label = "HLRegionChecker", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        /// <summary>
        /// 位置情報の更新間隔
        /// </summary>
        const long UPDATE_INTERVAL = 10 * 1000;
        const long FASTEST_UPDATE_INTERVAL = UPDATE_INTERVAL / 2;
        const long MAX_WAIT_TIME = UPDATE_INTERVAL * 3;
        
        /// <summary>
        /// GoogleApiClient
        /// </summary>
        GoogleApiClient mGoogleApiClient;

        /// <summary>
        /// LocationRequest
        /// </summary>
        LocationRequest mLocationRequest;

        /// <summary>
        /// GoogleApiClientの初期化を行います
        /// </summary>
        void BuildGoogleApiClient() {
            if (mGoogleApiClient != null)
                return;
            mGoogleApiClient = new GoogleApiClient.Builder(this)
                .AddConnectionCallbacks(this)
                .EnableAutoManage(this, this)
                .AddApi(LocationServices.API)
                .Build();
            CreateLocationRequest();
        }

        /// <summary>
        /// LocationRequestの初期化を行います
        /// </summary>
        void CreateLocationRequest() {
            mLocationRequest = new LocationRequest();

            mLocationRequest.SetInterval(UPDATE_INTERVAL);

            mLocationRequest.SetFastestInterval(FASTEST_UPDATE_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);

            mLocationRequest.SetMaxWaitTime(MAX_WAIT_TIME);
        }

        PendingIntent GetPendingIntent() {
            var intent = new Intent(this, typeof(LocationUpdateIntentService));
            intent.SetAction(LocationUpdateIntentService.ACTION_PROCESS_UPDATES);
            return PendingIntent.GetService(this, 0, intent, PendingIntentFlags.UpdateCurrent);
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

            BuildGoogleApiClient();

            var beaconManager = BeaconManager.GetInstanceForApplication(this.ApplicationContext);
            beaconManager.BeaconParsers.Add(new BeaconParser().SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24"));
            beaconManager.Bind(new MyBeaconConsumer(beaconManager));
        }

        public void RequestLocationUpdates()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting location updates");
                LocationRequestModel.Instance.LocationRequesting = true;
                LocationServices.FusedLocationApi.RequestLocationUpdates(
                        mGoogleApiClient, mLocationRequest, GetPendingIntent());
            }
            catch (SecurityException e)
            {
                LocationRequestModel.Instance.LocationRequesting = false;
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public void RemoveLocationUpdates(View view)
        {
            System.Diagnostics.Debug.WriteLine("Removing Location Updates");
            LocationRequestModel.Instance.LocationRequesting = false;
            LocationServices.FusedLocationApi.RemoveLocationUpdates(mGoogleApiClient,
                    GetPendingIntent());
        }

        /// <summary>
        /// IConnectionCallbackイベント
        /// </summary>
        /// <param name="connectionHint">Connection hint.</param>
        public void OnConnected(Bundle connectionHint)
        {
            System.Diagnostics.Debug.WriteLine("GoogleServiceClient: Connected!");
            RequestLocationUpdates();
        }

        /// <summary>
        /// IConnectionCallbackイベント
        /// </summary>
        /// <param name="cause">Cause.</param>
        public void OnConnectionSuspended(int cause)
        {
            System.Diagnostics.Debug.WriteLine("GoogleServiceClient: Connection Suspended!");
        }

        /// <summary>
        /// IConnectionFailedListenerイベント
        /// </summary>
        /// <param name="result">Result.</param>
        public void OnConnectionFailed(ConnectionResult result)
        {
            System.Diagnostics.Debug.WriteLine("GoogleServiceClient: Connection Failed!");
            System.Diagnostics.Debug.WriteLine(result);
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
            catch(System.Exception e)
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

