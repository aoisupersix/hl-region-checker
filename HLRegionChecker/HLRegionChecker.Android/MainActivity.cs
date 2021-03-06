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
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Prism;
using Prism.Ioc;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using static Android.Support.V4.App.ActivityCompat;
using Android.Runtime;
using HLRegionChecker.Droid.Geofences;
using HLRegionChecker.Models;
using HLRegionChecker.Interfaces;

namespace HLRegionChecker.Droid
{
    [Activity(Label = "HLRegionChecker", Icon = "@mipmap/appicon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGeofenceRegisterCompleteListener
    {
        #region メンバ
        protected string TAG = typeof(MainActivity).Name;

        /// <summary>
        /// 位置情報パーミッションコード
        /// </summary>
        public int REQUEST_FINE_LOCATION_CODE = 34;

        private RegisterGeofences _registerGeofences;
        #endregion

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));

            AppCenter.Start("1bcf60ce-7dc3-4084-b04f-8227131443f1", typeof(Analytics), typeof(Crashes));

            // デバイスID登録
            // この処理はDependencyServiceを使うのでForms.Initの前に呼び出してはいけない
            if (UserDataModel.Instance.DeviceId == null)
                UserDataModel.Instance.DeviceId = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);

            // ジオフェンスの初期化
            _registerGeofences = new RegisterGeofences(this, this);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (!CheckLocationPermissions())
            {
                RequestLocationPermissions();
            }
            else if (!RegisterGeofences.GeofenceAdded)
                _registerGeofences.AddGeofences();
        }

        #region パーミッション関係メソッド
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

        /// <summary>
        /// パーミッションの結果処理用
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
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
                    if (!RegisterGeofences.GeofenceAdded)
                        _registerGeofences.AddGeofences();
                }
                else
                {
                    ShowSnackbar("設定から位置情報の利用を許可してください。");
                }
            }
        }
        #endregion

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

        #region IGeofenceRegisterCompleteListener

        public void RegisterCompleted(bool successful, string message)
        {
            ShowSnackbar(message);
        }
        #endregion
    }

    /// <summary>
    /// 位置情報利用許可のリスナー
    /// </summary>
    public class RequestLocationPermissionsClickListener : Java.Lang.Object, View.IOnClickListener
    {
        public MainActivity Activity { get; set; }

        public void OnClick(View v)
        {
            RequestPermissions(Activity, new[] { Manifest.Permission.AccessFineLocation }, Activity.REQUEST_FINE_LOCATION_CODE);
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

