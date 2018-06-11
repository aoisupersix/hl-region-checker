using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLRegionChecker.Models;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using HLRegionChecker.Droid.DependencyServices;



[assembly: Dependency(typeof(DbAdapter_Droid))]
namespace HLRegionChecker.Droid.DependencyServices
{
    public class DbAdapter_Droid: IDbAdapter
    {
        /// <summary>
        /// ステータス情報
        /// </summary>
        private static List<StateModel> _states = new List<StateModel>();

        /// <summary>
        /// データベースの初期値を取得した際のイベントハンドラです。
        /// </summary>
        public event DbInitializeEventHandler InitializedDb;

        /// <summary>
        /// データベースのメンバー情報が更新された際のイベントハンドラです。
        /// </summary>
        public event DbUpdateEventHandler UpdatedMembers;

        /// <summary>
        /// Firebaseから取得したNSEnumeratorをMemberModelに代入します。
        /// </summary>
        /// <param name="member">メンバー情報</param>
        /// <returns>メンバー情報を代入したMemberModel</returns>
        private List<MemberModel> ConvertToMemberModels()
        {
            return null;
        }

        /// <summary>
        /// Firebaseから取得したNSEnumeratorをStateModelと_stateに代入します。
        /// </summary>
        /// <param name="member">ステータス情報</param>
        /// <returns>ステータス情報を代入したStateModel</returns>
        private List<StateModel> ConvertToStateModels()
        {
            InitializedDb.Invoke(this, null, null);
            UpdatedMembers.Invoke(this, null);
            return null;
        }

        /// <summary>
        /// 初期化処理を行います。
        /// </summary>
        void IDbAdapter.InitDb()
        {
        }

        /// <summary>
        /// デタッチ処理を行います。
        /// </summary>
        void IDbAdapter.Disappear()
        {
        }

        /// <summary>
        /// ステータス情報をIDから取得します。
        /// </summary>
        /// <param name="stateId">ステータスID</param>
        StateModel? IDbAdapter.GetStatusForId(int stateId)
        {
            return null;
        }

        /// <summary>
        /// 引数に与えられたメンバーのステータスを更新します。
        /// </summary>
        /// <param name="memberId">更新するメンバーのID</param>
        /// <param name="stateId">更新ステータスID</param>
        void IDbAdapter.UpdateStatus(int memberId, int stateId)
        {
        }
    }
}