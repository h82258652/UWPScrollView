using System.Numerics;
using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void DoScroll(double offset, Orientation orientation)
    {
        bool isVertical = orientation == Orientation.Vertical;

        if (_scrollPresenter is { } scrollPresenter)
        {
            if (SharedHelpers.IsAnimationsEnabled())
            {
                Vector2 inertiaDecayRate = new Vector2(0.9995f, 0.9995f);

                // A velocity less than or equal to this value has no effect.
                const double minVelocity = 30.0;

                // We need to add this much velocity over minVelocity per pixel we want to move:
                const double s_velocityNeededPerPixel = 7.600855902349023;

                var scrollDir = offset > 0 ? 1 : -1;

                // The minimum velocity required to move in the given direction.
                double baselineVelocity = minVelocity * scrollDir;

                // If there is already a scroll animation running for a previous key press, we want to take that into account
                // for calculating the baseline velocity.
                var previousScrollViewChangeCorrelationId = isVertical ? m_verticalAddScrollVelocityOffsetChangeCorrelationId : m_horizontalAddScrollVelocityOffsetChangeCorrelationId;
                if (previousScrollViewChangeCorrelationId != -1)
                {
                    var directionOfPreviousScrollOperation = isVertical ? m_verticalAddScrollVelocityDirection : m_horizontalAddScrollVelocityDirection;
                    if (directionOfPreviousScrollOperation == 1)
                    {
                        baselineVelocity -= minVelocity;
                    }
                    else if (directionOfPreviousScrollOperation == -1)
                    {
                        baselineVelocity += minVelocity;
                    }
                }

                var velocity = (float)(baselineVelocity + (offset * s_velocityNeededPerPixel));

                if (isVertical)
                {
                    Vector2 offsetsVelocity = new Vector2(0.0f, velocity);
                    m_verticalAddScrollVelocityOffsetChangeCorrelationId = scrollPresenter.AddScrollVelocity(offsetsVelocity, inertiaDecayRate);
                    m_verticalAddScrollVelocityDirection = scrollDir;
                }
                else
                {
                    Vector2 offsetsVelocity = new Vector2(velocity, 0.0f);
                    m_horizontalAddScrollVelocityOffsetChangeCorrelationId = scrollPresenter.AddScrollVelocity(offsetsVelocity, inertiaDecayRate);
                    m_horizontalAddScrollVelocityDirection = scrollDir;
                }
            }
            else
            {
                if (isVertical)
                {
                    // Any horizontal AddScrollVelocity animation recently launched should be ignored by a potential subsequent AddScrollVelocity call.
                    m_verticalAddScrollVelocityOffsetChangeCorrelationId = -1;

                    scrollPresenter.ScrollBy(0.0 /*horizontalOffsetDelta*/, offset /*verticalOffsetDelta*/);
                }
                else
                {
                    // Any vertical AddScrollVelocity animation recently launched should be ignored by a potential subsequent AddScrollVelocity call.
                    m_horizontalAddScrollVelocityOffsetChangeCorrelationId = -1;

                    scrollPresenter.ScrollBy(offset /*horizontalOffsetDelta*/, 0.0 /*verticalOffsetDelta*/);
                }
            }
        }
    }
}