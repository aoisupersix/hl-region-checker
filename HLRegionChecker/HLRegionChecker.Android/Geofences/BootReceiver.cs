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

namespace HLRegionChecker.Droid.Geofences
{
    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "android.permission.RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    /// <summary>
    /// ブート時にジオフェンスを登録するブロードキャストレシーバ
    /// </summary>
    public class BootReceiver : BroadcastReceiver, IOnCompleteListener
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
            _registerGeofences = new RegisterGeofences(context, this);
            _registerGeofences.AddGeofences();
        }

        /// <summary>
        /// ジオフェンスの追加完了コールバック
        /// </summary>
        /// <param name="task"></param>
        public void OnComplete(Task task)
        {
            if (task.IsSuccessful)
            {
                var message = _context.GetString(Resource.String.complete_add_geofence);
                Log.Info(TAG, message);
            }
            else
            {
                // Get the status code for the error and log it using a user-friendly message.
                var errorMessage = Geofences.GeofenceErrorMessages.GetErrorString(_context, task.Exception);
                Log.Info(TAG, errorMessage);
            }
        }
    }
}