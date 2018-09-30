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
        /// DB上のカラム名
        /// </summary>
        public string DbIdentifierName { get => Identifier.Replace(BundleIdentifier, "").Replace(".", ""); }

        public GeofenceRegion(string identifier, double lat, double lon, double rad) : base(identifier)
        {
            Latitude = lat;
            Longitude = lon;
            Radius = rad;
        }
    }
}
