using System;
using System.Collections.Generic;
using System.Linq;

namespace HLRegionChecker.Regions
{
    /// <summary>
    /// 領域の実際の定義
    /// ジオフェンス定義の識別子名はfunctionsと連動させること。
    /// </summary>
    public class RegionList
    {

        public static RegionBase GetRegionFromIdentifier(string identifier)
        {
            if (identifier.Equals(研究室.Identifier))
                return 研究室;

            var region = CampusAllRegions.Where(r => identifier.Equals(r.Identifier));
            if (!region.Any())
                return null;

            return region.First(); // 識別子は一意なので必ず一つに定まるはず
        }

        /// <summary>
        /// 研究室のビーコン領域
        /// </summary>
        public static BeaconRegion 研究室 = new BeaconRegion("org.hykwlab.hlregionchecker.region-laboratory", "2F0B0D9B-B52C-47BF-B5B8-2BFBCE094653", 1, 1);

        /// <summary>
        /// 駐輪場付近のジオフェンス領域
        /// </summary>
        public static GeofenceRegion キャンパス駐輪場 = new GeofenceRegion("org.hykwlab.hlregionchecker.region-campus-1", 35.63123, 139.28226, 100);

        /// <summary>
        /// グランド・A館付近のジオフェンス領域
        /// </summary>
        public static GeofenceRegion キャンパスグラウンド = new GeofenceRegion("org.hykwlab.hlregionchecker.region-campus-2", 35.62977, 139.28065, 200);

        /// <summary>
        /// D館・工学部棟付近のジオフェンス領域
        /// </summary>
        public static GeofenceRegion キャンパス工学部棟 = new GeofenceRegion("org.hykwlab.hlregionchecker.region-campus-3", 35.62561, 139.27954, 400);

        /// <summary>
        /// 学内領域
        /// </summary>
        public static IEnumerable<GeofenceRegion> CampusAllRegions
        {
            get => new[] { キャンパス駐輪場, キャンパスグラウンド, キャンパス工学部棟 };
        }
    }
}
