using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Models
{
    /// <summary>
    /// ジオフェンス状態のモデルクラスです。
    /// </summary>
    public class GeofenceStatusModel
    {
        /// <summary>
        /// ジオフェンス識別子（サーバサイド）
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 領域に侵入しているか？
        /// </summary>
        public bool IsEnter { get; set; }
    }
}
