namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double ScrollableHeight
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ScrollableHeight;
            }

            return 0;
        }
    }
}