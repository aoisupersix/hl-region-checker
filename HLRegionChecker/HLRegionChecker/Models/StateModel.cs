
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

        /// <summary>
        /// 背景のHTMLカラー
        /// </summary>
        public string BgColor_Hex { get; set; }

        /// <summary>
        /// テキストのHTMLカラー
        /// </summary>
        public string TextColor_Hex { get; set; }

        public object this[string propertyName]
        {
            get
            {
                return typeof(StateModel).GetProperty(propertyName).GetValue(this);
            }

            set
            {
                typeof(StateModel).GetProperty(propertyName).SetValue(this, value);
            }
        }
    }
}
