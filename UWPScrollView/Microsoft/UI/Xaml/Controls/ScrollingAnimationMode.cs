namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Defines constants that specify whether the <see cref="ScrollView"/> control plays animations for scrolling and zooming actions.
/// </summary>
public enum ScrollingAnimationMode
{
    /// <summary>
    /// Animations are not played.
    /// </summary>
    Disabled,

    /// <summary>
    /// Animations are played.
    /// </summary>
    Enabled,

    /// <summary>
    /// Animations are played when operating system (OS) settings enable animations.
    /// </summary>
    Auto
}