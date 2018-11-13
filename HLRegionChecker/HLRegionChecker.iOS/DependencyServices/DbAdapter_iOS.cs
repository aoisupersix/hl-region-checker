using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using HLRegionChecker.Models;
using HLRegionChecker.Interfaces;

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
        /// メンバー情報
        /// </summary>
        private static List<MemberModel> _members = new List<MemberModel>();

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
            _members.Clear();
            while (members.NextObject() is DataSnapshot member)
            {
                var last_name = member.GetChildSnapshot("last_name").GetValue().ToString();
                var first_name = member.GetChildSnapshot("first_name").GetValue().ToString();
                var last_update_date = member.GetChildSnapshot("last_update_date").GetValue().ToString();
                var last_update_auto_flg = member.GetChildSnapshot("last_update_is_auto").GetValue().ToString();
                _members.Add(new MemberModel()
                {
                    Id = int.Parse(member.Key),
                    Name = $"{last_name} {first_name}",
                    LastName = last_name,
                    FirstName = first_name,
                    Status = int.Parse(member.GetChildSnapshot("status").GetValue().ToString()),
                    LastStatus = int.Parse(member.GetChildSnapshot("last_status").GetValue().ToString()),
                    LastUpdateDate = !last_update_date.Equals("") ? DateTime.Parse(last_update_date) : DateTime.MinValue,
                    LastUpdateIsAuto = Convert.ToBoolean(int.Parse(last_update_auto_flg))
                });
            }

            return _members.OrderBy(x => x.Id).ToList();
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
        public void InitDb()
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
        public void Disappear()
        {
            var rootRef = Database.DefaultInstance.GetRootReference();
            var memRef = rootRef.GetChild("members").Reference;
            memRef.RemoveAllObservers();
        }

        /// <summary>
        /// ステータス情報をIDから取得します。
        /// </summary>
        /// <param name="stateId">ステータスID</param>
        public StateModel? GetStatusForId(int stateId)
        {
            return _states.Where(x => x.Id == stateId).First();
        }

        /// <summary>
        /// 引数に与えられたメンバーのステータスを更新します。
        /// 手動更新とビーコンの自動更新以外（つまりはジオフェンス領域の判定）はサーバサイドで判定するので、このメソッドから更新しないでください。
        /// </summary>
        /// <param name="memberId">更新するメンバーのID</param>
        /// <param name="stateId">更新ステータスID</param>
        public void UpdateStatus(int memberId, int stateId, bool autoUpdateFlg)
        {
            //ステータスIDが含まれているかのチェック
            if (_states != null && !_states.Select(x => x.Id).Contains(stateId))
                return;
            
            // 更新情報の用意
            // 最終更新はFirebaseFunctionsで行うのでここでは行わない。
            var keys = new[]
            {
                "status",
                "last_update_is_auto",
            };
            var vals = new[]
            {
                NSObject.FromObject(stateId),
                NSObject.FromObject(autoUpdateFlg),
            };
            var childDict = NSDictionary.FromObjectsAndKeys(vals, keys, keys.Length);

            // 更新
            var rootRef = Database.DefaultInstance.GetRootReference();
            var memRef = rootRef.GetChild("members");
            memRef.GetChild(memberId.ToString()).UpdateChildValues(childDict);
        }

        /// <summary>
        /// 引数に与えられたデバイスのジオフェンス状態を更新します。
        /// ステータス判定と更新はジオフェンス状態に基づいて、サーバサイドで行われます。
        /// </summary>
        /// <param name="deviceIdentifier">デバイス識別子</param>
        /// <param name="dbGeofenceIdentifier">データベースのジオフェンス識別子</param>
        /// <param name="inTheArea">領域の範囲内かどうか（true: 領域内, false: 領域外)</param>
        public void UpdateGeofenceStatus(string deviceIdentifier, string dbGeofenceIdentifier, bool inTheArea)
        {
            // 更新情報の用意
            var keys = new[] { dbGeofenceIdentifier };
            var vals = new[] { NSObject.FromObject(inTheArea) };
            var childDict = NSDictionary.FromObjectsAndKeys(vals, keys, keys.Length);

            // 更新
            var rootRef = Database.DefaultInstance.GetRootReference();
            var devRef = rootRef.GetChild("devices");
            devRef.GetChild(deviceIdentifier).GetChild("geofence_status").UpdateChildValues(childDict);
        }

        /// <summary>
        /// デバイス情報を更新します。
        /// </summary>
        /// <param name="fcmToken">プッシュ通知用のトークン</param>
        /// <param name="memberId">デバイスに指定されているメンバーID</param>
        public void UpdateDeviceInfo(string fcmToken, int memberId)
        {
            var devId = UserDataModel.Instance.DeviceId;
            if (devId == null)
                return;

            // 更新情報の用意
            var keys = new List<Object>();
            var vals = new List<Object>();

            if(fcmToken != null)
            {
                keys.Add("fcm_token");
                vals.Add(fcmToken);
            }
            if(memberId != -1)
            {
                keys.Add("member_id");
                vals.Add(memberId);
            }

            // OS情報
            keys.Add("os");
            vals.Add("iOS");

            // バージョン情報
            keys.Add("version");
            vals.Add(new AssemblyService_iOS().GetVersionName());

            if (!keys.Any())
                return;

            var childDict = NSDictionary.FromObjectsAndKeys(vals.ToArray(), keys.ToArray(), keys.Count());

            // 更新
            var rootRef = Database.DefaultInstance.GetRootReference();
            var devRef = rootRef.GetChild("devices");
            devRef.GetChild(devId).UpdateChildValues(childDict);
        }

        /// <summary>
        /// デバイスログを追加します。
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        /// <param name="extra">追加情報</param>
        public void AddDeviceLog(string message, string extra = null)
        {
            var devId = UserDataModel.Instance.DeviceId;
            if (devId == null)
                return;

            // 更新情報の用意
            var keys = new List<Object>() { "date", "message", "extra" };
            var vals = new List<Object>()
            {
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                message,
                extra ?? "",
            };

            var childDict = NSDictionary.FromObjectsAndKeys(vals.ToArray(), keys.ToArray(), keys.Count());

            // 更新
            var rootRef = Database.DefaultInstance.GetRootReference();
            var devRef = rootRef.GetChild("devices");
            devRef.GetChild(devId).GetChild("Logs").GetChildByAutoId().UpdateChildValues(childDict);
        }
    }
}