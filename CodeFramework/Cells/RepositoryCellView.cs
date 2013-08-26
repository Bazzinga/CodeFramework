using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using CodeFramework.Views;

namespace CodeFramework.Cells
{

    public partial class RepositoryCellView : UITableViewCell
    {
        public static UIImage User;
        public static UIImage Heart;
        public static UIImage Fork;

        public static bool RoundImages = true;

        public static RepositoryCellView Create()
        {
            var cell = new RepositoryCellView();
            var views = NSBundle.MainBundle.LoadNib("RepositoryCellView", cell, null);
            cell = Runtime.GetNSObject( views.ValueAt(0) ) as RepositoryCellView;

            if (cell != null)
            {
                cell.Image1.Image = Heart;
                cell.Image3.Image = Fork;
                cell.UserImage.Image = User;
                cell.BackgroundView = new MonoTouch.Dialog.CellBackgroundView();

                if (RoundImages)
                {
                    cell.BigImage.Layer.MasksToBounds = true;
                    cell.BigImage.Layer.CornerRadius = cell.BigImage.Bounds.Height / 2f;
                }
            }

            //Create the icons
            return cell;
        }


        public RepositoryCellView()
        {
        }

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
        }

        public void Bind(string name, string name2, string name3, string description, string repoOwner, UIImage logoImage)
        {
            Caption.Text = name;
            Label1.Text = name2;
            Label3.Text = name3;
            BigImage.Image = logoImage;
            Description.Hidden = description == null;
            Description.Text = description ?? string.Empty;

            var frame = Description.Frame;
            frame.Y = 29f;
            frame.Height = this.Bounds.Height - frame.Y - 16f - 12f;
            Description.Frame = frame;

            RepoName.Hidden = repoOwner == null;
            UserImage.Hidden = RepoName.Hidden;
            RepoName.Text = repoOwner != null ? repoOwner : string.Empty;
        }
    }
}

