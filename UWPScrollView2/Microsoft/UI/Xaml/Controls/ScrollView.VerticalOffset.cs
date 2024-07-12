namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double VerticalOffset
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.VerticalOffset;
            }

            return 0;
        }
    }
}