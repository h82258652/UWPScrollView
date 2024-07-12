namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void UpdateCanScroll()
    {
        bool oldCanScroll = _canScroll;
        _canScroll =
            _scrollBar is not null &&
            _scrollBar.Parent is not null &&
            _scrollBar.IsEnabled &&
            _scrollBar.Maximum > _scrollBar.Minimum &&
            _isScrollable;

        if (oldCanScroll != _canScroll)
        {
            RaiseCanScrollChanged();
        }
    }
}