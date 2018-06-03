using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace HLRegionChecker.Models
{
    /// <summary>
    /// ユーザデータの永続化を行うマネージャクラス
    /// </summary>
    public class UserDataModel : BindableBase
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        private static UserDataModel _instance;

        /// <summary>
        /// 永続化用のキー
        /// </summary>
        private const string PROPERTY_KEY_MEMBER_ID = "memberid";

        private int? _memberId;

        /// <summary>
        /// メンバーID
        /// </summary>
        public int? MemberId
        {
            get
            {
                if (Application.Current.Properties.ContainsKey(PROPERTY_KEY_MEMBER_ID))
                    return Application.Current.Properties[PROPERTY_KEY_MEMBER_ID] as int?;
                return null;
            }
            set
            {
                Application.Current.Properties[PROPERTY_KEY_MEMBER_ID] = value;
                Application.Current.SavePropertiesAsync();
                SetProperty(ref _memberId, value);
            }
        }

        /// <summary>
        /// クラスのインスタンスを取得します。
        /// </summary>
        public static UserDataModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserDataModel();

                return _instance;
            }
        }
    }
}
