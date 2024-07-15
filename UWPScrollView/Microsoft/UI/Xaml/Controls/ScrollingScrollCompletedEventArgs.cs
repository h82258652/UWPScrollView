using System;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.ScrollCompleted"/> event.
/// </summary>
public sealed class ScrollingScrollCompletedEventArgs
{
    private int m_offsetsChangeCorrelationId = -1;

    private ScrollingScrollCompletedEventArgs()
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
}