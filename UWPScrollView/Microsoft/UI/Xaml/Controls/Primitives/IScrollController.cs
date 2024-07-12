using System.Numerics;
using Windows.UI.Composition;

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
    IScrollControllerPanningInfo? PanningInfo { get; }

    /// <summary>
    /// Provides the <see cref="IScrollController"/> with the option of customizing the animation used to perform its scroll request.
    /// </summary>
    /// <param name="correlationId">A correlation ID number used to associate a method call with corresponding events.</param>
    /// <param name="startPosition">The start position of the content.</param>
    /// <param name="endPosition">The end position of the content.</param>
    /// <param name="defaultAnimation">The animation that is applied to the scroll request.</param>
    /// <returns>The animation to use, if any, or <see langword="null"/>.</returns>
    CompositionAnimation? GetScrollAnimation(int correlationId, Vector2 startPosition, Vector2 endPosition, CompositionAnimation defaultAnimation);

    /// <summary>
    /// Provides dimension information about the scrolling control to the scroll controller.
    /// </summary>
    /// <param name="minOffset">Specifies the minimum offset allowed for the relevant scrolling orientation.</param>
    /// <param name="maxOffset">Specifies the maximum offset allowed for the relevant scrolling orientation.</param>
    /// <param name="offset">Specifies the current offset value.</param>
    /// <param name="viewportLength">Specifies the length of the scrolling control's viewport.</param>
    void SetValues(double minOffset, double maxOffset, double offset, double viewportLength);
}