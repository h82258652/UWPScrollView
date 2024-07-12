using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Represents a container that provides scroll, pan, and zoom support for its content.
/// </summary>
public class ScrollView : Control
{
    /// <summary>
    /// Identifies the <see cref="ComputedHorizontalScrollBarVisibility"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedHorizontalScrollBarVisibilityProperty = DependencyProperty.Register(
        nameof(ComputedHorizontalScrollBarVisibility),
        typeof(Visibility),
        typeof(ScrollView),
        new PropertyMetadata(Visibility.Collapsed, OnComputedHorizontalScrollBarVisibilityPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ComputedHorizontalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedHorizontalScrollModeProperty = DependencyProperty.Register(
        nameof(ComputedHorizontalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingScrollMode.Disabled, OnComputedHorizontalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ComputedVerticalScrollBarVisibility"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedVerticalScrollBarVisibilityProperty = DependencyProperty.Register(
        nameof(ComputedVerticalScrollBarVisibility),
        typeof(Visibility),
        typeof(ScrollView),
        new PropertyMetadata(Visibility.Collapsed, OnComputedVerticalScrollBarVisibilityPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ComputedVerticalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedVerticalScrollModeProperty = DependencyProperty.Register(
        nameof(ComputedVerticalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingScrollMode.Disabled, OnComputedVerticalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ContentOrientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentOrientationProperty = DependencyProperty.Register(
        nameof(ContentOrientation),
        typeof(ScrollingContentOrientation),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingContentOrientation.Vertical, OnContentOrientationPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="Content"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(UIElement),
        typeof(ScrollView),
        new PropertyMetadata(null, OnContentPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalAnchorRatio"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalAnchorRatioProperty = DependencyProperty.Register(
        nameof(HorizontalAnchorRatio),
        typeof(double),
        typeof(ScrollView),
        new PropertyMetadata(0d, OnHorizontalAnchorRatioPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollBarVisibility"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(
        nameof(HorizontalScrollBarVisibility),
        typeof(ScrollingScrollBarVisibility),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingScrollBarVisibility.Auto, OnHorizontalScrollBarVisibilityPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollChainMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollChainModeProperty = DependencyProperty.Register(
        nameof(HorizontalScrollChainMode),
        typeof(ScrollingChainMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingChainMode.Auto, OnHorizontalScrollChainModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollModeProperty = DependencyProperty.Register(
        nameof(HorizontalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingScrollMode.Auto, OnHorizontalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollRailMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollRailModeProperty = DependencyProperty.Register(
        nameof(HorizontalScrollRailMode),
        typeof(ScrollingRailMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingRailMode.Enabled, OnHorizontalScrollRailModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="IgnoredInputKinds"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IgnoredInputKindsProperty = DependencyProperty.Register(
        nameof(IgnoredInputKinds),
        typeof(ScrollingInputKinds),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingInputKinds.None, OnIgnoredInputKindsPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="MaxZoomFactor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaxZoomFactorProperty = DependencyProperty.Register(
        nameof(MaxZoomFactor),
        typeof(double),
        typeof(ScrollView),
        new PropertyMetadata(10d, OnMaxZoomFactorPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="MinZoomFactor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinZoomFactorProperty = DependencyProperty.Register(
        nameof(MinZoomFactor),
        typeof(double),
        typeof(ScrollView),
        new PropertyMetadata(0.1d, OnMinZoomFactorPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="VerticalAnchorRatio"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalAnchorRatioProperty;

    /// <summary>
    /// Identifies the <see cref="VerticalScrollBarVisibility"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(
        nameof(VerticalScrollBarVisibility),
        typeof(ScrollingScrollBarVisibility),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingScrollBarVisibility.Auto, OnVerticalScrollBarVisibilityPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="VerticalScrollChainMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollChainModeProperty;

    /// <summary>
    /// Identifies the <see cref="VerticalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollModeProperty = DependencyProperty.Register(
        nameof(VerticalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingScrollMode.Auto, OnVerticalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="VerticalScrollRailMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollRailModeProperty;

    /// <summary>
    /// Identifies the <see cref="ZoomChainMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ZoomChainModeProperty = DependencyProperty.Register(
        nameof(ZoomChainMode),
        typeof(ScrollingChainMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingChainMode.Auto, OnZoomChainModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ZoomMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ZoomModeProperty = DependencyProperty.Register(
        nameof(ZoomMode),
        typeof(ScrollingZoomMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingZoomMode.Disabled, OnZoomModePropertyChanged));

    private const string s_horizontalScrollBarPartName = "PART_HorizontalScrollBar";

    private const string s_IScrollAnchorProviderNotImpl = "Template part named PART_ScrollPresenter does not implement IScrollAnchorProvider.";

    private const string s_mouseIndicatorStateName = "MouseIndicator";

    private const string s_noIndicatorStateName = "NoIndicator";

    private const int s_noOpCorrelationId = -1;

    private const string s_noScrollPresenterPart = "No template part named PART_ScrollPresenter was loaded.";

    private const string s_rootPartName = "PART_Root";

    private const string s_scrollBarsSeparatorPartName = "PART_ScrollBarsSeparator";

    private const string s_scrollPresenterPartName = "PART_ScrollPresenter";

    private const string s_touchIndicatorStateName = "TouchIndicator";

    private const string s_verticalScrollBarPartName = "PART_VerticalScrollBar";

    private ScrollPresenter? _scrollPresenter;

    /// <summary>
    /// List of temporary ScrollViewBringIntoViewOperation instances used to track expected
    /// ScrollPresenter::BringingIntoView occurrences due to navigation.
    /// </summary>
    private List<ScrollViewBringIntoViewOperation> m_bringIntoViewOperations;

    private FocusInputDeviceKind m_focusInputDeviceKind = FocusInputDeviceKind.None;

    /// <summary>
    /// Indicates whether the NoIndicator visual state has a Storyboard for which a completion event was hooked up.
    /// </summary>
    private bool m_hasNoIndicatorStateStoryboardCompletedHandler;

    private DispatcherTimer m_hideIndicatorsTimer;

    private ScrollBarController? m_horizontalScrollBarController;

    private IScrollController m_horizontalScrollController;

    private UIElement m_horizontalScrollControllerElement;

    /// <summary>
    /// Set to the values of IScrollController::IsScrollingWithMouse.
    /// </summary>
    private bool m_isHorizontalScrollControllerScrollingWithMouse;

    private bool m_isLeftMouseButtonPressedForFocus = false;

    /// <summary>
    /// Set to True when the pointer is over the optional scroll controllers.
    /// </summary>
    private bool m_isPointerOverHorizontalScrollController;

    /// <summary>
    /// Set to True when the pointer is over the optional scroll controllers.
    /// </summary>
    private bool m_isPointerOverVerticalScrollController;

    /// <summary>
    /// Set to the values of IScrollController::IsScrollingWithMouse.
    /// </summary>
    private bool m_isVerticalScrollControllerScrollingWithMouse;

    /// <summary>
    /// Set to True to prevent the normal fade-out of the scrolling indicators.
    /// </summary>
    private bool m_keepIndicatorsShowing = false;

    private bool m_preferMouseIndicators = false;

    private UIElement m_scrollControllersSeparatorElement;

    /// <summary>
    /// Set to True when the mouse scrolling indicators are currently showing.
    /// </summary>
    private bool m_showingMouseIndicators = false;

    private ScrollBarController? m_verticalScrollBarController;

    private IScrollController m_verticalScrollController;

    private UIElement m_verticalScrollControllerElement;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollView"/> class.
    /// </summary>
    public ScrollView()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Occurs when the <see cref="ScrollView"/> is about to select an anchor element.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingAnchorRequestedEventArgs> AnchorRequested;

    /// <summary>
    /// Occurs at the beginning of a bring-into-view request participation. Allows customization of that participation.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingBringingIntoViewEventArgs> BringingIntoView;

    /// <summary>
    /// Occurs when either the <see cref="ExtentWidth"/> or <see cref="ExtentHeight"/> properties has changed.
    /// </summary>
    public event TypedEventHandler<ScrollView, object> ExtentChanged;

    /// <summary>
    /// Occurs when a call to <see cref="ScrollTo"/> or <see cref="ScrollBy"/> triggers an animation.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingScrollAnimationStartingEventArgs> ScrollAnimationStarting;

    /// <summary>
    /// Occurs when a <see cref="ScrollTo"/>, <see cref="ScrollBy"/>, or <see cref="AddScrollVelocity"/> asynchronous operation ends. Provides the original correlation ID.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingScrollCompletedEventArgs> ScrollCompleted;

    /// <summary>
    /// Occurs when the current interaction state of the control has changed.
    /// </summary>
    public event TypedEventHandler<ScrollView, object> StateChanged;

    /// <summary>
    /// Occurs when manipulations such as scrolling and zooming have caused the view to change.
    /// </summary>
    public event TypedEventHandler<ScrollView, object> ViewChanged;

    /// <summary>
    /// Occurs when a call to <see cref="ZoomTo"/> or <see cref="ZoomBy"/> triggers an animation.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingZoomAnimationStartingEventArgs> ZoomAnimationStarting;

    /// <summary>
    /// Occurs when a <see cref="ZoomTo"/>, <see cref="ZoomBy"/>, or <see cref="AddZoomVelocity"/> asynchronous operation ends. Provides the original correlation ID.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingZoomCompletedEventArgs> ZoomCompleted;

    /// <summary>
    /// Gets a value that indicates the effective visibility of the horizontal scrollbar.
    /// </summary>
    public Visibility ComputedHorizontalScrollBarVisibility
    {
        get
        {
            return (Visibility)GetValue(ComputedHorizontalScrollBarVisibilityProperty);
        }
    }

    /// <summary>
    /// Gets a value that indicates the effective ability to scroll horizontally by means of user input.
    /// </summary>
    public ScrollingScrollMode ComputedHorizontalScrollMode
    {
        get
        {
            return (ScrollingScrollMode)GetValue(ComputedHorizontalScrollModeProperty);
        }
    }

    /// <summary>
    /// Gets a value that indicates the effective visibility of the vertical scrollbar.
    /// </summary>
    public Visibility ComputedVerticalScrollBarVisibility
    {
        get
        {
            return (Visibility)GetValue(ComputedVerticalScrollBarVisibilityProperty);
        }
    }

    /// <summary>
    /// Gets a value that indicates the effective ability to scroll vertically by means of user input.
    /// </summary>
    public ScrollingScrollMode ComputedVerticalScrollMode
    {
        get
        {
            return (ScrollingScrollMode)GetValue(ComputedVerticalScrollModeProperty);
        }
    }

    /// <summary>
    /// Gets or sets the content that can be scrolled, panned, or zoomed.
    /// </summary>
    public UIElement? Content
    {
        get => (UIElement?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the content prefers to scroll horizontally or vertically.
    /// </summary>
    public ScrollingContentOrientation ContentOrientation
    {
        get => (ScrollingContentOrientation)GetValue(ContentOrientationProperty);
        set => SetValue(ContentOrientationProperty, value);
    }

    /// <summary>
    /// Gets the most recently chosen <see cref="UIElement"/> for scroll anchoring after a layout pass, if any.
    /// </summary>
    public UIElement? CurrentAnchor => (_scrollPresenter as IScrollAnchorProvider)?.CurrentAnchor;

    /// <summary>
    /// Gets a <see cref="CompositionPropertySet"/> of scrolling related property values.
    /// </summary>
    public CompositionPropertySet? ExpressionAnimationSources => _scrollPresenter?.ExpressionAnimationSources;

    /// <summary>
    /// Gets the vertical size of all the scrollable content in the <see cref="ScrollView"/>.
    /// </summary>
    public double ExtentHeight => _scrollPresenter?.ExtentHeight ?? 0;

    /// <summary>
    /// Gets the horizontal size of all the scrollable content in the ScrollView.
    /// </summary>
    public double ExtentWidth => _scrollPresenter?.ExtentWidth ?? 0;

    /// <summary>
    /// Gets or sets the ratio within the viewport where the anchor element is selected.
    /// </summary>
    public double HorizontalAnchorRatio
    {
        get => (double)GetValue(HorizontalAnchorRatioProperty);
        set
        {
            ValidateAnchorRatio(value);
            SetValue(HorizontalAnchorRatioProperty, value);
        }
    }

    /// <summary>
    /// Gets the distance the content has been scrolled horizontally.
    /// </summary>
    public double HorizontalOffset => _scrollPresenter?.HorizontalOffset ?? 0;

    /// <summary>
    /// Gets or sets a value that indicates whether a scroll controller should be displayed for the horizontal scrolling direction.
    /// </summary>
    public ScrollingScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => (ScrollingScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain horizontal scrolling to an outer scroll control.
    /// </summary>
    public ScrollingChainMode HorizontalScrollChainMode
    {
        get => (ScrollingChainMode)GetValue(HorizontalScrollChainModeProperty);
        set => SetValue(HorizontalScrollChainModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that determines how manipulation input influences scrolling behavior on the horizontal axis.
    /// </summary>
    public ScrollingScrollMode HorizontalScrollMode
    {
        get => (ScrollingScrollMode)GetValue(HorizontalScrollModeProperty);
        set => SetValue(HorizontalScrollModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the scroll rail is enabled for the horizontal axis.
    /// </summary>
    public ScrollingRailMode HorizontalScrollRailMode
    {
        get => (ScrollingRailMode)GetValue(HorizontalScrollRailModeProperty);
        set => SetValue(HorizontalScrollRailModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the kinds of user input the control does not respond to.
    /// </summary>
    public ScrollingInputKinds IgnoredInputKinds
    {
        get => (ScrollingInputKinds)GetValue(IgnoredInputKindsProperty);
        set => SetValue(IgnoredInputKindsProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum value for the read-only <see cref="ZoomFactor"/> property.
    /// </summary>
    public double MaxZoomFactor
    {
        get => (double)GetValue(MaxZoomFactorProperty);
        set
        {
            ValidateZoomFactoryBoundary(value);
            SetValue(MaxZoomFactorProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the minimum value for the read-only <see cref="ZoomFactor"/> property.
    /// </summary>
    public double MinZoomFactor
    {
        get => (double)GetValue(MinZoomFactorProperty);
        set
        {
            ValidateZoomFactoryBoundary(value);
            SetValue(MinZoomFactorProperty, value);
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
    public ScrollPresenter? ScrollPresenter => _scrollPresenter;

    /// <summary>
    /// Gets the current interaction state of the control.
    /// </summary>
    public ScrollingInteractionState State => _scrollPresenter?.State ?? ScrollingInteractionState.Idle;

    /// <summary>
    /// Determines the vertical position of the <see cref="ScrollView"/>'s anchor point with respect to the viewport. By default, the <see cref="ScrollView"/> selects an element as its <see cref="CurrentAnchor"/> by identifying the element in its viewport nearest to the anchor point.
    /// </summary>
    public double VerticalAnchorRatio
    {
        get
        {
            return (double)GetValue(VerticalAnchorRatioProperty);
        }
        set
        {
            ValidateAnchorRatio(value);
            SetValue(VerticalAnchorRatioProperty, value);
        }
    }

    /// <summary>
    /// Gets the distance the content has been scrolled vertically.
    /// </summary>
    public double VerticalOffset => _scrollPresenter?.VerticalOffset ?? 0;

    /// <summary>
    /// Gets or sets a value that indicates whether a scroll controller should be displayed for the vertical scrolling direction.
    /// </summary>
    public ScrollingScrollBarVisibility VerticalScrollBarVisibility
    {
        get
        {
            return (ScrollingScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
        }
        set
        {
            SetValue(VerticalScrollBarVisibilityProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain vertical scrolling to an outer scroll control.
    /// </summary>
    public ScrollingChainMode VerticalScrollChainMode
    {
        get
        {
            return (ScrollingChainMode)GetValue(VerticalScrollChainModeProperty);
        }
        set
        {
            SetValue(VerticalScrollChainModeProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets a value that determines how manipulation input influences scrolling behavior on the horizontal axis.
    /// </summary>
    public ScrollingScrollMode VerticalScrollMode
    {
        get => (ScrollingScrollMode)GetValue(VerticalScrollModeProperty);
        set => SetValue(VerticalScrollModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the scroll rail is enabled for the vertical axis.
    /// </summary>
    public ScrollingRailMode VerticalScrollRailMode
    {
        get
        {
            return (ScrollingRailMode)GetValue(VerticalScrollRailModeProperty);
        }
        set
        {
            SetValue(VerticalScrollRailModeProperty, value);
        }
    }

    /// <summary>
    /// Gets the vertical size of the viewable content in the <see cref="ScrollView"/>.
    /// </summary>
    public double ViewportHeight => _scrollPresenter?.ViewportHeight ?? 0;

    /// <summary>
    /// Gets the horizontal size of the viewable content in the <see cref="ScrollView"/>.
    /// </summary>
    public double ViewportWidth => _scrollPresenter?.ViewportWidth ?? 0;

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain zooming to an outer scroll control.
    /// </summary>
    public ScrollingChainMode ZoomChainMode
    {
        get => (ScrollingChainMode)GetValue(ZoomChainModeProperty);
        set => SetValue(ZoomChainModeProperty, value);
    }

    /// <summary>
    /// Gets a value that indicates the amount of scaling currently applied to content.
    /// </summary>
    public float ZoomFactor => _scrollPresenter?.ZoomFactor ?? 0;

    /// <summary>
    /// Gets or sets a value that indicates the ability to zoom in and out by means of user input.
    /// </summary>
    public ScrollingZoomMode ZoomMode
    {
        get => (ScrollingZoomMode)GetValue(ZoomModeProperty);
        set => SetValue(ZoomModeProperty, value);
    }

    /// <summary>
    /// Asynchronously adds velocity to a scroll action.
    /// </summary>
    /// <param name="offsetsVelocity">The rate of the scroll offset change.</param>
    /// <param name="inertiaDecayRate">The decay rate of the inertia.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int AddScrollVelocity(Vector2 offsetsVelocity, Vector2? inertiaDecayRate)
    {
        return _scrollPresenter?.AddScrollVelocity(offsetsVelocity, inertiaDecayRate) ?? s_noOpCorrelationId;
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
        return _scrollPresenter?.AddZoomVelocity(zoomFactorVelocity, centerPoint, inertiaDecayRate) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously scrolls by the specified delta amount with animations enabled and snap points respected.
    /// </summary>
    /// <param name="horizontalOffsetDelta">The offset to scroll by horizontally.</param>
    /// <param name="verticalOffsetDelta">The offset to scroll by vertically.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta)
    {
        return _scrollPresenter?.ScrollBy(horizontalOffsetDelta, verticalOffsetDelta) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously scrolls by the specified delta amount with the specified animation and snap point modes.
    /// </summary>
    /// <param name="horizontalOffsetDelta">The offset to scroll by horizontally.</param>
    /// <param name="verticalOffsetDelta">The offset to scroll by vertically.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta, ScrollingScrollOptions? options)
    {
        return _scrollPresenter?.ScrollBy(horizontalOffsetDelta, verticalOffsetDelta, options) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously scrolls to the specified offsets with animations enabled and snap points respected.
    /// </summary>
    /// <param name="horizontalOffset">The horizontal offset to scroll to.</param>
    /// <param name="verticalOffset">The vertical offset to scroll to.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollTo(double horizontalOffset, double verticalOffset)
    {
        return _scrollPresenter?.ScrollTo(horizontalOffset, verticalOffset) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously scrolls to the specified offsets with the specified animation and snap point modes.
    /// </summary>
    /// <param name="horizontalOffset">The horizontal offset to scroll to.</param>
    /// <param name="verticalOffset">The vertical offset to scroll to.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollTo(double horizontalOffset, double verticalOffset, ScrollingScrollOptions? options)
    {
        return _scrollPresenter?.ScrollTo(horizontalOffset, verticalOffset, options) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously zooms by the specified delta amount with animations enabled and snap point respected.
    /// </summary>
    /// <param name="zoomFactorDelta">The amount by which to change the zoom factor.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint)
    {
        return _scrollPresenter?.ZoomBy(zoomFactorDelta, centerPoint) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously zooms by the specified delta amount with the specified animation and snap point modes.
    /// </summary>
    /// <param name="zoomFactorDelta">The amount by which to change the zoom factor.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint, ScrollingZoomOptions? options)
    {
        return _scrollPresenter?.ZoomBy(zoomFactorDelta, centerPoint, options) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously zooms to the specified zoom factor with animations enabled and snap points respected.
    /// </summary>
    /// <param name="zoomFactor">The amount to scale the content.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomTo(float zoomFactor, Vector2? centerPoint)
    {
        return _scrollPresenter?.ZoomTo(zoomFactor, centerPoint) ?? s_noOpCorrelationId;
    }

    /// <summary>
    /// Asynchronously zooms to the specified zoom factor with the specified animation and snap point modes.
    /// </summary>
    /// <param name="zoomFactor">The amount to scale the content.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <param name="options">Options that specify whether or not animations are enabled and snap points are respected.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomTo(float zoomFactor, Vector2? centerPoint, ScrollingZoomOptions? options)
    {
        return _scrollPresenter?.ZoomTo(zoomFactor, centerPoint, options) ?? s_noOpCorrelationId;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        m_hasNoIndicatorStateStoryboardCompletedHandler = false;
        m_keepIndicatorsShowing = false;

        Control thisAsControlProtected = this;

        ScrollPresenter scrollPresenter = (ScrollPresenter)GetTemplateChild(s_scrollPresenterPartName);

        UpdateScrollPresenter(scrollPresenter);

        UIElement horizontalScrollControllerElement = (UIElement)GetTemplateChild(s_horizontalScrollBarPartName);
        IScrollController? horizontalScrollController = horizontalScrollControllerElement as IScrollController;
        ScrollBar horizontalScrollBar = null;

        if (horizontalScrollControllerElement is not null && horizontalScrollController is null)
        {
            horizontalScrollBar = horizontalScrollControllerElement as ScrollBar;

            if (horizontalScrollBar is not null)
            {
                if (m_horizontalScrollBarController is null)
                {
                    m_horizontalScrollBarController = new ScrollBarController();
                }
                horizontalScrollController = m_horizontalScrollBarController as IScrollController;
            }
        }

        if (horizontalScrollBar is not null)
        {
            m_horizontalScrollBarController.SetScrollBar(horizontalScrollBar);
        }
        else
        {
            m_horizontalScrollBarController = null;
        }

        UpdateHorizontalScrollController(horizontalScrollController, horizontalScrollControllerElement);

        UIElement verticalScrollControllerElement = (UIElement)GetTemplateChild(s_verticalScrollBarPartName);
        IScrollController verticalScrollController = verticalScrollControllerElement as IScrollController;
        ScrollBar verticalScrollBar = null;

        if (verticalScrollControllerElement is not null && verticalScrollController is null)
        {
            verticalScrollBar = verticalScrollControllerElement as ScrollBar;

            if (verticalScrollBar is not null)
            {
                if (m_verticalScrollBarController is null)
                {
                    m_verticalScrollBarController = new ScrollBarController();
                }
                verticalScrollController = m_verticalScrollBarController as IScrollController;
            }
        }

        if (verticalScrollBar is not null)
        {
            m_verticalScrollBarController.SetScrollBar(verticalScrollBar);
        }
        else
        {
            m_verticalScrollBarController = null;
        }

        UpdateVerticalScrollController(verticalScrollController, verticalScrollControllerElement);

        UIElement scrollControllersSeparator = (UIElement)GetTemplateChild(s_scrollBarsSeparatorPartName);

        UpdateScrollControllersSeparator(scrollControllersSeparator);

        UpdateScrollControllersVisibility(true /*horizontalChange*/, true /*verticalChange*/);

        FrameworkElement root = (FrameworkElement)GetTemplateChild(s_rootPartName);

        if (root is not null)
        {
            var rootVisualStateGroups = VisualStateManager.GetVisualStateGroups(root);

            if (rootVisualStateGroups is not null)
            {
                var groupCount = rootVisualStateGroups.Count;

                for (var groupIndex = 0; groupIndex < groupCount; ++groupIndex)
                {
                    VisualStateGroup group = rootVisualStateGroups[groupIndex];

                    if (group is not null)
                    {
                        var visualStates = group.States;

                        if (visualStates is not null)
                        {
                            var stateCount = visualStates.Count;

                            for (var stateIndex = 0; stateIndex < stateCount; ++stateIndex)
                            {
                                VisualState state = visualStates[stateIndex];

                                if (state is not null)
                                {
                                    var stateName = state.Name;
                                    Storyboard stateStoryboard = state.Storyboard;

                                    if (stateStoryboard is not null)
                                    {
                                        if (stateName == s_noIndicatorStateName)
                                        {
                                            stateStoryboard.Completed += OnNoIndicatorStateStoryboardCompleted;
                                            m_hasNoIndicatorStateStoryboardCompletedHandler = true;
                                        }
                                        else if (stateName == s_touchIndicatorStateName || stateName == s_mouseIndicatorStateName)
                                        {
                                            stateStoryboard.Completed += OnIndicatorStateStoryboardCompleted;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        UpdateVisualStates(false /*useTransitions*/);
    }

    /// <inheritdoc/>
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);

        m_preferMouseIndicators =
            m_focusInputDeviceKind == FocusInputDeviceKind.Mouse ||
            m_focusInputDeviceKind == FocusInputDeviceKind.Pen;

        UpdateVisualStates(
            useTransitions: true,
            showIndicators: true,
            hideIndicators: false,
            scrollControllersAutoHidingChanged: false,
            updateScrollControllersAutoHiding: true,
            onlyForAutoHidingScrollControllers: true);
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        base.OnKeyDown(e);

        m_preferMouseIndicators = false;

        if (_scrollPresenter is not null)
        {
            KeyRoutedEventArgs eventArgs = e;
            if (!eventArgs.Handled)
            {
                var originalKey = eventArgs.OriginalKey;
                bool isGamepadKey = FocusHelper.IsGamepadNavigationDirection(originalKey) || FocusHelper.IsGamepadPageNavigationDirection(originalKey);

                if (isGamepadKey)
                {
                    if (IsInputKindIgnored(ScrollingInputKinds.Gamepad))
                    {
                        return;
                    }
                }
                else
                {
                    if (IsInputKindIgnored(ScrollingInputKinds.Keyboard))
                    {
                        return;
                    }
                }

                bool isXYFocusEnabledForKeyboard = XYFocusKeyboardNavigation == XYFocusKeyboardNavigationMode.Enabled;
                bool doXYFocusScrolling = isGamepadKey || isXYFocusEnabledForKeyboard;

                if (doXYFocusScrolling)
                {
                    HandleKeyDownForXYNavigation(eventArgs);
                }
                else
                {
                    HandleKeyDownForStandardScroll(eventArgs);
                }
            }
        }
    }

    private static void OnComputedHorizontalScrollBarVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnComputedHorizontalScrollModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnComputedVerticalScrollBarVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnComputedVerticalScrollModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnContentOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalAnchorRatioPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private static void OnHorizontalScrollBarVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnHorizontalScrollChainModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnHorizontalScrollModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnHorizontalScrollRailModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnIgnoredInputKindsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnMaxZoomFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnMinZoomFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnVerticalScrollBarVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnVerticalScrollModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnZoomChainModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnZoomModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void ValidateAnchorRatio(double value)
    {
        ScrollPresenter.ValidateAnchorRatio(value);
    }

    private static void ValidateZoomFactoryBoundary(double value)
    {
        throw new NotImplementedException();
    }

    private bool AreAllScrollControllersCollapsed()
    {
        throw new NotImplementedException();
    }

    private bool AreScrollControllersAutoHiding()
    {
        throw new NotImplementedException();
    }

    private bool CanScrollDown()
    {
        return CanScrollVerticallyInDirection(inPositiveDirection: true);
    }

    private bool CanScrollHorizontallyInDirection(bool inPositiveDirection)
    {
        throw new NotImplementedException();
    }

    private bool CanScrollInDirection(FocusNavigationDirection direction)
    {
        return direction switch
        {
            FocusNavigationDirection.Down => CanScrollDown(),
            FocusNavigationDirection.Up => CanScrollUp(),
            FocusNavigationDirection.Left => CanScrollLeft(),
            FocusNavigationDirection.Right => CanScrollRight(),
            _ => false
        };
    }

    private bool CanScrollLeft()
    {
        return CanScrollHorizontallyInDirection(inPositiveDirection: false);
    }

    private bool CanScrollRight()
    {
        return CanScrollHorizontallyInDirection(inPositiveDirection: true);
    }

    private bool CanScrollUp()
    {
        return CanScrollVerticallyInDirection(inPositiveDirection: false);
    }

    private bool CanScrollVerticallyInDirection(bool inPositiveDirection)
    {
        throw new NotImplementedException();
    }

    private void DoScroll(double offsetAmount, Orientation orientation)
    {
        throw new NotImplementedException();
    }

    private bool DoScrollForKey(VirtualKey key, double scrollProportion)
    {
        throw new NotImplementedException();
    }

    private void GoToState(string stateName, bool useTransitions = true)
    {
        VisualStateManager.GoToState(this, stateName, useTransitions);
    }

    private void HandleKeyDownForStandardScroll(KeyRoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void HandleKeyDownForXYNavigation(KeyRoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void HandleScrollControllerPointerEntered(bool isForHorizontalScrollController)
    {
        throw new NotImplementedException();
    }

    private void HandleScrollControllerPointerExited(bool isForHorizontalScrollController)
    {
        throw new NotImplementedException();
    }

    private void HideIndicators(bool useTransitions = true)
    { throw new NotImplementedException(); }

    private void HideIndicatorsAfterDelay()
    {
        throw new NotImplementedException();
    }

    private void HookCompositionTargetRendering()
    {
        throw new NotImplementedException();
    }

    private void HookHorizontalScrollControllerEvents()
    {
        throw new NotImplementedException();
    }

    private void HookScrollPresenterEvents()
    {
        throw new NotImplementedException();
    }

    private void HookScrollViewEvents()
    {
        throw new NotImplementedException();
    }

    private void HookUISettingsEvent()
    {
        throw new NotImplementedException();
    }

    private void HookVerticalScrollControllerEvents()
    {
        throw new NotImplementedException();
    }

    private bool IsInputKindIgnored(ScrollingInputKinds inputKind)
    {
        return (IgnoredInputKinds & inputKind) == inputKind;
    }

    private void OnCompositionTargetRendering(object sender, object args)
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

    private void OnIndicatorStateStoryboardCompleted(
         object sender,
         object args)
    {
        throw new NotImplementedException();
    }

    private void OnNoIndicatorStateStoryboardCompleted(
            object sender,
            object args)
    {
        throw new NotImplementedException();
    }

    private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
    {
        DependencyProperty dependencyProperty = args.Property;

        bool horizontalChange = dependencyProperty == HorizontalScrollBarVisibilityProperty;
        bool verticalChange = dependencyProperty == VerticalScrollBarVisibilityProperty;

        if (horizontalChange || verticalChange)
        {
            UpdateScrollControllersVisibility(horizontalChange, verticalChange);
            UpdateVisualStates(
                useTransitions: true,
                showIndicators: false,
                hideIndicators: false,
                scrollControllersAutoHidingChanged: false,
                updateScrollControllersAutoHiding: true);
        }
    }

    private void OnScrollPresenterExtentChanged(object sender, object args)
    {
        throw new NotImplementedException();
    }

    private void OnScrollPresenterStateChanged(object sender, object args)
    {
        throw new NotImplementedException();
    }

    private void OnScrollViewGettingFocus(object sender, GettingFocusEventArgs args)
    {
        m_focusInputDeviceKind = args.InputDevice;
    }

    private void OnScrollViewIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        UpdateVisualStates(
            useTransitions: true,
            showIndicators: false,
            hideIndicators: false,
            scrollControllersAutoHidingChanged: false,
            updateScrollControllersAutoHiding: true);
    }

    private void OnScrollViewPointerCanceled(object sender, PointerRoutedEventArgs args)
    {
        if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
        {
            m_isLeftMouseButtonPressedForFocus = false;
        }
    }

    private void OnScrollViewPointerEntered(object sender, PointerRoutedEventArgs args)
    {
        if (args.Pointer.PointerDeviceType != PointerDeviceType.Touch)
        {
            // Mouse/Pen inputs dominate. If touch panning indicators are shown, switch to mouse indicators.
            m_preferMouseIndicators = true;

            UpdateVisualStates(
                useTransitions: true,
                showIndicators: true,
                hideIndicators: false,
                scrollControllersAutoHidingChanged: false,
                updateScrollControllersAutoHiding: true,
                onlyForAutoHidingScrollControllers: true);
        }
    }

    private void OnScrollViewPointerMoved(object sender, PointerRoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnVerticalScrollControllerPointerExited(object sender, PointerRoutedEventArgs args)
    {
        HandleScrollControllerPointerExited(isForHorizontalScrollController: false);
    }

    private void OnZoomAnimationStarting(object sender, ScrollingZoomAnimationStartingEventArgs args)
    {
        ZoomAnimationStarting?.Invoke(this, args);
    }

    private void ResetHideIndicatorsTimer(bool isForDestructor = false, bool restart = false)
    {
        throw new NotImplementedException();
    }

    private void UnhookCompositionTargetRendering()
    {
        throw new NotImplementedException();
    }

    private void UnhookHorizontalScrollControllerEvents(bool isForDestructor)
    {
        throw new NotImplementedException();
    }

    private void UnhookScrollPresenterEvents(bool isForDestructor)
    {
        throw new NotImplementedException();
    }

    private void UnhookScrollViewEvents()
    {
        throw new NotImplementedException();
    }

    private void UnhookVerticalScrollControllerEvents(bool isForDestructor)
    {
        throw new NotImplementedException();
    }

    private void UpdateHorizontalScrollController(
                                                                                                                                                                                                                                                                                        IScrollController horizontalScrollController,
        UIElement horizontalScrollControllerElement)
    {
        throw new NotImplementedException();
    }

    private void UpdateScrollControllersAutoHiding(bool forceUpdate = false)
    {
        throw new NotImplementedException();
    }

    private void UpdateScrollControllersSeparator(UIElement scrollControllersSeparator)
    { throw new NotImplementedException(); }

    private void UpdateScrollControllersSeparatorVisualState(bool useTransitions = true, bool scrollControllersAutoHidingChanged = false)
    {
        throw new NotImplementedException();
    }

    private void UpdateScrollControllersVisibility(bool horizontalChange, bool verticalChange)
    {
        Debug.Assert(horizontalChange || verticalChange);

        bool isHorizontalScrollControllerVisible = false;

        if (horizontalChange)
        {
            var scrollBarVisibility = HorizontalScrollBarVisibility;

            if (scrollBarVisibility == ScrollingScrollBarVisibility.Auto &&
                m_horizontalScrollController is not null &&
                m_horizontalScrollController.CanScroll)
            {
                isHorizontalScrollControllerVisible = true;
            }
            else
            {
                isHorizontalScrollControllerVisible = (scrollBarVisibility == ScrollingScrollBarVisibility.Visible);
            }

            SetValue(ComputedHorizontalScrollBarVisibilityProperty, isHorizontalScrollControllerVisible ? Visibility.Visible : Visibility.Collapsed);
        }
        else
        {
            isHorizontalScrollControllerVisible = ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        }

        bool isVerticalScrollControllerVisible = false;

        if (verticalChange)
        {
            var scrollBarVisibility = VerticalScrollBarVisibility;

            if (scrollBarVisibility == ScrollingScrollBarVisibility.Auto &&
                m_verticalScrollController is not null &&
                m_verticalScrollController.CanScroll)
            {
                isVerticalScrollControllerVisible = true;
            }
            else
            {
                isVerticalScrollControllerVisible = (scrollBarVisibility == ScrollingScrollBarVisibility.Visible);
            }

            SetValue(ComputedVerticalScrollBarVisibilityProperty, isVerticalScrollControllerVisible ? Visibility.Visible : Visibility.Collapsed);
        }
        else
        {
            isVerticalScrollControllerVisible = ComputedVerticalScrollBarVisibility == Visibility.Visible;
        }

        if (m_scrollControllersSeparatorElement is not null)
        {
            m_scrollControllersSeparatorElement.Visibility = (isHorizontalScrollControllerVisible && isVerticalScrollControllerVisible ?
                Visibility.Visible : Visibility.Collapsed);
        }
    }

    private void UpdateScrollControllersVisualState(bool useTransitions = true, bool showIndicators = false, bool hideIndicators = false)
    {
        Debug.Assert(!(showIndicators && hideIndicators));

        bool areScrollControllersAutoHiding = AreScrollControllersAutoHiding();

        Debug.Assert(!(!areScrollControllersAutoHiding && hideIndicators));

        if ((!areScrollControllersAutoHiding || showIndicators) && !hideIndicators)
        {
            if (AreAllScrollControllersCollapsed())
            {
                return;
            }

            ResetHideIndicatorsTimer(false /*isForDestructor*/, true /*restart*/);

            // Mouse indicators dominate if they are already showing or if we have set the flag to prefer them.
            if (m_preferMouseIndicators || m_showingMouseIndicators || !areScrollControllersAutoHiding)
            {
                GoToState(s_mouseIndicatorStateName, useTransitions);

                m_showingMouseIndicators = true;
            }
            else
            {
                GoToState(s_touchIndicatorStateName, useTransitions);
            }
        }
        else if (!m_keepIndicatorsShowing)
        {
            if (SharedHelpers.IsAnimationsEnabled())
            {
                // By default there is a delay before the NoIndicator state actually shows.
                HideIndicators();
            }
            else
            {
                // Since OS animations are turned off, use a timer to delay the indicators' hiding.
                HideIndicatorsAfterDelay();
            }
        }
    }

    private void UpdateScrollPresenter(ScrollPresenter scrollPresenter)
    {
        throw new NotImplementedException();
    }

    private void UpdateVerticalScrollController(
                        IScrollController verticalScrollController,
        UIElement verticalScrollControllerElement)
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
        if (updateScrollControllersAutoHiding)
        {
            UpdateScrollControllersAutoHiding();
        }

        if (onlyForAutoHidingScrollControllers && !AreScrollControllersAutoHiding())
        {
            return;
        }

        UpdateScrollControllersVisualState(useTransitions, showIndicators, hideIndicators);
        UpdateScrollControllersSeparatorVisualState(useTransitions, scrollControllersAutoHidingChanged);
    }
}