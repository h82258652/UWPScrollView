﻿using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Provides data for the <see cref="IScrollController.ScrollByRequested"/> event.
/// </summary>
public sealed class ScrollControllerScrollByRequestedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollControllerScrollByRequestedEventArgs"/> class with the specified offset delta and options.
    /// </summary>
    /// <param name="offsetDelta">The amount of change to the scroll offset.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    public ScrollControllerScrollByRequestedEventArgs(double offsetDelta, ScrollingScrollOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the correlation ID associated with the offset change.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the amount of change to the scroll offset.
    /// </summary>
    public double OffsetDelta
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets options that specify whether or not animations are enabled and snap points are respected.
    /// </summary>
    public ScrollingScrollOptions Options
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}