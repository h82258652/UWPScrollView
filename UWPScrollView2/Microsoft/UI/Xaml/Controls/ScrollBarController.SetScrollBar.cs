using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    public void SetScrollBar(ScrollBar scrollBar)
    {
        UnhookScrollBarEvent();

        _scrollBar = scrollBar;

        HookScrollBarEvent();
        HookScrollBarPropertyChanged();
    }
}