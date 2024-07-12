using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Represents a container that provides scroll, pan, and zoom support for its content.
/// </summary>
public class ScrollView : Control
{
    private ScrollPresenter _scrollPresenter;

    /// <summary>
    /// Gets or sets the content that can be scrolled, panned, or zoomed.
    /// </summary>
    public UIElement Content
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
    /// Gets the most recently chosen <see cref="UIElement"/> for scroll anchoring after a layout pass, if any.
    /// </summary>
    public UIElement? CurrentAnchor
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the vertical size of all the scrollable content in the <see cref="ScrollView"/>.
    /// </summary>
    public double ExtentHeight => _scrollPresenter?.ExtentHeight ?? 0;

    /// <summary>
    /// Gets the horizontal size of all the scrollable content in the ScrollView.
    /// </summary>
    public double ExtentWidth
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets or sets a value that determines how manipulation input influences scrolling behavior on the horizontal axis.
    /// </summary>
    public ScrollingScrollMode HorizontalScrollMode
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
    /// Gets or sets a value that indicates whether the scroll rail is enabled for the horizontal axis.
    /// </summary>
    public ScrollingRailMode HorizontalScrollRailMode
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
    /// Gets or sets the maximum value for the read-only <see cref="ZoomFactor"/> property.
    /// </summary>
    public double MaxZoomFactor
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
    /// Gets the vertical length of the content that can be scrolled.
    /// </summary>
    public double ScrollableHeight => _scrollPresenter?.ScrollableHeight ?? 0;

    /// <summary>
    /// Gets the horizontal length of the content that can be scrolled.
    /// </summary>
    public double ScrollableWidth => _scrollPresenter?.ScrollableWidth ?? 0;

    /// <summary>
    /// Gets the loaded <see cref="ScrollPresenter"/> control template part.
    /// </summary>
    public ScrollPresenter? ScrollPresenter
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the current interaction state of the control.
    /// </summary>
    public ScrollingInteractionState State => _scrollPresenter?.State ?? ScrollingInteractionState.Idle;

    /// <summary>
    /// Gets or sets a value that determines how manipulation input influences scrolling behavior on the horizontal axis.
    /// </summary>
    public ScrollingScrollMode VerticalScrollMode
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
    /// Gets the vertical size of the viewable content in the <see cref="ScrollView"/>.
    /// </summary>
    public double ViewportHeight
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the horizontal size of the viewable content in the <see cref="ScrollView"/>.
    /// </summary>
    public double ViewportWidth => _scrollPresenter?.ViewportWidth ?? 0;

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain zooming to an outer scroll control.
    /// </summary>
    public ScrollingChainMode ZoomChainMode
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
    /// Gets a value that indicates the amount of scaling currently applied to content.
    /// </summary>
    public float ZoomFactor
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Asynchronously scrolls by the specified delta amount with animations enabled and snap points respected.
    /// </summary>
    /// <param name="horizontalOffsetDelta">The offset to scroll by horizontally.</param>
    /// <param name="verticalOffsetDelta">The offset to scroll by vertically.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously scrolls by the specified delta amount with the specified animation and snap point modes.
    /// </summary>
    /// <param name="horizontalOffsetDelta">The offset to scroll by horizontally.</param>
    /// <param name="verticalOffsetDelta">The offset to scroll by vertically.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta, ScrollingScrollOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously zooms by the specified delta amount with animations enabled and snap point respected.
    /// </summary>
    /// <param name="zoomFactorDelta">The amount by which to change the zoom factor.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously zooms by the specified delta amount with the specified animation and snap point modes.
    /// </summary>
    /// <param name="zoomFactorDelta">The amount by which to change the zoom factor.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint, ScrollingZoomOptions options)
    {
        throw new NotImplementedException();
    }

    private void OnHideIndicatorsTimerTick(object sender, object args)
    {
        throw new NotImplementedException();
    }

    private void OnHorizontalScrollControllerPointerEntered(object sender, PointerRoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnScrollViewGettingFocus(object sender, GettingFocusEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnScrollViewIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnScrollViewPointerMoved(object sender, PointerRoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnVerticalScrollControllerPointerExited(object sender, PointerRoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void UpdateVisualStates(
                    bool useTransitions = true,
        bool showIndicators = false,
        bool hideIndicators = false,
        bool scrollControllersAutoHidingChanged = false,
        bool updateScrollControllersAutoHiding = false,
        bool onlyForAutoHidingScrollControllers = false)
    {
        throw new NotImplementedException();
    }
}