using System;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Bindings;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using CodeFramework.iOS;
using CodeFramework.iOS.Views;
using CodeFramework.Views;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.CoreGraphics;

namespace CodeFramework.ViewControllers
{
    public class BaseDialogViewController : DialogViewController, IMvxTouchView, IMvxEventSourceViewController
    {
        private UISearchBar _searchBar;

        public new string SearchPlaceholder
        {
            get { return base.SearchPlaceholder; }
            set
            {
                base.SearchPlaceholder = value;
                if (_searchBar != null)
                    _searchBar.Placeholder = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDialogViewController"/> class.
        /// </summary>
        /// <param name="push">If set to <c>true</c> push.</param>
        public BaseDialogViewController(bool push)
            : base(new RootElement(""), push)
        {
            this.AdaptForBinding();

            SearchPlaceholder = "Search".t();
            Autorotate = true;
            AutoHideSearch = true;
            Style = UITableViewStyle.Grouped;
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.BackButton, () => NavigationController.PopViewControllerAnimated(true)));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (NavigationController != null && IsSearching)
                NavigationController.SetNavigationBarHidden(true, true);
            if (IsSearching)
            {
                //This needs to be in the begin invoke because there's logic in the base class that
                //moves the scroll around. So by doing this we move this logic to execute after it.
                BeginInvokeOnMainThread(() => TableView.ScrollRectToVisible(new RectangleF(0, 0, 1, 1), false));
            }

            ViewWillAppearCalled.Raise(this, animated);
        }
        
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (NavigationController != null && NavigationController.NavigationBarHidden)
                NavigationController.SetNavigationBarHidden(false, true);
            
            if (IsSearching)
            {
                View.EndEditing(true);
                var searchBar = TableView.TableHeaderView as UISearchBar;
                if (searchBar != null)
                {
                    //Enable the cancel button again....
                    foreach (var s in searchBar.Subviews)
                    {
                        var x = s as UIButton;
                        if (x != null)
                            x.Enabled = true;
                    }
                }
            }

            ViewWillDisappearCalled.Raise(this, animated);

            //TableView.Scrolled -= HandleScrolled;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            GoogleAnalytics.GAI.SharedInstance.DefaultTracker.TrackView(this.GetType().Name);
            ViewDidAppearCalled.Raise(this, animated);
            //TableView.Scrolled += HandleScrolled;
        }

        void HandleScrolled (object sender, System.EventArgs e)
        {
            // Depending on how far the user scrolled, set the new offset.
            // Divide by a hundred so we have a sane value. You could adjust this
            // for different effects.
            // The larger you number divide by, the slower the shadow will change

            if (TableView.ContentOffset.Y != 0)
            {
                StartShadow();
            }
            else
            {
                StopShadow();
            }
//
//            float shadowOffset = (this.TableView.ContentOffset.Y/100);
//
////            // Make sure that the offset doesn't exceed 3 or drop below 0.5
////            shadowOffset = MIN(MAX(shadowOffset, 0), 3);
////
////            //Ensure that the shadow radius is between 1 and 3
////            float shadowRadius = MIN(MAX(shadowOffset, 1), 3);
//
//            //apply the offset and radius
//
//            NavigationController.NavigationBar.Layer.ShadowOffset = new SizeF(0, 1);
//            NavigationController.NavigationBar.Layer.ShadowRadius = 1f;
//            NavigationController.NavigationBar.Layer.ShadowColor = UIColor.DarkGray;
//            NavigationController.NavigationBar.Layer.ShadowOpacity = 0.8;
        }

        bool shadowing = false;
        private void StartShadow()
        {
            if (shadowing)
                return;

            NavigationController.NavigationBar.Layer.ShadowOffset = new SizeF(0, 1);
            NavigationController.NavigationBar.Layer.ShadowRadius = 1f;
            NavigationController.NavigationBar.Layer.ShadowColor = UIColor.DarkGray.CGColor;
            NavigationController.NavigationBar.Layer.ShadowOpacity = 0.5f;
            shadowing = true;
        }

        private void StopShadow()
        {
            if (!shadowing)
                return;

            NavigationController.NavigationBar.Layer.ShadowOffset = new SizeF(0, 0);
            NavigationController.NavigationBar.Layer.ShadowRadius = 0;
            NavigationController.NavigationBar.Layer.ShadowColor = UIColor.Clear.CGColor;
            NavigationController.NavigationBar.Layer.ShadowOpacity = 0.0f;
            shadowing = false;
        }

