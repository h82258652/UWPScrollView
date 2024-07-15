using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Microsoft.UI;

internal static class SharedHelpers
{
    internal static bool IsAncestor(DependencyObject child, ScrollView parent, bool checkVisibility)
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