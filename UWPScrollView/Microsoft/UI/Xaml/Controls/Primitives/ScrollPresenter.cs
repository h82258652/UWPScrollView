﻿using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents a primitive container that provides scroll, pan, and zoom support for its content.
/// </summary>
[ContentProperty(Name = nameof(Content))]
public class ScrollPresenter : FrameworkElement, IScrollAnchorProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollPresenter"/> class.
    /// </summary>
    public ScrollPresenter()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Occurs when the current interaction state of the control has changed.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, object> StateChanged;

    /// <summary>
    /// Occurs when manipulations such as scrolling and zooming have caused the view to change.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, object> ViewChanged;

    /// <summary>
    /// Gets or sets the content that can be scrolled, panned, or zoomed.
    /// </summary>
    public UIElement? Content
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
    public UIElement CurrentAnchor => throw new NotImplementedException();

    /// <summary>
    /// Gets the vertical size of all the scrollable content in the <see cref="ScrollPresenter"/>.
    /// </summary>
    public double ExtentHeight
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the horizontal size of all the scrollable content in the <see cref="ScrollPresenter"/>.
    /// </summary>
    public double ExtentWidth
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the distance the content has been scrolled horizontally.
    /// </summary>
    public double HorizontalOffset
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
    /// Gets or sets the minimum value for the read-only <see cref="ZoomFactor"/> property.
    /// </summary>
    public double MinZoomFactor
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
    public double ScrollableHeight
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the horizontal length of the content that can be scrolled.
    /// </summary>
    public double ScrollableWidth
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the current interaction state of the control.
    /// </summary>
    public ScrollingInteractionState State
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the distance the content has been scrolled vertically.
    /// </summary>
    public double VerticalOffset
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Asynchronously adds velocity to a scroll action.
    /// </summary>
    /// <param name="offsetsVelocity">The rate of the scroll offset change.</param>
    /// <param name="inertiaDecayRate">The decay rate of the inertia.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int AddScrollVelocity(Vector2 offsetsVelocity, Vector2? inertiaDecayRate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously adds velocity to a zoom action.
    /// </summary>
    /// <param name="zoomFactorVelocity">The rate of the zoom factor change.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <param name="inertiaDecayRate">The decay rate of the inertia.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int AddZoomVelocity(float zoomFactorVelocity, Vector2? centerPoint, float? inertiaDecayRate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Registers a <see cref="UIElement"/> as a potential scroll anchor.
    /// </summary>
    /// <param name="element">A <see cref="UIElement"/> within the subtree of the <see cref="ScrollPresenter"/>.</param>
    public void RegisterAnchorCandidate(UIElement element)
    {
        throw new NotImplementedException();
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
    /// Asynchronously scrolls to the specified offsets with animations enabled and snap points respected.
    /// </summary>
    /// <param name="horizontalOffset">The horizontal offset to scroll to.</param>
    /// <param name="verticalOffset">The vertical offset to scroll to.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollTo(double horizontalOffset, double verticalOffset)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously scrolls to the specified offsets with the specified animation and snap point modes.
    /// </summary>
    /// <param name="horizontalOffset">The horizontal offset to scroll to.</param>
    /// <param name="verticalOffset">The vertical offset to scroll to.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollTo(double horizontalOffset, double verticalOffset, ScrollingScrollOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unregisters a <see cref="UIElement"/> as a potential scroll anchor.
    /// </summary>
    /// <param name="element">A <see cref="UIElement"/> within the subtree of the <see cref="ScrollView"/>.</param>
    public void UnregisterAnchorCandidate(UIElement element)
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

    /// <summary>
    /// Asynchronously zooms to the specified zoom factor with animations enabled and snap points respected.
    /// </summary>
    /// <param name="zoomFactor">The amount to scale the content.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomTo(float zoomFactor, Vector2? centerPoint)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously zooms to the specified zoom factor with the specified animation and snap point modes.
    /// </summary>
    /// <param name="zoomFactor">The amount to scale the content.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomTo(float zoomFactor, Vector2? centerPoint, ScrollingZoomOptions options)
    {
        throw new NotImplementedException();
    }
}