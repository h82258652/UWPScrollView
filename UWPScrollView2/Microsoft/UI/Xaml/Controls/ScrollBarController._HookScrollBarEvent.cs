namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void HookScrollBarEvent()
    {
        if (_scrollBar is not null)
        {
            _scrollBar.Scroll += OnScroll;
        }
    }
}