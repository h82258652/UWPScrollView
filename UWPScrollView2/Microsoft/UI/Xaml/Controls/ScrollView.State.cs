namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public ScrollingInteractionState State
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.State;
            }

            return ScrollingInteractionState.Idle;
        }
    }
}