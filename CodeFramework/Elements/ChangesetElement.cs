using System;
using MonoTouch.Dialog;
using CodeFramework.Views;

namespace CodeFramework.Elements
{
    public class ChangesetElement : SubcaptionElement
    {
        private int? _added;
        private int? _removed;

        public ChangesetElement(string title, string subtitle, int? added, int? removed)
            : base(title, subtitle)
        {
            Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;
            LineBreakMode = MonoTouch.UIKit.UILineBreakMode.TailTruncation;
            Lines = 1;
            _added = added;
            _removed = removed;
        }

        protected override MonoTouch.UIKit.UITableViewCell CreateTableViewCell(MonoTouch.UIKit.UITableViewCellStyle style, string key)
        {
            return new ChangesetCell(key);
        }

        public override MonoTouch.UIKit.UITableViewCell GetCell(MonoTouch.UIKit.UITableView tv)
        {
            var cell = base.GetCell(tv);
            var addRemove = ((ChangesetCell)cell).AddRemoveView;
            addRemove.Added = _added;
            addRemove.Removed = _removed;
            return cell;
        }

        /// Bastardized version. I'll redo this code later...
        private class ChangesetCell : MonoTouch.UIKit.UITableViewCell
        {
            public AddRemoveView AddRemoveView { get; private set; }

            public ChangesetCell(string key)
                : base(MonoTouch.UIKit.UITableViewCellStyle.Subtitle, key)
            {
                AddRemoveView = new AddRemoveView();
                this.ContentView.AddSubview(AddRemoveView);
                TextLabel.LineBreakMode = MonoTouch.UIKit.UILineBreakMode.TailTruncation;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                var addRemoveX = ContentView.Frame.Width - 83f;
                AddRemoveView.Frame = new System.Drawing.RectangleF(addRemoveX, 12, 80f, 19f);

                var textFrame = TextLabel.Frame;
                textFrame.Width = addRemoveX - textFrame.X - 5f;
                TextLabel.Frame = textFrame;
            }
        }
    }
}

