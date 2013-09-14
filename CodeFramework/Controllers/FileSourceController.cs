using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeFramework.Views;

namespace CodeFramework.Controllers
{
    public abstract class FileSourceController : WebViewController
    {
        protected static string TempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "source");
        private bool _loaded;

        protected FileSourceController()
            : base(false)
        {
            Web.DataDetectorTypes = UIDataDetectorType.None;
            Title = "Source".t();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Create the temp directory if it does not exist!
            if (System.IO.Directory.Exists(TempDir))
                System.IO.Directory.Delete(TempDir, true);
            System.IO.Directory.CreateDirectory(TempDir);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //Do the request
            if (_loaded == false)
            {
                this.DoWork(Request, ex => ErrorView.Show(this.View, ex.Message));
                _loaded = true;
            }
        }

        protected override void OnLoadError(object sender, UIWebErrorArgs e)
        {
            base.OnLoadError(sender, e);

            //Can't load this!
            ErrorView.Show(this.View, "Unable to display this type of file.".t());
        }

        protected abstract void Request();

        protected void LoadRawData(string data, string language)
        {
            InvokeOnMainThread(delegate {
                var html = System.IO.File.ReadAllText("SourceBrowser/index.html");
                var filled = html.Replace("{CODE}", language).Replace("{DATA}", data);

                var url = NSBundle.MainBundle.BundlePath + "/SourceBrowser";
                url = url.Replace("/", "//").Replace(" ", "%20");

                Web.LoadHtmlString(filled, NSUrl.FromString("file:/" + url + "//"));
            });
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("app://ready"))
            {
                DOMReady();
                return false;
            }

            return true;
        }

        protected virtual void DOMReady()
        {
        }

        protected void LoadDiffData()
        {
            var path = System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "Diff/diffindex.html");
            LoadFile(path);
        }

        public static string JavaScriptStringEncode (string value)
        {
            return System.Web.HttpUtility.JavaScriptStringEncode(value);
        }

        public static string Decode(string value)
        {
            return System.Web.HttpUtility.UrlDecode(value);
        }
    }
}

