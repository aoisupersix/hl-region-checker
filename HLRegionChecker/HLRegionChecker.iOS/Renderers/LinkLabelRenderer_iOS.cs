using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using Foundation;
using UIKit;

using HLRegionChecker.iOS.Renderers;
using HLRegionChecker.Controls;

[assembly: ExportRenderer(typeof(LinkLabel), typeof(LinkLabelRenderer_iOS))]
namespace HLRegionChecker.iOS.Renderers
{
    public class LinkLabelRenderer_iOS: LabelRenderer
    {
        readonly List<Match> Matches = new List<Match>();

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            Control.UserInteractionEnabled = true;
            Control.AddGestureRecognizer(new UITapGestureRecognizer(this.OnTapUrl));

            UpdateLinks();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(this.Element.Text))
            {
                UpdateLinks();
            }
        }

        protected void UpdateLinks()
        {
            var element = Element as LinkLabel;

            var attributedString = new NSMutableAttributedString(Element.Text, Control.Font, Element.TextColor.ToUIColor());

            attributedString.AddAttribute(
                UIStringAttributeKey.ForegroundColor,
                Element.TextColor.Equals(Color.Default) ? Color.Black.ToUIColor() : Element.TextColor.ToUIColor(),
                new NSRange(0, Element.Text.Length)
            );

            var matches = Regex.Matches(Element.Text, @"https?://[-_.!~*'a-zA-Z0-9;/?:@&=+$,%#]+");

            foreach (Match match in matches)
            {
                var range = new NSRange(match.Index, match.Length);
                attributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, element.LinkTextColor.ToUIColor(), range);
                this.Matches.Add(match);
            }

            Control.AttributedText = attributedString;
        }

        protected void OnTapUrl(UIGestureRecognizer tap)
        {
            var location = tap.LocationInView(Control);
            var textView = new UITextView(Control.Frame);

            textView.TextContainer.LineFragmentPadding = 0;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.AttributedText = Control.AttributedText;

            var position = textView.GetOffsetFromPosition(
                textView.BeginningOfDocument,
                textView.GetClosestPositionToPoint(location)
            );

            var url = this.Matches.FirstOrDefault(m =>
                m.Index <= position && position <= (m.Index + m.Length)
            );

            if (url != null)
            {
                UIApplication.SharedApplication.OpenUrl(new Uri(url.Value));
            }
        }
    }
}