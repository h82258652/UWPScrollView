namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Provides data for the <see cref="IScrollController.AddScrollVelocityRequested"/> event.
/// </summary>
public sealed class ScrollControllerAddScrollVelocityRequestedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollControllerAddScrollVelocityRequestedEventArgs"/> class with the specified offset velocity and inertia decay rate.
    /// </summary>
    /// <param name="offsetVelocity">The requested velocity of the offset change.</param>
    /// <param name="inertiaDecayRate">The inertia decay rate.</param>
    public ScrollControllerAddScrollVelocityRequestedEventArgs(float offsetVelocity, float? inertiaDecayRate)
    {
        OffsetVelocity = offsetVelocity;
        InertiaDecayRate = inertiaDecayRate;
    }

    /// <summary>
    /// Gets or sets the correlation ID associated with the offset change.
    /// </summary>
    public int CorrelationId { get; set; } = -1;

    /// <summary>
    /// Gets the inertia decay rate, between 0.0 and 1.0, for the requested scroll operation.
    /// </summary>
    public float? InertiaDecayRate { get; }

    /// <summary>
    /// Gets the requested velocity of the offset change.
    /// </summary>
    public float OffsetVelocity { get; }
}