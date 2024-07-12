using System;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    private void ChangeOffsetsPrivate(
        double zoomedHorizontalOffset,
        double zoomedVerticalOffset,
        ScrollPresenterViewKind offsetsKind,
        ScrollingScrollOptions? options,
        BringIntoViewRequestedEventArgs? bringIntoViewRequestedEventArgs,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        int existingViewChangeCorrelationId,
        out int viewChangeCorrelationId)
    {
        throw new NotImplementedException();
    }
}