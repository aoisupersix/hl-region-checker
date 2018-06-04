using HLRegionChecker.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace HLRegionChecker.Converters
{
    /// <summary>
    /// ステータスIDからparameterに指定されたプロパティ値に変換するコンバータクラス
    /// </summary>
    public class StatusPropertyConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stateId = (int)value;
            var state = DbModel.Instance.GetStatusNameForId(stateId).Value;
            var property = state[parameter.ToString()];
            return property;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
