
namespace HLRegionChecker.Const
{
    /// <summary>
    /// ステータスID
    /// ステータスのマスタはDBから取得すること
    /// </summary>
    public enum Status
    {
        帰宅 = 0,
        学内 = 1,
        在室 = 2
    }

    /// <summary>
    /// ステータスIDの拡張
    /// </summary>
    public static partial class StatusUtil
    {
        /// <summary>
        /// ステータスIDを取得します。
        /// </summary>
        /// <param name="val">ステータス</param>
        /// <returns>ステータスID</returns>
        public static int GetStatusId(this Status val)
        {
            return (int)val;
        }
    }
}
