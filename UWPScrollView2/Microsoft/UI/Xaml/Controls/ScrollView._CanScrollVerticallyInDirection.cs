using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool CanScrollVerticallyInDirection(bool inPositiveDirection)
    {
        bool canScrollInDirection = false;
        if (_scrollPresenter is not null)
        {
            var scrollPresenter = _scrollPresenter as ScrollPresenter;
            var verticalScrollMode = ComputedVerticalScrollMode;

            if (verticalScrollMode == ScrollingScrollMode.Enabled)
            {
                var zoomedExtentHeight = scrollPresenter.ExtentHeight * scrollPresenter.ZoomFactor;
                var viewportHeight = scrollPresenter.ActualHeight;
                if (zoomedExtentHeight > viewportHeight)
                {
                    // Ignore distance to an edge smaller than 1/1000th of a pixel to account for rounding approximations.
                    // Otherwise an Up/Down arrow key may be processed and have no effect.
                    const double offsetEpsilon = 0.001;

                    if (inPositiveDirection)
                    {
                        var maxVerticalOffset = zoomedExtentHeight - viewportHeight;
                        if (scrollPresenter.VerticalOffset < maxVerticalOffset - offsetEpsilon)
                        {
                            canScrollInDirection = true;
                        }
                    }
                    else
                    {
                        if (scrollPresenter.VerticalOffset > offsetEpsilon)
                        {
                            canScrollInDirection = true;
                        }
                    }
                }
            }
        }

        return canScrollInDirection;
    }
}