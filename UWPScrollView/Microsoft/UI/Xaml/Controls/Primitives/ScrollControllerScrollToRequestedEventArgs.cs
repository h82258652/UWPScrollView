﻿namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Provides data for the <see cref="IScrollController.ScrollToRequested"/> event.
/// </summary>
public sealed class ScrollControllerScrollToRequestedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollControllerScrollToRequestedEventArgs"/> class with the specified offset and options.
    /// </summary>
    /// <param name="offset">The target scroll offset.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    public ScrollControllerScrollToRequestedEventArgs(double offset, ScrollingScrollOptions options)
    {
        Offset = offset;
        Options = options;
    }

    /// <summary>
    /// Gets the correlation ID associated with the offset change.
    /// </summary>
    public int CorrelationId { get; set; } = -1;

    /// <summary>
    /// Gets the target scroll offset.
    /// </summary>
    public double Offset { get; }

    /// <summary>
    /// Gets options that specify whether or not animations are enabled and snap points are respected.
    /// </summary>
    public ScrollingScrollOptions Options { get; }
}