using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool CanScrollHorizontallyInDirection(bool inPositiveDirection)
    {
        bool canScrollInDirection = false;

        if (FlowDirection == FlowDirection.RightToLeft)
        {
            inPositiveDirection = !inPositiveDirection;
        }

        if (_scrollPresenter is not null)
        {
            var scrollPresenter = _scrollPresenter as ScrollPresenter;
            var horizontalScrollMode = ComputedHorizontalScrollMode;

            if (horizontalScrollMode == ScrollingScrollMode.Enabled)
            {
                var zoomedExtentWidth = scrollPresenter.ExtentWidth * scrollPresenter.ZoomFactor;
                var viewportWidth = scrollPresenter.ActualWidth;
                if (zoomedExtentWidth > viewportWidth)
                {
                    // Ignore distance to an edge smaller than 1/1000th of a pixel to account for rounding approximations.
                    // Otherwise a Left/Right arrow key may be processed and have no effect.
                    const double offsetEpsilon = 0.001;

                    if (inPositiveDirection)
                    {
                        var maxHorizontalOffset = zoomedExtentWidth - viewportWidth;
                        if (scrollPresenter.HorizontalOffset < maxHorizontalOffset - offsetEpsilon)
                        {
                            canScrollInDirection = true;
                        }
                    }
                    else
                    {
                        if (scrollPresenter.HorizontalOffset > offsetEpsilon)
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