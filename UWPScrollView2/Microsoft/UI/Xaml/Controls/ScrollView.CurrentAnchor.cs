using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public UIElement? CurrentAnchor
    {
        get
        {
            if (_scrollPresenter is { } scrollPresenter)
            {
                if (scrollPresenter is IScrollAnchorProvider scrollPresenterAsAnchorProvider)
                {
                    return scrollPresenterAsAnchorProvider.CurrentAnchor;
                }
            }

            return null;
        }
    }
}