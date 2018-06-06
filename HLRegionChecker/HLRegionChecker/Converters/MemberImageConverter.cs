using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace HLRegionChecker.Converters
{
    /// <summary>
    /// メンバーIDからイメージに変換するコンバータクラス
    /// </summary>
    public class MemberImageConverter: IValueConverter
    {
        private static ImageSource memberImage = ImageSource.FromResource("HLRegionChecker.Resources.Icon_Dog.png");
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var memberId = (int)value;
            //とりあえず全員同じイメージを表示する
            return memberImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
