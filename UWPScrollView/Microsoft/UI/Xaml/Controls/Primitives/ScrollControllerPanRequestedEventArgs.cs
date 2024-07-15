using Windows.UI.Input;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Provides data for the <see cref="IScrollControllerPanningInfo.PanRequested"/> event.
/// </summary>
public sealed class ScrollControllerPanRequestedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollControllerPanRequestedEventArgs"/> class with the specified <paramref name="pointerPoint"/>.
    /// </summary>
    /// <param name="pointerPoint">The <see cref="PointerPoint"/> instance associated with the user gesture that initiated the <see cref="IScrollControllerPanningInfo.PanRequested"/> event.</param>
    public ScrollControllerPanRequestedEventArgs(PointerPoint pointerPoint)
    {
        PointerPoint = pointerPoint;
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not the pan manipulation was successfully initiated.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Gets he <see cref="PointerPoint"/> instance associated with the user gesture that initiated the <see cref="IScrollControllerPanningInfo.PanRequested"/> event.
    /// </summary>
    public PointerPoint PointerPoint { get; }
}