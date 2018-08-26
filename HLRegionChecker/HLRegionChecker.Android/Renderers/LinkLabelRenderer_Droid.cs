using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Android.Text.Util;
using HLRegionChecker.Controls;
using HLRegionChecker.Droid.Renderers;

[assembly: ExportRenderer(typeof(LinkLabel), typeof(LinkLabelRenderer_Droid))]
namespace HLRegionChecker.Droid.Renderers
{
    public class LinkLabelRenderer_Droid: LabelRenderer
    {
        public LinkLabelRenderer_Droid(Android.Content.Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            var element = Element as LinkLabel;

            Linkify.AddLinks(this.Control, MatchOptions.WebUrls);
            this.Control.SetLinkTextColor(element.LinkTextColor.ToAndroid());
        }
    }
}