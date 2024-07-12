using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void OnScrollBarPropertyChanged(DependencyObject sender, DependencyProperty args)
    {
        if (args == Control.IsEnabledProperty)
        {
            UpdateCanScroll();
        }
    }
}