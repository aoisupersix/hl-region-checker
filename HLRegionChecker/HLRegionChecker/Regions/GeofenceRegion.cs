using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Regions
{
    /// <summary>
    /// ジオフェンス領域
    /// </summary>
    public class GeofenceRegion : RegionBase
    {
        /// <summary>
        /// 緯度
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// 経度
        /// </summary>
        public double Longitude { get; set; }
        
        /// <summary>
        /// 領域の半径
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// 領域の表示名（名前と識別子名）
        /// </summary>
        public string DisplayName => $"{Name}({DbIdentifierName})";

        /// <summary>
        /// 領域の情報
        /// </summary>
        public string Description => $"Lat:{Latitude},Lon:{Longitude},Radius:{Radius}";

        /// <summary>
        /// DB上のカラム名
        /// </summary>
        public string DbIdentifierName { get => Identifier.Replace(BundleIdentifier, "").Replace(".", ""); }

        public GeofenceRegion(string identifier, double lat, double lon, double rad) : base(identifier, "")
        {
            Latitude = lat;
            Longitude = lon;
            Radius = rad;
        }

        public GeofenceRegion(string identifier, string name, double lat, double lon, double rad) : base(identifier, name)
        {
            Latitude = lat;
            Longitude = lon;
            Radius = rad;
        }
    }
}
