using CodeFramework.iOS;
using MonoTouch.UIKit;
using System.Drawing;

namespace CodeFramework.Views
{
    public class SearchFilterBar : UISearchBar
    {
        public static UIImage ButtonBackground;
        private readonly UIButton _button;

        public UIButton FilterButton { get { return _button; } }

        public bool FilterButtonVisible
        {
            get
            {
                return !_button.Hidden;
            }
            set
            {
                _button.Hidden = !value;
            }
        }

        public SearchFilterBar()
            : base(new RectangleF(0, 0, 320, 44f))
        {
            _button = new UIButton(UIButtonType.Custom);
            _button.SetBackgroundImage(ButtonBackground, UIControlState.Normal);

            if (MonoTouch.Utilities.iOSVersion.Item1 < 6)
            {
                _button.SetBackgroundImage(ButtonBackground, UIControlState.Selected);
                _button.SetBackgroundImage(ButtonBackground, UIControlState.Highlighted);
            }

            _button.SetImage(Theme.CurrentTheme.FilterButton, UIControlState.Normal);
            _button.SizeToFit();
            this.AddSubview(_button);
        }

        public override void LayoutSubviews()
        {
            this.AutosizesSubviews = true;
            var bounds = this.Bounds;
            base.LayoutSubviews();

            if (!FilterButtonVisible) return;

            const float buttonWidth = 44f;
            _button.Frame = new RectangleF(bounds.Width - 5 - buttonWidth, 7f, buttonWidth, 31f);

            var searchBar = this.Subviews.GetValue(1) as UIView;
            if (searchBar == null) return;
            var frame = searchBar.Frame;
            frame.Width -= (_button.Frame.Width + 10f);
            searchBar.Frame = frame;
        }
    }

}

