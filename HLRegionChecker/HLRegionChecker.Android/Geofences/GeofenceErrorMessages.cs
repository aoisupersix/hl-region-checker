using System;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;

namespace HLRegionChecker.Droid.Geofences
{
    /// <summary>
    /// ジオフェンスのエラーメッセージ処理クラス
    /// </summary>
    public class GeofenceErrorMessages
    {
        private GeofenceErrorMessages()
        {
        }

        /// <summary>
        /// GeofenceExceptionからエラー文字列を返します。
        /// </summary>
        /// <returns>The error string.</returns>
        /// <param name="context">Context.</param>
        /// <param name="e">E.</param>
        public static string GetErrorString(Context context, Exception e)
        {
            if (e is ApiException) {
                return GetErrorString(context, ((ApiException)e).StatusCode);
            } else {
                return "Unknown geofence error.";
            }
        }

        /// <summary>
        /// エラー文字列を返します。
        /// </summary>
        /// <returns>The error string.</returns>
        /// <param name="context">Context.</param>
        /// <param name="errorCode">Error code.</param>
        public static String GetErrorString(Context context, int errorCode)
        {
            switch (errorCode)
            {
                case GeofenceStatusCodes.GeofenceNotAvailable:
                    return "Geofence Not Available";
                case GeofenceStatusCodes.GeofenceTooManyGeofences:
                    return "Geofence Too Many Geofences";
                case GeofenceStatusCodes.GeofenceTooManyPendingIntents:
                    return "Geofence Too Many Pending Intents";
                default:
                    return "Geofence Unknown Error";
            }
        }
    }
}
