using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void UpdateScrollControllersSeparator(UIElement scrollControllersSeparator)
    {
        m_scrollControllersSeparatorElement = scrollControllersSeparator;
    }
}