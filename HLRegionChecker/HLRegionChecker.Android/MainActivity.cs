using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Firebase;
using Firebase.Database;
using Android.Gms.Location;
using System.Collections.Generic;
using Android.Gms.Tasks;
using Prism;
using Prism.Ioc;

namespace HLRegionChecker.Droid
{
    [Activity(Label = "HLRegionChecker", Icon = "@mipmap/appicon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IOnCompleteListener
    {
        protected string TAG = typeof(MainActivity).Name;
        public int REQUEST_PERMISSIONS_REQUEST_CODE = 34;

        /// <summary>
        /// ジオフェンスの状態
        /// </summary>
        private enum PendingGeofenceTask
        {
            ADD, REMOVE, NONE
        }

        private GeofencingClient mGeofencingClient;
        private IList<IGeofence> mGeofenceList;
        private PendingIntent mGeofencePendingIntent;
        //private PendingGeofenceTask mPendingGeofenceTask = PendingGeofenceTask.NONE;

        public const string PROPERTY_KEY_LOCATION_UPDATES_REQUESTED = "location-updates-requested";

        /// <summary>
        /// ジオフェンスが追加されているか？
        /// </summary>
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

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            FirebaseApp.InitializeApp(this);
            var db = FirebaseDatabase.Instance;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));

            //ジオフェンスの初期化
            mGeofenceList = new List<IGeofence>();
            mGeofencePendingIntent = null;
            PopulateGeofenceList();
            mGeofencingClient = LocationServices.GetGeofencingClient(this);
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
        /// ジオフェンスを追加します。
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
        /// ジオフェンスを削除します。
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
            Intent intent = new Intent(this, typeof(Geofences.GeofenceTransitionsIntentService));
            mGeofencePendingIntent = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.UpdateCurrent);
            return mGeofencePendingIntent;
        }

        /// <summary>
        /// ジオフェンスのリストを設定します。
        /// </summary>
        void PopulateGeofenceList()
        {
            foreach (var entry in Geofences.Constants.BAY_AREA_LANDMARKS)
            {

                mGeofenceList.Add(new GeofenceBuilder()
                    .SetRequestId(entry.Key)
                    .SetCircularRegion(
                        entry.Value.Latitude,
                        entry.Value.Longitude,
                        Geofences.Constants.GEOFENCE_RADIUS_IN_METERS
                    )
                    .SetExpirationDuration(Geofences.Constants.GEOFENCE_EXPIRATION_IN_MILLISECONDS)
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
            //mPendingGeofenceTask = PendingGeofenceTask.NONE;
            if (task.IsSuccessful)
            {
                GeofenceAdded = !GeofenceAdded;

                string message = GeofenceAdded.HasValue && GeofenceAdded.Value ? "Geofence Added" : "Geofence Removed";
                System.Diagnostics.Debug.WriteLine(message);
            }
            else
            {
                // Get the status code for the error and log it using a user-friendly message.
                string errorMessage = Geofences.GeofenceErrorMessages.GetErrorString(this, task.Exception);
                System.Diagnostics.Debug.WriteLine(errorMessage);
            }
        }
    }

    /// <summary>
    /// よくわかんないけど消すと落ちる
    /// ジオフェンス関係の処理だと思う
    /// </summary>
    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            // Register any platform specific implementations
        }
    }
}

