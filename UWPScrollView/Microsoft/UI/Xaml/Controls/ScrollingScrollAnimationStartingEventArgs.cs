using System;
using System.Numerics;
using Windows.UI.Composition;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.ScrollAnimationStarting"/> event.
/// </summary>
public sealed class ScrollingScrollAnimationStartingEventArgs
{
    /// <summary>
    /// Gets or sets the animation to run during the animated scroll offset change. The animation targets the content's position.
    /// </summary>
    public CompositionAnimation Animation
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
    /// Gets the correlation ID associated with the animated scroll offset change, previously returned by <see cref="ScrollView.ScrollTo"/> or <see cref="ScrollView.ScrollBy"/>.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the position of the content at the end of the animation.
    /// </summary>
    public Vector2 EndPosition
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the position of the content at the start of the animation.
    /// </summary>
    public Vector2 StartPosition
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}