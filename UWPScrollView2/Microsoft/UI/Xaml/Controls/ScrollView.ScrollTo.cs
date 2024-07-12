namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public int ScrollTo(double horizontalOffset, double verticalOffset)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ScrollTo(horizontalOffset, verticalOffset);
        }

        return -1;
    }

    public int ScrollTo(double horizontalOffset, double verticalOffset, ScrollingScrollOptions? options)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ScrollTo(horizontalOffset, verticalOffset, options);
        }

        return -1;
    }
}