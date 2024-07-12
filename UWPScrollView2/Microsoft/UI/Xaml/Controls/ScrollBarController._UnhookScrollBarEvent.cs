namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void UnhookScrollBarEvent()
    {
        if (_scrollBar is not null)
        {
            _scrollBar.Scroll -= OnScroll;
        }
    }
}