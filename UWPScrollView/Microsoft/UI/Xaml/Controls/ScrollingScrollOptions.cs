using System;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Represents options that can be set in scroll methods of the <see cref="ScrollView"/> control.
/// </summary>
public class ScrollingScrollOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollingScrollOptions"/> class with the specified animation mode.
    /// </summary>
    /// <param name="animationMode">A value that indicates whether or not an animation is played for the scroll operation.</param>
    public ScrollingScrollOptions(ScrollingAnimationMode animationMode)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollingScrollOptions"/> class with the specified animation and snap points modes.
    /// </summary>
    /// <param name="animationMode">A value that indicates whether or not an animation is played for the scroll operation.</param>
    /// <param name="snapPointsMode">A value that indicates whether snap points are ignored or respected during the scroll operation.</param>
    public ScrollingScrollOptions(ScrollingAnimationMode animationMode, ScrollingSnapPointsMode snapPointsMode)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not an animation is played for a scroll operation.
    /// </summary>
    public ScrollingAnimationMode AnimationMode
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
    /// Gets or sets a value that indicates whether snap points are ignored or respected during the scroll operation.
    /// </summary>
    public ScrollingSnapPointsMode SnapPointsMode
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
}