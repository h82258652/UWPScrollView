using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Specifies a contract for scrollbar-like widgets that can set the scroll offsets of the content in a scrolling control.
/// </summary>
public interface IScrollController
{
    /// <summary>
    /// Occurs when a scroll velocity change is requested.
    /// </summary>
    event TypedEventHandler<IScrollController, ScrollControllerAddScrollVelocityRequestedEventArgs>? AddScrollVelocityRequested;

    /// <summary>
    /// Occurs when the <see cref="CanScroll"/> property value has changed.
    /// </summary>
    event TypedEventHandler<IScrollController, object?>? CanScrollChanged;

    /// <summary>
    /// Occurs when the <see cref="IsScrollingWithMouse"/> property value changes.
    /// </summary>
    event TypedEventHandler<IScrollController, object?>? IsScrollingWithMouseChanged;

    /// <summary>
    /// Occurs when a scroll by an particular offset delta is requested.
    /// </summary>
    event TypedEventHandler<IScrollController, ScrollControllerScrollByRequestedEventArgs>? ScrollByRequested;

    /// <summary>
    /// Occurs when a scroll to a particular offset is requested.
    /// </summary>
    event TypedEventHandler<IScrollController, ScrollControllerScrollToRequestedEventArgs>? ScrollToRequested;

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
    /// Indicates that a scrolling operation initiated through a <see cref="ScrollToRequested"/>, <see cref="ScrollByRequested"/>, or <see cref="AddScrollVelocityRequested"/> event has completed.
    /// </summary>
    /// <param name="correlationId">A correlation ID number used to associate a method call with corresponding events.</param>
    void NotifyRequestedScrollCompleted(int correlationId);

    /// <summary>
    /// Sets a value that indicates whether or not the <see cref="ScrollPresenter"/> content is scrollable by means of user input.
    /// </summary>
    /// <param name="isScrollable"><see langword="true"/> if the <see cref="ScrollPresenter"/> content is scrollable by means of user input; otherwise, <see langword="false"/>.</param>
    void SetIsScrollable(bool isScrollable);

    /// <summary>
    /// Provides dimension information about the scrolling control to the scroll controller.
    /// </summary>
    /// <param name="minOffset">Specifies the minimum offset allowed for the relevant scrolling orientation.</param>
    /// <param name="maxOffset">Specifies the maximum offset allowed for the relevant scrolling orientation.</param>
    /// <param name="offset">Specifies the current offset value.</param>
    /// <param name="viewportLength">Specifies the length of the scrolling control's viewport.</param>
    void SetValues(double minOffset, double maxOffset, double offset, double viewportLength);
}