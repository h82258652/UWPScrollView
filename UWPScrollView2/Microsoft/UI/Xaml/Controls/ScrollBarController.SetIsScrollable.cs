namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    public void SetIsScrollable(bool isScrollable)
    {
        _isScrollable = isScrollable;

        UpdateCanScroll();
    }
}