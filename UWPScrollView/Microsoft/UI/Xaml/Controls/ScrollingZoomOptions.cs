using System;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Represents options that can be set in zoom methods of the <see cref="ScrollView"/> control.
/// </summary>
public class ScrollingZoomOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollingZoomOptions"/> class with the specified animation mode.
    /// </summary>
    /// <param name="animationMode">A value that indicates whether or not an animation is played for the zoom factor change.</param>
    public ScrollingZoomOptions(ScrollingAnimationMode animationMode)
    {
        AnimationMode = animationMode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollingZoomOptions"/> class with the specified animation and snap points modes.
    /// </summary>
    /// <param name="animationMode">A value that indicates whether or not an animation is played for the zoom factor change.</param>
    /// <param name="snapPointsMode"></param>
    /// <exception cref="NotImplementedException">A value that indicates whether snap points are ignored or respected during the zoom factor change.</exception>
    public ScrollingZoomOptions(ScrollingAnimationMode animationMode, ScrollingSnapPointsMode snapPointsMode)
    {
        AnimationMode = animationMode;
        SnapPointsMode = snapPointsMode;
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not an animation is played for the zoom factor change.
    /// </summary>
    public ScrollingAnimationMode AnimationMode { get; set; } = ScrollingAnimationMode.Auto;

    /// <summary>
    /// Gets or sets a value that indicates whether snap points are ignored or respected during the zoom factor change.
    /// </summary>
    public ScrollingSnapPointsMode SnapPointsMode { get; set; } = ScrollingSnapPointsMode.Default;
}