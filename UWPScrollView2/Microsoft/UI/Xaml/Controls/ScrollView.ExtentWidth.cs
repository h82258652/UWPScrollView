namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double ExtentWidth
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ExtentWidth;
            }

            return 0;
        }
    }
}