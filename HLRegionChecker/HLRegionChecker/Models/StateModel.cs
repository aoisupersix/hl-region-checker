
namespace HLRegionChecker.Models
{
    /// <summary>
    /// データベースのステータス情報のモデルクラスです。
    /// </summary>
    public struct StateModel
    {
        /// <summary>
        /// ステータスID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ステータス名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ステータスのBootStrap用カラー
        /// </summary>
        public string Color { get; set; }
    }
}
