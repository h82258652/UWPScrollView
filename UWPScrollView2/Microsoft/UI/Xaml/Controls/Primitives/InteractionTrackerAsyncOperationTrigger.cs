using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
    internal enum InteractionTrackerAsyncOperationTrigger
    {
        // Operation is triggered by a direct call to ScrollPresenter's ScrollTo/ScrollBy/AddScrollVelocity or ZoomTo/ZoomBy/AddZoomVelocity
        DirectViewChange = 0x01,
        // Operation is triggered by the horizontal IScrollController.
        HorizontalScrollControllerRequest = 0x02,
        // Operation is triggered by the vertical IScrollController.
        VerticalScrollControllerRequest = 0x04,
        // Operation is triggered by the UIElement.BringIntoViewRequested event handler.
        BringIntoViewRequest = 0x08
    }
}
