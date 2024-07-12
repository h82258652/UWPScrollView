using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public ScrollPresenter? ScrollPresenter
    {
        get
        {
            return _scrollPresenter;
        }
    }
}