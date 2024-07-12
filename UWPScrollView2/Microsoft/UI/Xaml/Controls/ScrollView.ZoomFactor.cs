namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public float ZoomFactor
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ZoomFactor;
            }

            return 0;
        }
    }
}