        /// <summary>
        /// Makes the refresh table header view.
        /// </summary>
        /// <returns>
        /// The refresh table header view.
        /// </returns>
        /// <param name='rect'>
        /// Rect.
        /// </param>
        public override RefreshTableHeaderView MakeRefreshTableHeaderView(RectangleF rect)
        {
            //Replace it with our own
            return new RefreshView(rect);
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Title != null && Root != null)
                Root.Caption = Title;

            TableView.BackgroundColor = UIColor.Clear;
            TableView.BackgroundView = null;
            if (Style != UITableViewStyle.Grouped)
            {
                TableView.TableFooterView = new DropbarView(View.Bounds.Width) {Hidden = true};
            }

            var backgroundView = new UIView { BackgroundColor = Theme.CurrentTheme.ViewBackgroundColor };
            backgroundView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            this.TableView.BackgroundView = backgroundView;

            ViewDidLoadCalled.Raise(this);
        }

        sealed class RefreshView : RefreshTableHeaderView
        {
            static readonly CGGradient BottomGradient;

            static RefreshView ()
            {
                using (var rgb = CGColorSpace.CreateDeviceRGB()){
                    float [] colorsBottom = {
                        1, 1, 1, 0f,
                        0.75f, 0.79f, 0.81f, .8f
                    };
                    BottomGradient = new CGGradient (rgb, colorsBottom, null);
                }
            }

            public RefreshView(RectangleF rect)
                : base(rect)
            {
                BackgroundColor = UIColor.Clear;
                StatusLabel.BackgroundColor = UIColor.Clear;
                StatusLabel.ShadowColor = UIColor.Clear;
                //StatusLabel.TextColor = UIColor.FromRGB(70, 70, 70);

                LastUpdateLabel.BackgroundColor = UIColor.Clear;
                LastUpdateLabel.ShadowColor = UIColor.Clear;
                //LastUpdateLabel.TextColor = UIColor.FromRGB(90, 90, 90);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                ArrowView.Frame = new RectangleF (20, Bounds.Height - 55, 20, 45);
            }

            public override void Draw(RectangleF rect)
            {
                var context = UIGraphics.GetCurrentContext();
                var bounds = Bounds;
                var midx = bounds.Width/2;

                //UIColor.FromRGB(232, 237, 240).SetColor ();
                //context.FillRect (bounds);
                context.DrawLinearGradient (BottomGradient, new PointF (midx, bounds.Height-100), new PointF (midx, bounds.Height), 0);

                base.Draw(rect);
            }
        }

        /// <summary>
        /// Called when the searching starts
        /// </summary>
        protected virtual void SearchStart()
        {
        }

        /// <summary>
        /// Called when the searching ends
        /// </summary>
        protected virtual void SearchEnd()
        {
        }

        /// <summary>
        /// Creates the search bar.
        /// </summary>
        /// <returns>The search bar.</returns>
        protected virtual UISearchBar CreateSearchBar()
        {
            return new UISearchBar(new RectangleF(0f, 0f, 320f, 44f)) { Delegate = new CustomSearchDelegate(this) };
        }

        /// <summary>
        /// Creates the header view.
        /// </summary>
        /// <returns>The header view.</returns>
        protected sealed override UISearchBar CreateHeaderView()
        {
            _searchBar = CreateSearchBar();
            _searchBar.Placeholder = SearchPlaceholder;
            return _searchBar;
        }
               
        protected class CustomSearchDelegate : UISearchBarDelegate
        {
            readonly BaseDialogViewController _container;
            DialogViewController _searchController;
            List<ElementContainer> _searchElements;

            static readonly UIColor NoItemColor = UIColor.FromRGBA(0.1f, 0.1f, 0.1f, 0.9f);

            class ElementContainer
            {
                public Element Element;
                public Element Parent;
            }

            public CustomSearchDelegate (BaseDialogViewController container)
            {
                _container = container;
            }

            public override void OnEditingStarted (UISearchBar searchBar)
            {
                _container.SearchStart();

                if (_searchController == null)
                {
                    _searchController = new DialogViewController(UITableViewStyle.Plain, null);
                    _searchController.LoadView();
                    _searchController.TableView.TableFooterView = new DropbarView(1f);
                }

                searchBar.ShowsCancelButton = true;
                _container.TableView.ScrollRectToVisible(new RectangleF(0, 0, 1, 1), false);
                _container.NavigationController.SetNavigationBarHidden(true, true);
                _container.IsSearching = true;
                _container.TableView.ScrollEnabled = false;

                if (_searchController.Root != null && _searchController.Root.Count > 0 && _searchController.Root[0].Count > 0)
                {
                    _searchController.TableView.TableFooterView.Hidden = false;
                    _searchController.View.BackgroundColor = UIColor.White;
                    _searchController.TableView.ScrollEnabled = true;
                }
                else
                {
                    _searchController.TableView.TableFooterView.Hidden = true;
                    _searchController.View.BackgroundColor = NoItemColor;
                    _searchController.TableView.ScrollEnabled = false;
                }

                _searchElements = new List<ElementContainer>();

                //Grab all the elements that we could search trhough
                foreach (var s in _container.Root)
                    foreach (var e in s.Elements)
                        _searchElements.Add(new ElementContainer { Element = e, Parent = e.Parent });

                if (!_container.ChildViewControllers.Contains(_searchController))
                {
                    _searchController.View.Frame = new RectangleF(_container.TableView.Bounds.X, 44f, _container.TableView.Bounds.Width, _container.TableView.Bounds.Height - 44f);
                    _container.AddChildViewController(_searchController);
                    _container.View.AddSubview(_searchController.View);
                }


            }

            public override void OnEditingStopped (UISearchBar searchBar)
            {

            }

            public override void TextChanged (UISearchBar searchBar, string searchText)
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    if (_searchController.Root != null)
                        _searchController.Root.Clear();
                    _searchController.View.BackgroundColor = NoItemColor;
                    _searchController.TableView.TableFooterView.Hidden = true;
                    _searchController.TableView.ScrollEnabled = false;
                    return;
                }

                var sec = new Section();
                foreach (var el in _searchElements)
                {
                    if (el.Element.Matches(searchText))
                    {
                        sec.Add(el.Element);
                    }
                }
                _searchController.TableView.ScrollEnabled = true;

                if (sec.Count == 0)
                {
                    sec.Add(new NoItemsElement());
                }

                _searchController.View.BackgroundColor = UIColor.White;
                _searchController.TableView.TableFooterView.Hidden = sec.Count == 0;
                var root = new RootElement("") { sec };
                root.UnevenRows = true;
                _searchController.Root = root;
            }

            public override void CancelButtonClicked (UISearchBar searchBar)
            {
                //Reset the parent
                foreach (var s in _searchElements)
                    s.Element.Parent = s.Parent;

                searchBar.Text = "";
                searchBar.ShowsCancelButton = false;
                _container.FinishSearch ();
                searchBar.ResignFirstResponder ();
                _container.NavigationController.SetNavigationBarHidden(false, true);
                _container.IsSearching = false;
                _container.TableView.ScrollEnabled = true;

                _searchController.RemoveFromParentViewController();
                _searchController.View.RemoveFromSuperview();

                if (_searchController.Root != null)
                    _searchController.Root.Clear();

                _searchElements.Clear();
                _searchElements = null;

                _container.SearchEnd();
            }

            public override void SearchButtonClicked (UISearchBar searchBar)
            {
                //container.SearchButtonClicked (searchBar.Text);
                searchBar.ResignFirstResponder();


                //Enable the cancel button again....
                foreach (var s in searchBar.Subviews)
                {
                    var x = s as UIButton;
                    if (x != null)
                        x.Enabled = true;
                }
            }
        }

        protected T Bind<T>(T element, string bindingDescription)
        {
            return element.Bind(this, bindingDescription);
        }

        protected T Bind<T>(T element, IEnumerable<MvxBindingDescription> bindingDescription)
        {
            return element.Bind(this, bindingDescription);
        }

        public object DataContext
        {
            get { return BindingContext.DataContext; }
            set { BindingContext.DataContext = value; }
        }

        public IMvxViewModel ViewModel
        {
            get { return DataContext as IMvxViewModel;  }
            set { DataContext = value; }
        }

        public IMvxBindingContext BindingContext { get; set; }

        public MvxViewModelRequest Request { get; set; }


        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            ViewDidDisappearCalled.Raise(this, animated);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCalled.Raise(this);
            }
            base.Dispose(disposing);
        }

        public event EventHandler DisposeCalled;
        public event EventHandler ViewDidLoadCalled;
        public event EventHandler<MvxValueEventArgs<bool>> ViewWillAppearCalled;
        public event EventHandler<MvxValueEventArgs<bool>> ViewDidAppearCalled;
        public event EventHandler<MvxValueEventArgs<bool>> ViewDidDisappearCalled;
        public event EventHandler<MvxValueEventArgs<bool>> ViewWillDisappearCalled;
    }
}

