using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Microsoft.UI;

internal static class SharedHelpers
{
    internal static bool DoRectsIntersect(Rect rect1 , Rect rect2)
    {

    var doIntersect =
            !(rect1.Width <= 0 || rect1.Height <= 0 || rect2.Width <= 0 || rect2.Height <= 0) &&
            (rect2.X <= rect1.X + rect1.Width) &&
            (rect2.X + rect2.Width >= rect1.X) &&
            (rect2.Y <= rect1.Y + rect1.Height) &&
            (rect2.Y + rect2.Height >= rect1.Y);
        return doIntersect;
    }

    internal static bool IsAncestor(DependencyObject? child, DependencyObject? parent, bool checkVisibility = false)
    {
        if (child is null || parent is null || child == parent)
        {
            return false;
        }

        if (checkVisibility)
        {
            UIElement parentAsUIE = parent as UIElement;

            if (parentAsUIE is not null && parentAsUIE.Visibility == Visibility.Collapsed)
            {
                return false;
            }

            UIElement childAsUIE = child as UIElement;

            if (childAsUIE is not null && childAsUIE.Visibility == Visibility.Collapsed)
            {
                return false;
            }
        }

        DependencyObject parentTemp = VisualTreeHelper.GetParent(child);
        while (parentTemp is not null)
        {
            if (checkVisibility)
            {
                UIElement parentTempAsUIE = parentTemp as UIElement;

                if (parentTempAsUIE is not null && parentTempAsUIE.Visibility == Visibility.Collapsed)
                {
                    return false;
                }
            }

            if (parentTemp == parent)
            {
                return true;
            }

            parentTemp = VisualTreeHelper.GetParent(parentTemp);
        }

        return false;
    }

    internal static bool IsAnimationsEnabled()
    {
        UISettings uiSettings = new UISettings();
        return uiSettings.AnimationsEnabled;
    }
}