namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    private bool IsLoadedAndSetUp()
    {
        return IsLoaded && m_interactionTracker is not null;
    }
}