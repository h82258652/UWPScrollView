using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void UpdateHorizontalScrollController(IScrollController horizontalScrollController, UIElement horizontalScrollControllerElement)
    {
        UnhookHorizontalScrollControllerEvents(false /*isForDestructor*/);

        m_horizontalScrollController = horizontalScrollController;
        m_horizontalScrollControllerElement = horizontalScrollControllerElement;
        HookHorizontalScrollControllerEvents();
        UpdateScrollPresenterHorizontalScrollController(horizontalScrollController);
    }
}