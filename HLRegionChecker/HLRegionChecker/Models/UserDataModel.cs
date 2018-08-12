using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace HLRegionChecker.Models
{
    /// <summary>
    /// ユーザデータの永続化を行うマネージャクラス
    /// </summary>
    public class UserDataModel: BindableBase
    {
        #region AppSettings

        private static Lazy<ISettings> _appSettings;

        public static ISettings AppSettings
        {
            get
            {
                if (_appSettings == null)
                    _appSettings = new Lazy<ISettings>(() => CrossSettings.Current, LazyThreadSafetyMode.PublicationOnly);

                return _appSettings.Value;
            }
            set
            {
                _appSettings = new Lazy<ISettings>(() => value, LazyThreadSafetyMode.PublicationOnly);
            }
        }
        #endregion

        #region インスタンス
        /// <summary>
        /// インスタンス
        /// </summary>
        private static UserDataModel _instance;

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
        #endregion

        /// <summary>
        /// 永続化用のキー
        /// </summary>
        private const string PROPERTY_KEY_MEMBER_ID = "memberid";

        public static readonly int DefaultMemberId = -1;

        private int _memberId;

        /// <summary>
        /// メンバーID
        /// </summary>
        public int MemberId
        {
            get => AppSettings.GetValueOrDefault(PROPERTY_KEY_MEMBER_ID, DefaultMemberId);
            set
            {
                AppSettings.AddOrUpdateValue(PROPERTY_KEY_MEMBER_ID, value);
                _memberId = value;
                SetProperty(ref _memberId, value);
            }
        }
    }
}
