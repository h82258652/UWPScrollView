using System;
using System.Numerics;
using Windows.UI.Composition;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.ZoomAnimationStarting"/> event.
/// </summary>
public sealed class ScrollingZoomAnimationStartingEventArgs
{
    private float m_endZoomFactor;

    private ScrollingZoomAnimationStartingEventArgs()
    { }

    /// <summary>
    /// Gets or sets the animation to run during the animated zoom factor change. The animation targets the content's scale.
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
    /// Gets the center point for the zoom factor change.
    /// </summary>
    public Vector2 CenterPoint
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the correlation ID associated with the animated zoom factor change, previously returned by <see cref="ScrollView.ZoomTo"/> or <see cref="ScrollView.ZoomBy"/>.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the content scale at the end of the animation.
    /// </summary>
    public float EndZoomFactor
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the content scale at the start of the animation.
    /// </summary>
    public float StartZoomFactor
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}