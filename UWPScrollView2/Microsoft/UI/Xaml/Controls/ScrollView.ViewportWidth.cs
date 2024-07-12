namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double ViewportWidth
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ViewportWidth;
            }

            return 0;
        }
    }
}