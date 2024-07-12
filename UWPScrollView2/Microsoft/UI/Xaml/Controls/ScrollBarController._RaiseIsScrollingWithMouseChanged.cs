namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void RaiseIsScrollingWithMouseChanged()
    {
        if (_isScrollingWithMouseChanged is null)
        {
            return;
        }

        _isScrollingWithMouseChanged(this, null);
    }
}