using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using HLRegionChecker.Const;
using HLRegionChecker.Interfaces;

namespace HLRegionChecker.Droid.Geofences
{
    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    /// <summary>
    /// ブート時にジオフェンスを登録するブロードキャストレシーバ
    /// </summary>
    public class BootReceiver : BroadcastReceiver
    {
        #region メンバ
        protected string TAG = typeof(BootReceiver).Name;

        private RegisterGeofences _registerGeofences;
        private Android.Content.Context _context;
        #endregion

        /// <summary>
        /// レシーバ起動
        /// </summary>
        /// <param name="context"></param>
        /// <param name="intent"></param>
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Info(TAG, "Boot intent received.");

            //ジオフェンスの登録
            _context = context;
            _registerGeofences = new RegisterGeofences(context);
            _registerGeofences.AddGeofences();
        }
    }
}