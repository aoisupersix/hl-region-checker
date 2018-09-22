﻿using System;
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
using Android.Runtime;

namespace HLRegionChecker.Droid
{
    [Activity(Label = "HLRegionChecker", Icon = "@mipmap/appicon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IOnCompleteListener
    {
        #region メンバ
        protected string TAG = typeof(MainActivity).Name;

        /// <summary>
        /// ストレージパーミッションコード
        /// </summary>
        public int REQUEST_EXTERNAL_STORAGE_CODE = 1;

        /// <summary>
        /// 位置情報パーミッションコード
        /// </summary>
        public int REQUEST_FINE_LOCATION_CODE = 34;

        public int NO_INITIAL_TRIGGER = 0;

        private enum PendingGeofenceTask
        {
            ADD, REMOVE, NONE
        }

        private GeofencingClient mGeofencingClient;
        private IList<IGeofence> mGeofenceList;
        private PendingIntent mGeofencePendingIntent;
        private PendingGeofenceTask mPendingGeofenceTask = PendingGeofenceTask.NONE;

        public const string PROPERTY_KEY_LOCATION_UPDATES_REQUESTED = "location-updates-requested";

        #endregion

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

            NotificationUtil.Instance.CreateNotificationChannel((NotificationManager)GetSystemService(NotificationService));

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

            //if (!CheckStoragePermissions())
            //    RequestStoragePermissions();

            if (!CheckLocationPermissions())
                RequestLocationPermissions();
            else
                AddGeofences();
        }

        #region パーミッション関係メソッド

        /// <summary>
        /// ストレージの利用が許可されているのか確認します。
        /// </summary>
        /// <returns><c>true</c>, if permissions was checked, <c>false</c> otherwise.</returns>
        bool CheckStoragePermissions()
        {
            var permissionWriteState = ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage);
            return permissionWriteState == (int)Permission.Granted;
        }

        /// <summary>
        /// ストレージの許可リクエストを行います。
        /// </summary>
        void RequestStoragePermissions()
        {
            var shouldProvideRationale = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage);

            if (shouldProvideRationale)
            {
                Log.Info(TAG, "Displaying permission rationale to provide additional context.");
                var listener = (View.IOnClickListener)new RequestStoragePermissionsClickListener { Activity = this };
                ShowSnackbar(Resource.String.permission_rationale, Android.Resource.String.Ok, listener);
            }
            else
            {
                Log.Info(TAG, "Requesting external storage permission");
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.WriteExternalStorage }, REQUEST_EXTERNAL_STORAGE_CODE);
            }
        }

        /// <summary>
        /// 位置情報の利用が許可されているのか確認します。
        /// </summary>
        /// <returns><c>true</c>, if permissions was checked, <c>false</c> otherwise.</returns>
        bool CheckLocationPermissions()
        {
            var permissionState = ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation);
            return permissionState == (int)Permission.Granted;
        }

        /// <summary>
        /// 位置情報の許可リクエストを行います。
        /// </summary>
        void RequestLocationPermissions()
        {
            var shouldProvideRationale = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation);

            if (shouldProvideRationale)
            {
                Log.Info(TAG, "Displaying permission rationale to provide additional context.");
                var listener = (View.IOnClickListener)new RequestLocationPermissionsClickListener { Activity = this };
                ShowSnackbar(Resource.String.permission_rationale, Android.Resource.String.Ok, listener);
            }
            else
            {
                Log.Info(TAG, "Requesting fine location permission");
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, REQUEST_FINE_LOCATION_CODE);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Log.Info(TAG, "onRequestPermissionResult");
            if (requestCode == REQUEST_FINE_LOCATION_CODE)
            {
                if (grantResults.Length <= 0)
                {
                    Log.Info(TAG, "User interaction was cancelled.");
                }
                else if (grantResults[0] == (int)Permission.Granted)
                {
                    Log.Info(TAG, "Permission granted.");
                    if(mPendingGeofenceTask == PendingGeofenceTask.ADD)
                        AddGeofences();
                    mPendingGeofenceTask = PendingGeofenceTask.NONE;
                }
                else
                {
                    //var listener = (View.IOnClickListener)new OnRequestPermissionsResultClickListener { Activity = this };
                    ShowSnackbar("設定から位置情報の利用を許可してください。");
                }
            }
        }

        #endregion

        /// <summary>
        /// GeofencingRequestを生成して返します。
        /// </summary>
        /// <returns>The geofencing request.</returns>
        GeofencingRequest GetGeofencingRequest()
        {
            GeofencingRequest.Builder builder = new GeofencingRequest.Builder();
            //builder.SetInitialTrigger(NO_INITIAL_TRIGGER);
            builder.SetInitialTrigger(GeofencingRequest.InitialTriggerEnter);
            builder.AddGeofences(mGeofenceList);
            return builder.Build();
        }

        /// <summary>
        /// ジオフェンスを追加します。
        /// </summary>
        void AddGeofences()
        {
            if (!CheckLocationPermissions())
            {
                ShowSnackbar("Permission Denied");
                System.Diagnostics.Debug.WriteLine("Permission Denied");
                mPendingGeofenceTask = PendingGeofenceTask.ADD;
                RequestLocationPermissions();
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
            if (!CheckLocationPermissions())
            {
                ShowSnackbar("Permission Denied");
                System.Diagnostics.Debug.WriteLine("Permission Denied");
                RequestLocationPermissions();
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
            var intent = new Intent(this, typeof(Geofences.GeofenceTransitionsIntentService));
            return PendingIntent.GetService(this, 0, intent, PendingIntentFlags.UpdateCurrent);
        }

        /// <summary>
        /// ジオフェンスのリストを設定します。
        /// </summary>
        void PopulateGeofenceList()
        {
            mGeofenceList.Add(new GeofenceBuilder()
                .SetRequestId(Region.学内.GetIdentifier())
                .SetCircularRegion(
                    35.817187,
                    139.424551,
                    200
                )
                .SetExpirationDuration(Geofence.NeverExpire)
                .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
                .Build());
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
                string message = GeofenceAdded.HasValue && GeofenceAdded.Value ? "Geofence Added" : "Geofence Removed";
                ShowSnackbar("ジオフェンスの登録完了");
                System.Diagnostics.Debug.WriteLine(message);
            }
            else
            {
                // Get the status code for the error and log it using a user-friendly message.
                string errorMessage = Geofences.GeofenceErrorMessages.GetErrorString(this, task.Exception);
                ShowSnackbar($"Error: {errorMessage}");
                System.Diagnostics.Debug.WriteLine(errorMessage);
            }
        }
    }

    /// <summary>
    /// ストレージ利用許可のリスナー
    /// </summary>
    public class RequestLocationPermissionsClickListener : Java.Lang.Object, View.IOnClickListener
    {
        public MainActivity Activity { get; set; }

        public void OnClick(View v)
        {
            RequestPermissions(Activity, new[] { Manifest.Permission.AccessFineLocation }, Activity.REQUEST_FINE_LOCATION_CODE);
        }
    }

    /// <summary>
    /// 位置情報利用許可のリスナー
    /// </summary>
    public class RequestStoragePermissionsClickListener : Java.Lang.Object, View.IOnClickListener
    {
        public MainActivity Activity { get; set; }

        public void OnClick(View v)
        {
            RequestPermissions(Activity, new[] { Manifest.Permission.AccessFineLocation }, Activity.REQUEST_EXTERNAL_STORAGE_CODE);
        }
    }

    public class OnRequestPermissionsResultClickListener : Java.Lang.Object, View.IOnClickListener
    {

        public MainActivity Activity { get; set; }
        public void OnClick(View v)
        {
            Intent intent = new Intent();
            intent.SetAction(Settings.ActionApplicationDetailsSettings);
            var uri = Android.Net.Uri.FromParts("package", Activity.PackageName, null);
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

