using MonoTouch.UIKit;

namespace CodeFramework.Views
{
    public class WatermarkView : UIView
    {
        public static UIImage Image;

        public static void AssureWatermark(UIViewController controller)
        {
            if (Image == null)
                return;

            if (controller.ParentViewController != null)
            {
                controller.ParentViewController.View.BackgroundColor = UIColor.FromPatternImage(Image);
            }
        }
    }
}

