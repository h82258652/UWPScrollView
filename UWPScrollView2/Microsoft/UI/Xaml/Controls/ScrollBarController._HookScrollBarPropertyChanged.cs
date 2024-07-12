using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void HookScrollBarPropertyChanged()
    {
        if (_scrollBar is not null)
        {
            _scrollBarIsEnabledChangedToken = _scrollBar.RegisterPropertyChangedCallback(Control.IsEnabledProperty, OnScrollBarPropertyChanged);
        }
    }
}