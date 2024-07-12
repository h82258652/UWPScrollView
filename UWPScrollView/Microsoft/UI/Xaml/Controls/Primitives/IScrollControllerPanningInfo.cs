using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Encapsulates information related to UI-thread-independent panning that an <see cref="IScrollController"/> implementation may support.
/// </summary>
public interface IScrollControllerPanningInfo
{
    /// <summary>
    /// Occurs when any <see cref="IScrollControllerPanningInfo"/> property changes.
    /// </summary>
    event TypedEventHandler<IScrollControllerPanningInfo, object> Changed;

    /// <summary>
    /// Occurs when a user attempts to initiate a pan with touch or a pen.
    /// </summary>
    event TypedEventHandler<IScrollControllerPanningInfo, ScrollControllerPanRequestedEventArgs> PanRequested;

    /// <summary>
    /// Gets a value that indicates whether or not the movement of a pannable element must be locked on one orientation (horizontal or vertical).
    /// </summary>
    bool IsRailEnabled { get; }

    /// <summary>
    /// Gets an ancestor of a <see cref="UIElement"/> that can be panned with touch off the UI-thread, like the <see cref="ScrollPresenter"/>'s content.
    /// </summary>
    UIElement? PanningElementAncestor { get; }

    /// <summary>
    /// Gets a value that indicates whether the content can be panned horizontally or vertically.
    /// </summary>
    Orientation PanOrientation { get; }

    /// <summary>
    /// Provides information for expression animations used to support panning.
    /// </summary>
    /// <param name="propertySet">A set of composition properties.</param>
    /// <param name="minOffsetPropertyName">The minimum scroll offset value.</param>
    /// <param name="maxOffsetPropertyName">The maximum scroll offset value.</param>
    /// <param name="offsetPropertyName">The current scroll offset value.</param>
    /// <param name="multiplierPropertyName">A ratio that represents the relative movement of the content <see cref="PanningElementAncestor"/>'s child and <see cref="ScrollPresenter"/> content.</param>
    void SetPanningElementExpressionAnimationSources(CompositionPropertySet propertySet, string minOffsetPropertyName, string maxOffsetPropertyName, string offsetPropertyName, string multiplierPropertyName);
}