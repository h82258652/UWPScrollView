namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta)
    {
        return ScrollBy(horizontalOffsetDelta, verticalOffsetDelta, null);
    }

    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta, ScrollingScrollOptions? options)
    {
        int viewChangeCorrelationId;
        ChangeOffsetsPrivate(
            horizontalOffsetDelta /*zoomedHorizontalOffset*/,
            verticalOffsetDelta /*zoomedVerticalOffset*/,
            ScrollPresenterViewKind.RelativeToCurrentView,
            options,
            null /*bringIntoViewRequestedEventArgs*/,
            InteractionTrackerAsyncOperationTrigger.DirectViewChange,
            -1 /*existingViewChangeCorrelationId*/,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }
}