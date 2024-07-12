namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Specifies a contract for scrollbar-like widgets that can set the scroll offsets of the content in a scrolling control.
/// </summary>
public interface IScrollController
{
    /// <summary>
    /// Gets a value that indicates whether the user can scroll or pan with the scroll controller.
    /// </summary>
    bool CanScroll { get; }

    /// <summary>
    /// Gets a value that indicates whether or not the scroll controller is handling a mouse-driven scroll.
    /// </summary>
    bool IsScrollingWithMouse { get; }

    /// <summary>
    /// Gets an instance of an <see cref="IScrollControllerPanningInfo"/> implementation that contains information related to content panning, or <see langword="null"/>.
    /// </summary>    
    IScrollControllerPanningInfo PanningInfo { get; }
}