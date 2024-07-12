using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void UpdateScrollPresenterHorizontalScrollController(IScrollController horizontalScrollController)
    {
        if (_scrollPresenter is { } scrollPresenter)
        {
            scrollPresenter.HorizontalScrollController = horizontalScrollController;
        }
    }
}