using System;

namespace HLRegionChecker.Models
{
    /// <summary>
    /// データベースのメンバー情報のモデルクラスです。
    /// </summary>
    public struct MemberModel
    {
        /// <summary>
        /// メンバーID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// メンバーの表示名(フルネーム)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// メンバーの名字
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// メンバーの名前
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// メンバーのステータスID
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// ステータスの最終更新時間
        /// </summary>
        /// <value>The last update date.</value>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// ステータスの最終更新方法が自動か？
        /// </summary>
        /// <value><c>true</c> if last update is auto; otherwise, <c>false</c>.</value>
        public bool LastUpdateIsAuto { get; set; }
    }
}
