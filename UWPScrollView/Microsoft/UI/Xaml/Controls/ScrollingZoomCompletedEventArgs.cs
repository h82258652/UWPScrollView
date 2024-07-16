using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.ZoomCompleted"/> event.
/// </summary>
public sealed class ScrollingZoomCompletedEventArgs
{
    private ScrollPresenterViewChangeResult m_result = ScrollPresenterViewChangeResult.Completed;
    private int m_zoomFactorChangeCorrelationId = -1;

    internal ScrollingZoomCompletedEventArgs()
    {
    }

    /// <summary>
    /// Gets the correlation ID associated with the zoom factor change, previously returned by <see cref="ZoomTo"/>, <see cref="ZoomBy"/>, or <see cref="AddZoomVelocity"/>.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            return m_zoomFactorChangeCorrelationId;
        }
    }

    internal void Result(ScrollPresenterViewChangeResult result)
    {
        m_result = result;
    }

    internal ScrollPresenterViewChangeResult Result()
    {
        return m_result;
    }

    internal void ZoomFactorChangeCorrelationId(int zoomFactorChangeCorrelationId)
    {
        m_zoomFactorChangeCorrelationId = zoomFactorChangeCorrelationId;
    }
}