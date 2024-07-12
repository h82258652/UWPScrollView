using System;
using Windows.UI.Composition;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.ZoomAnimationStarting"/> event.
/// </summary>
public sealed class ScrollingZoomAnimationStartingEventArgs
{
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
    /// Gets the content scale at the end of the animation.
    /// </summary>
    public float EndZoomFactor
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}