namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double ScrollableWidth
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ScrollableWidth;
            }

            return 0;
        }
    }
}