
using System.ComponentModel;
using System.Reflection;

namespace HLRegionChecker.Const
{
    /// <summary>
    /// 仮想領域の識別子
    /// </summary>
    public enum Region
    {
        [Description("org.hykwlab.hlregionchecker.region-laboratory")]
        研究室,
        [Description("org.hykwlab.hlregionchecker.region-campus")]
        学内,
    }

    /// <summary>
    /// 仮想領域の拡張
    /// </summary>
    public static partial class RegionUtil
    {
        /// <summary>
        /// 仮想領域の識別子をアトリビュートから取得します。
        /// </summary>
        /// <param name="val">仮想領域</param>
        /// <returns>仮想領域の識別子</returns>
        public static string GetIdentifier(this Region val)
        {
            FieldInfo fi = val.GetType().GetField(val.ToString());
            var attribute = (DescriptionAttribute)fi.GetCustomAttribute(typeof(DescriptionAttribute), false);
            if (attribute != null)
                return attribute.Description;
            return val.ToString();
        }
    }

    /// <summary>
    /// ジオフェンス領域関係の定数クラス
    /// </summary>
    public static class RegionConst
    {
        /// <summary>
        /// 研究室ビーコンのUUID
        /// </summary>
        public const string BEACON_UUID = "2F0B0D9B-B52C-47BF-B5B8-2BFBCE094653";

        /// <summary>
        /// 研究室ビーコンのメジャー値
        /// </summary>
        public const int BEACON_MAJOR = 1;

        /// <summary>
        /// 研究室ビーコンのマイナー値
        /// </summary>
        public const int BEACON_MINOR = 1;

        /// <summary>
        /// 学内領域の緯度
        /// </summary>
        public const double CAMPUS_LATITUDE = 35.626514;

        /// <summary>
        /// 学内領域の経度
        /// </summary>
        public const double CAMPUS_LONGITUDE = 139.279283;

        /// <summary>
        /// 学内領域の半径[m]
        /// </summary>
        public const double CAMPUS_RADIUS = 400;
    }
}
