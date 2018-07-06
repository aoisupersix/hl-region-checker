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

namespace HLRegionChecker.Droid
{
    [Activity(Label = "HLRegionChecker", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            FirebaseApp.InitializeApp(this);
            var db = FirebaseDatabase.Instance;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));

            var beaconManager = BeaconManager.GetInstanceForApplication(this.ApplicationContext);
            beaconManager.BeaconParsers.Add(new BeaconParser().SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24"));
            beaconManager.Bind(new MyBeaconConsumer(beaconManager));
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
            catch(Exception e)
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

