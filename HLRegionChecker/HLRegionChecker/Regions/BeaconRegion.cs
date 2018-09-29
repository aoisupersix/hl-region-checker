using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Regions
{
    /// <summary>
    /// ビーコン領域
    /// </summary>
    public class BeaconRegion : RegionBase
    {
        /// <summary>
        /// UUID
        /// </summary>
        public string Uuid { get; set; }

        /// <summary>
        /// メジャー値
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// マイナー値
        /// </summary>
        public int Minor { get; set; }

        public BeaconRegion(string identifier, string uuid, int major, int minor) : base(identifier)
        {
            Uuid = uuid;
            Major = major;
            Minor = minor;
        }
    }
}
