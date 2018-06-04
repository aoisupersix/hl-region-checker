using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using HLRegionChecker.Models;

using Firebase.Database;
using HLRegionChecker.iOS.DependencyServices;
using Foundation;

[assembly: Dependency(typeof(DbAdapter_iOS))]
namespace HLRegionChecker.iOS.DependencyServices
{
    public class DbAdapter_iOS : IDbAdapter
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
        private List<MemberModel> ConvertToMemberModels(NSEnumerator members)
        {
            List<MemberModel> memModels = new List<MemberModel>();
            while (members.NextObject() is DataSnapshot member)
            {
                memModels.Add(new MemberModel()
                {
                    Id = int.Parse(member.Key),
                    Name = member.GetChildSnapshot("name").GetValue().ToString(),
                    Status = int.Parse(member.GetChildSnapshot("status").GetValue().ToString())
                });
            }

            return memModels.OrderBy(x => x.Id).ToList();
        }

        /// <summary>
        /// Firebaseから取得したNSEnumeratorをStateModelと_stateに代入します。
        /// </summary>
        /// <param name="member">ステータス情報</param>
        /// <returns>ステータス情報を代入したStateModel</returns>
        private List<StateModel> ConvertToStateModels(NSEnumerator states)
        {
            _states.Clear();
            while (states.NextObject() is DataSnapshot state)
            {
                _states.Add(new StateModel()
                {
                    Id = int.Parse(state.Key),
                    Name = state.GetChildSnapshot("name").GetValue().ToString(),
                    Color = state.GetChildSnapshot("color").GetValue().ToString(),
                    BgColor_Hex = state.GetChildSnapshot("hcolor-bg").GetValue().ToString(),
                    TextColor_Hex = state.GetChildSnapshot("hcolor-text").GetValue().ToString(),
                });
            }

            return _states.OrderBy(x => x.Id).ToList();
        }

        /// <summary>
        /// 初期化処理を行います。
        /// </summary>
        void IDbAdapter.InitDb()
        {
            var rootRef = Database.DefaultInstance.GetRootReference();
            rootRef.ObserveSingleEvent(DataEventType.Value, (datasnapshot, prevChild) =>
            {
                //初期値取得
                var members = datasnapshot.GetChildSnapshot("members").Children;
                var states = datasnapshot.GetChildSnapshot("status").Children;

                //初期値送信
                InitializedDb?.Invoke(
                    this,
                    ConvertToMemberModels(members),
                    ConvertToStateModels(states)
                );

                //更新イベントハンドラ登録
                var memRef = datasnapshot.GetChildSnapshot("members").Reference;
                var memUpdateHandle = memRef.ObserveEvent(DataEventType.Value, (memSnapshot, memChild) =>
                {
                    var mems = memSnapshot.Children;
                    //更新
                    UpdatedMembers?.Invoke(
                        this,
                        ConvertToMemberModels(mems)
                    );
                });
            });
        }

        /// <summary>
        /// デタッチ処理を行います。
        /// </summary>
        void IDbAdapter.Disappear()
        {
            var rootRef = Database.DefaultInstance.GetRootReference();
            var memRef = rootRef.GetChild("members").Reference;
            memRef.RemoveAllObservers();
        }

        /// <summary>
        /// ステータス情報をIDから取得します。
        /// </summary>
        /// <param name="stateId">ステータスID</param>
        StateModel? IDbAdapter.GetStatusForId(int stateId)
        {
            return _states.Where(x => x.Id == stateId).First();
        }

        /// <summary>
        /// 引数に与えられたメンバーのステータスを更新します。
        /// </summary>
        /// <param name="memberId">更新するメンバーのID</param>
        /// <param name="stateId">更新ステータスID</param>
        void IDbAdapter.UpdateStatus(int memberId, int stateId)
        {
            //ステータスIDが含まれているかのチェック
            if (!_states.Select(x => x.Id).Contains(stateId))
                return;

            //更新情報の用意
            var childDict = new NSDictionary("status", stateId);

            //更新
            var rootRef = Database.DefaultInstance.GetRootReference();
            var memRef = rootRef.GetChild("members");
            memRef.GetChild(memberId.ToString()).UpdateChildValues(childDict);
        }
    }
}