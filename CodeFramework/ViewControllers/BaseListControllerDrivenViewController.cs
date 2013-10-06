using System;
using System.Linq;
using CodeFramework.Views;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch;
using CodeFramework.Filters.Controllers;

namespace CodeFramework.Controllers
{
    public abstract class BaseListControllerDrivenViewController : BaseControllerDrivenViewController
    {
        private bool _enableFilter;

        public string NoItemsText { get; set; }

        public bool EnableFilter
        {
            get { return _enableFilter; }
            set 
            {
                if (value)
                    EnableSearch = true;
                _enableFilter = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name='push'>True if navigation controller should push, false if otherwise</param>
        /// <param name='refresh'>True if the data can be refreshed, false if otherwise</param>
		protected BaseListControllerDrivenViewController(bool push = true, bool refresh = true)
            : base(push, refresh)
        {
            NoItemsText = "No Items".t();
            Style = UITableViewStyle.Plain;
            EnableSearch = true;
        }
  
        protected void RenderList<T>(ListModel<T> items, Func<T, Element> select)
        {
            var root = new RootElement(Title) { UnevenRows = Root.UnevenRows };

            if (items.FilteredData != null)
            {
                foreach (var grp in items.FilteredData)
                {
                    var sec = new Section(new TableViewSectionView(grp.Key));
                    foreach (var element in grp.Select(select).Where(element => element != null))
                        sec.Add(element);

                    if (sec.Elements.Count > 0)
                        root.Add(sec);
                }
            }
            else
            {
                var sec = new Section();
                foreach (var item in items.Data)
                {
                    var element = select(item);
                    if (element != null)
                        sec.Add(element);
                }

                //Make sure there's something to add
                if (sec.Elements.Count > 0)
                    root.Add(sec);
            }

            var elements = 0;
            foreach (var s in root)
                elements += s.Elements.Count;

            //There are no items! We must have filtered them out
            if (elements == 0)
                root.Add(new Section { new NoItemsElement(NoItemsText) });

            if (items.More != null)
            {
                var loadMore = new PaginateElement("Load More".t(), "Loading...".t(), e => this.DoWorkNoHud(() => items.More(),
                                                                                                            x => Utilities.ShowAlert("Unable to load more!".t(), x.Message))) { AutoLoadOnVisible = true };
                root.Add(new Section { loadMore });
            }

            Root = root;

            if (TableView.TableFooterView != null)
                TableView.TableFooterView.Hidden = false;
        }


        protected override void SearchStart()
        {
            base.StartSearch();

            var searchBar = SearchBar as SearchFilterBar;
            if (searchBar != null)
                searchBar.FilterButtonVisible = false;
        }

        protected override void SearchEnd()
        {
            var searchBar = SearchBar as SearchFilterBar;
            if (searchBar != null)
                searchBar.FilterButtonVisible = true;
        }

        protected override UISearchBar CreateSearchBar()
        {
            if (EnableFilter)
            {
                var searchBar = new SearchFilterBar { Delegate = new CustomSearchDelegate(this) };
                searchBar.FilterButton.TouchUpInside += FilterButtonTouched;
                return searchBar;
            }
            return base.CreateSearchBar();
        }

        protected virtual FilterViewController CreateFilterController()
        {
            return null;
        }

        void FilterButtonTouched (object sender, EventArgs e)
        {
            var filter = CreateFilterController();
            if (filter != null)
                ShowFilterController(filter);
        }

        private void ShowFilterController(FilterViewController filter)
        {
            var nav = new UINavigationController(filter);
            PresentViewController(nav, true, null);
        }
    }
}
