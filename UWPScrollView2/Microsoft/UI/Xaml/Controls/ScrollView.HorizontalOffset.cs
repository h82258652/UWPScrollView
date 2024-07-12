namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double HorizontalOffset
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.HorizontalOffset;
            }

            return 0;
        }
    }
}