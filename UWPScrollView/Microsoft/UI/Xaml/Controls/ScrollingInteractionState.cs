namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Defines constants that specify the interaction state of a <see cref="ScrollView"/>.
/// </summary>
public enum ScrollingInteractionState
{
    /// <summary>
    /// No interaction is occurring.
    /// </summary>
    Idle,

    /// <summary>
    /// User interaction is occurring.
    /// </summary>
    Interaction,

    /// <summary>
    /// Inertial movement is occurring.
    /// </summary>
    Inertia,

    /// <summary>
    /// An animation is occurring.
    /// </summary>
    Animation
}