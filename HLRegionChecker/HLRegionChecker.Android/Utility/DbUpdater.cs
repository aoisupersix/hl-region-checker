using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using HLRegionChecker.Models;

namespace HLRegionChecker.Droid.Utility
{
    /// <summary>
    /// Droid固有のDB更新
    /// </summary>
    public static class DbUpdater
    {
        /// <summary>
        /// ステータス情報を更新します。
        /// </summary>
        /// <param name="stateId">更新するステータスID</param>
        public static void UpdateStatus(int stateId)
        {
            var memId = UserDataModel.Instance.MemberId;
            if (memId == UserDataModel.DefaultMemberId)
                return;

            //ステータスの更新処理
            var childDict = new Dictionary<string, Java.Lang.Object>();
            childDict.Add("status", stateId);
            childDict.Add("last_update_is_auto", true);

            //更新
            var memRef = FirebaseDatabase.Instance.GetReference("members");
            memRef.Child(memId.ToString()).UpdateChildren(childDict);
        }
    }
}