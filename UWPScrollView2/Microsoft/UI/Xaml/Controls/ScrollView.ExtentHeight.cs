namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public double ExtentHeight
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ExtentHeight;
            }

            return 0;
        }
    }
}