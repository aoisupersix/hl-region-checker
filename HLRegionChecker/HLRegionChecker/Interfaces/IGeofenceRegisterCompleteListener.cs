using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Interfaces
{
    /// <summary>
    /// Androidのジオフェンス登録完了コールバックリスナー
    /// </summary>
    public interface IGeofenceRegisterCompleteListener
    {
        /// <summary>
        /// ジオフェンスの登録が完了した際に呼び出されます。
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="message"></param>
        void RegisterCompleted(bool isSuccess, string message);
    }
}
