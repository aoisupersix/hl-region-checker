using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HLRegionChecker.Controls
{
    public class LinkLabel: Label
    {
        public static readonly BindableProperty LinkTextColorProperty = BindableProperty.Create(
            "LinkTextColor", typeof(Color), typeof(LinkLabel), Color.Default
        );

        public Color LinkTextColor
        {
            get { return (Color)GetValue(LinkTextColorProperty); }
            set { SetValue(LinkTextColorProperty, value); }
        }
    }
}
