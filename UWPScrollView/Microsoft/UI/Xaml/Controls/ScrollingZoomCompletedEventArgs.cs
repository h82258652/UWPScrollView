using System;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.ZoomCompleted"/> event.
/// </summary>
public sealed class ScrollingZoomCompletedEventArgs
{
    /// <summary>
    /// Gets the correlation ID associated with the zoom factor change, previously returned by <see cref="ZoomTo"/>, <see cref="ZoomBy"/>, or <see cref="AddZoomVelocity"/>.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}