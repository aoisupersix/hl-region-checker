using HLRegionChecker.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace HLRegionChecker.Converters
{
    /// <summary>
    /// ステータスIDから表示名に変換するコンバータクラス
    /// </summary>
    public class StatusNameConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stateId = (int)value;
            var state = DbModel.Instance.GetStatusNameForId(stateId);
            return state is StateModel ? state.Value.Name : "オフライン";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
