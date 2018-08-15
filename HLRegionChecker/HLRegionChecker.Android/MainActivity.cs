using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using System.Collections.Generic;
using Android;
using Android.Arch.Lifecycle;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Tasks;
using Prism;
using Prism.Ioc;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using static Android.Support.V4.App.ActivityCompat;
using Firebase;
using Firebase.Database;
using HLRegionChecker.Const;

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

            if (!CheckPermissions())
                RequestPermissions();
            else
            {
                AddGeofences();
            }
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
                RequestPermissions();
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
            mGeofenceList.Add(new GeofenceBuilder()
                .SetRequestId(Region.研究室.GetIdentifier())
                .SetCircularRegion(
                    RegionConst.CAMPUS_LATITUDE,
                    RegionConst.CAMPUS_LONGITUDE,
                    Geofences.Constants.GEOFENCE_RADIUS_IN_METERS
                )
                .SetExpirationDuration(Geofences.Constants.GEOFENCE_EXPIRATION_IN_MILLISECONDS)
                .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
                .Build());
        }

        /// <summary>
        /// 位置情報の利用が許可されているのか確認します。
        /// </summary>
        /// <returns><c>true</c>, if permissions was checked, <c>false</c> otherwise.</returns>
        bool CheckPermissions()
        {
            var permissionState = ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation);
            return permissionState == (int)Permission.Granted;
        }

        /// <summary>
        /// 位置情報の許可リクエストを行います。
        /// </summary>
        void RequestPermissions()
        {
            var shouldProvideRationale = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation);

            if (shouldProvideRationale)
            {
                Log.Info(TAG, "Displaying permission rationale to provide additional context.");
                var listener = (View.IOnClickListener)new RequestPermissionsClickListener { Activity = this };
                ShowSnackbar(Resource.String.permission_rationale, Android.Resource.String.Ok, listener);
            }
            else
            {
                Log.Info(TAG, "Requesting permission");
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, REQUEST_PERMISSIONS_REQUEST_CODE);
            }
        }

        /// <summary>
        /// スナックバーを表示します。
        /// </summary>
        /// <param name="text"></param>
        private void ShowSnackbar(string text)
        {
            var container = FindViewById<View>(Android.Resource.Id.Content);
            if (container != null)
            {
                Snackbar.Make(container, text, Snackbar.LengthLong).Show();
            }
        }

        /// <summary>
        /// スナックバーを表示します。
        /// </summary>
        /// <param name="text"></param>
        private void ShowSnackbar(int mainTextStringId, int actionStringId, View.IOnClickListener listener)
        {
            Snackbar.Make(FindViewById(Android.Resource.Id.Content),
                    GetString(mainTextStringId),
                    Snackbar.LengthIndefinite)
                .SetAction(GetString(actionStringId), listener).Show();
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

    public class RequestPermissionsClickListener : Java.Lang.Object, View.IOnClickListener
    {
        public MainActivity Activity { get; set; }

        public void OnClick(View v)
        {
            RequestPermissions(Activity, new[] { Manifest.Permission.AccessFineLocation }, Activity.REQUEST_PERMISSIONS_REQUEST_CODE);
        }
    }

    public class OnRequestPermissionsResultClickListener : Java.Lang.Object, View.IOnClickListener
    {

        public MainActivity Activity { get; set; }
        public void OnClick(View v)
        {
            Intent intent = new Intent();
            intent.SetAction(Settings.ActionApplicationDetailsSettings);
            var uri = Android.Net.Uri.FromParts("package", BuildConfig.ApplicationId, null);
            intent.SetData(uri);
            intent.SetFlags(ActivityFlags.NewTask);
            Activity.StartActivity(intent);
        }
    }

    /// <summary>
    /// Prismの初期化処理？
    /// </summary>
    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            // Register any platform specific implementations
        }
    }
}

