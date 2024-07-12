namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public int ScrollTo(double horizontalOffset, double verticalOffset)
    {
        return ScrollTo(horizontalOffset, verticalOffset, null);
    }

    public int ScrollTo(double horizontalOffset, double verticalOffset, ScrollingScrollOptions? options)
    {
        int viewChangeCorrelationId;
        ChangeOffsetsPrivate(
            horizontalOffset /*zoomedHorizontalOffset*/,
            verticalOffset /*zoomedVerticalOffset*/,
            ScrollPresenterViewKind.Absolute,
            options,
            null /*bringIntoViewRequestedEventArgs*/,
            InteractionTrackerAsyncOperationTrigger.DirectViewChange,
            -1 /*existingViewChangeCorrelationId*/,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }
}