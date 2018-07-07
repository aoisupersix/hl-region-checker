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
using Firebase.Database;
using HLRegionChecker.Droid.DependencyServices;
using Reactive.Bindings;

[assembly: Dependency(typeof(DbAdapter_Droid))]
namespace HLRegionChecker.Droid.DependencyServices
{
    public class DbAdapter_Droid: IDbAdapter
    {
        #region フィールド
        /// <summary>
        /// 初期値取得用通知クラス
        /// </summary>
        private DataSnapshotChangedNotifier _initDataNotifier = new DataSnapshotChangedNotifier();

        /// <summary>
        /// メンバー情報更新用通知クラス
        /// </summary>
        private DataSnapshotChangedNotifier _updateDataNotifier = new DataSnapshotChangedNotifier();

        /// <summary>
        /// データベースの初期値を取得した際のイベントハンドラです。
        /// </summary>
        public event DbInitializeEventHandler InitializedDb;

        /// <summary>
        /// データベースのメンバー情報が更新された際のイベントハンドラです。
        /// </summary>
        public event DbUpdateEventHandler UpdatedMembers;
        #endregion フィールド

        #region プロパティ
        /// <summary>
        /// メンバー情報
        /// </summary>
        public ReactiveProperty<List<MemberModel>> Members { get; } = new ReactiveProperty<List<MemberModel>>();
        /// <summary>
        /// ステータス情報(未使用)
        /// </summary>
        public ReactiveProperty<List<StateModel>> States { get; } = new ReactiveProperty<List<StateModel>>();
        #endregion プロパティ

        #region メソッド
        /// <summary>
        /// Firebaseから取得したDataSnapshotをMemberModelに代入します。
        /// </summary>
        /// <param name="memRef">メンバー情報</param>
        /// <returns>メンバー情報を代入したMemberModel</returns>
        private List<MemberModel> ConvertToMemberModels(DataSnapshot memRef)
        {
            if (memRef == null)
                return null;

            List<MemberModel> memModels = new List<MemberModel>();
            foreach (DataSnapshot member in memRef.Children.ToEnumerable())
            {
                var last_name = member.Child("last_name").GetValue(true).ToString();
                var first_name = member.Child("first_name").GetValue(true).ToString();
                memModels.Add(new MemberModel()
                {
                    Id = int.Parse(member.Key),
                    Name = $"{last_name} {first_name}",
                    LastName = last_name,
                    FirstName = first_name,
                    Status = int.Parse(member.Child("status").GetValue(true).ToString())
                });
            }
            return memModels.OrderBy(x => x.Id).ToList();
        }

        /// <summary>
        /// Firebaseから取得したDataSnapshotをStateModelに代入します。
        /// </summary>
        /// <param name="stateRef">ステータス情報</param>
        /// <returns>ステータス情報を代入したStateModel</returns>
        private List<StateModel> ConvertToStateModels(DataSnapshot stateRef)
        {
            if (stateRef == null)
                return null;

            List<StateModel> stateModels = new List<StateModel>();
            foreach (DataSnapshot state in stateRef.Children.ToEnumerable())
            {
                stateModels.Add(new StateModel()
                {
                    Id = int.Parse(state.Key),
                    Name = state.Child("name").GetValue(true).ToString(),
                    Color = state.Child("color").GetValue(true).ToString(),
                    BgColor_Hex = state.Child("hcolor-bg").GetValue(true).ToString(),
                    TextColor_Hex = state.Child("hcolor-text").GetValue(true).ToString(),
                });
            }

            return stateModels.OrderBy(x => x.Id).ToList();
        }
        #endregion メソッド

        #region インタフェース実装
        /// <summary>
        /// 初期化処理を行います。
        /// </summary>
        void IDbAdapter.InitDb()
        {
            //初期化処理登録
            FirebaseDatabase.Instance.Reference.Root.AddValueEventListener(_initDataNotifier);
            _initDataNotifier.ChangedData.Subscribe(snap =>
            {
                if (snap == null)
                    return;

                var members = ConvertToMemberModels(snap.Child("members")) ?? new List<MemberModel>();
                var states = ConvertToStateModels(snap.Child("status")) ?? new List<StateModel>();
                if(members.Any() && states.Any())
                {
                    Members.Value = members;
                    States.Value = states;
                    InitializedDb?.Invoke(this, members, states);
                }
            });

            //更新処理登録
            FirebaseDatabase.Instance.GetReference("members").AddValueEventListener(_updateDataNotifier);
            _updateDataNotifier.ChangedData.Subscribe(snap =>
            {
                if (snap == null)
                    return;

                var members = ConvertToMemberModels(snap) ?? new List<MemberModel>();
                if (members.Any())
                {
                    Members.Value = members;
                    UpdatedMembers?.Invoke(this, members);
                }
            });
        }

        /// <summary>
        /// デタッチ処理を行います。
        /// </summary>
        void IDbAdapter.Disappear()
        {
            Members.Dispose();
            States.Dispose();

            var rootRef = FirebaseDatabase.Instance.Reference.Root;
            rootRef.RemoveEventListener(_initDataNotifier);
            _initDataNotifier.Dispose();

            rootRef.Child("members").RemoveEventListener(_updateDataNotifier);
            _updateDataNotifier.Dispose();
        }

        /// <summary>
        /// ステータス情報をIDから取得します。
        /// </summary>
        /// <param name="stateId">ステータスID</param>
        StateModel? IDbAdapter.GetStatusForId(int stateId)
        {
            return States.Value.Where(x => x.Id == stateId).First();
        }

        /// <summary>
        /// 引数に与えられたメンバーのステータスを更新します。
        /// </summary>
        /// <param name="memberId">更新するメンバーのID</param>
        /// <param name="stateId">更新ステータスID</param>
        void IDbAdapter.UpdateStatus(int memberId, int stateId, bool autoUpdateFlg = false)
        {
            //ステータスIDが含まれているかのチェック
            if (!States.Value.Select(x => x.Id).Contains(stateId))
                return;

            //更新情報の用意
            var childDict = new Dictionary<string, Java.Lang.Object>();
            childDict.Add("status", stateId);

            //更新
            var memRef = FirebaseDatabase.Instance.GetReference("members");
            memRef.Child(memberId.ToString()).UpdateChildren(childDict);
        }
        #endregion インタフェース実装
    }

    /// <summary>
    /// DBのDataSnapshot変更通知を行うクラス
    /// </summary>
    class DataSnapshotChangedNotifier : Java.Lang.Object, IValueEventListener
    {
        /// <summary>
        /// 変更データ
        /// </summary>
        public ReactiveProperty<DataSnapshot> ChangedData { get; } = new ReactiveProperty<DataSnapshot>();

        public void OnCancelled(DatabaseError error) { }
        public void OnDataChange(DataSnapshot snapshot)
        {
            ChangedData.Value = snapshot;
        }

        public new void Dispose()
        {
            ChangedData.Dispose();
            base.Dispose();
        }
    }
}