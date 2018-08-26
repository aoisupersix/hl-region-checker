using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Interfaces
{
    /// <summary>
    /// DBの初期値取得イベントハンドラです。
    /// メンバー情報とステータス情報が含まれています。
    /// </summary>
    /// <param name="sender">イベントの発火元</param>
    /// <param name="members">メンバー情報</param>
    /// <param name="states">ステータス情報</param>
    public delegate void DbInitializeEventHandler(object sender, List<Models.MemberModel> members, List<Models.StateModel> states);

    /// <summary>
    /// DBのメンバー情報更新イベントハンドラです。
    /// </summary>
    /// <param name="sender">イベントの発火元</param>
    /// <param name="members">メンバー情報</param>
    public delegate void DbUpdateEventHandler(object sender, List<Models.MemberModel> members);

    /// <summary>
    /// データベースとのやり取りを行う共通のインターフェースです。
    /// </summary>
    public interface IDbAdapter
    {
        /// <summary>
        /// 初期化処理を行います。
        /// </summary>
        void InitDb();

        /// <summary>
        /// デタッチ処理を行います。
        /// </summary>
        void Disappear();

        /// <summary>
        /// ステータス情報をIDから取得します。
        /// </summary>
        /// <param name="stateId">ステータスID</param>
        Models.StateModel? GetStatusForId(int stateId);

        /// <summary>
        /// 引数に与えられたメンバーのステータスを更新します。
        /// </summary>
        /// <param name="memberId">更新するメンバーのID</param>
        /// <param name="stateId">更新ステータスID</param>
        /// <param name="autoUpdateFlg">自動更新か？</param>
        void UpdateStatus(int memberId, int stateId, bool autoUpdateFlg);

        /// <summary>
        /// データベースの初期値を取得した際のイベントハンドラです。
        /// </summary>
        event DbInitializeEventHandler InitializedDb;

        /// <summary>
        /// データベースのメンバー情報が更新された際のイベントハンドラです。
        /// </summary>
        event DbUpdateEventHandler UpdatedMembers;
    }
}
