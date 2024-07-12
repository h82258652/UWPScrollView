using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void UpdateVerticalScrollController(IScrollController verticalScrollController, UIElement verticalScrollControllerElement)
    {
        UnhookVerticalScrollControllerEvents(false /*isForDestructor*/);

        m_verticalScrollController = verticalScrollController;
        m_verticalScrollControllerElement = verticalScrollControllerElement;
        HookVerticalScrollControllerEvents();
        UpdateScrollPresenterVerticalScrollController(verticalScrollController);
    }
}