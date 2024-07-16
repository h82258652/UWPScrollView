using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.ScrollCompleted"/> event.
/// </summary>
public sealed class ScrollingScrollCompletedEventArgs
{
    private int m_offsetsChangeCorrelationId = -1;
    private ScrollPresenterViewChangeResult m_result = ScrollPresenterViewChangeResult.Completed;

    internal ScrollingScrollCompletedEventArgs()
    {
    }

    /// <summary>
    /// Gets the correlation ID associated with the offsets change, previously returned by <see cref="ScrollView.ScrollTo"/>, <see cref="ScrollView.ScrollBy"/>, or <see cref="ScrollView.AddScrollVelocity"/>.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            return m_offsetsChangeCorrelationId;
        }
    }

    internal void OffsetsChangeCorrelationId(int offsetsChangeCorrelationId)
    {
        m_offsetsChangeCorrelationId = offsetsChangeCorrelationId;
    }

    internal ScrollPresenterViewChangeResult Result()
    {
        return m_result;
    }

    internal void Result(ScrollPresenterViewChangeResult result)
    {
        m_result = result;
    }
}