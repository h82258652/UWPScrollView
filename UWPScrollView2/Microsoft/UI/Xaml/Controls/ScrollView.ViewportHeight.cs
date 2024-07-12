namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double ViewportHeight
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ViewportHeight;
            }

            return 0;
        }
    }
}