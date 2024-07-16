using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Animation;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Represents a container that provides scroll, pan, and zoom support for its content.
/// </summary>
[ContentProperty(Name = nameof(Content))]
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
    /// Identifies the <see cref="ScrollPresenter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ScrollPresenterProperty = DependencyProperty.Register(
        nameof(ScrollPresenter),
        typeof(ScrollPresenter),
        typeof(ScrollView),
        new PropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="VerticalAnchorRatio"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalAnchorRatioProperty = DependencyProperty.Register(
        nameof(VerticalAnchorRatio),
        typeof(double),
        typeof(ScrollView),
        new PropertyMetadata(0d, OnVerticalAnchorRatioPropertyChanged));

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
    public static readonly DependencyProperty VerticalScrollChainModeProperty = DependencyProperty.Register(
        nameof(VerticalScrollChainMode),
        typeof(ScrollingChainMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingChainMode.Auto, OnVerticalScrollChainModePropertyChanged));

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
    public static readonly DependencyProperty VerticalScrollRailModeProperty = DependencyProperty.Register(
        nameof(VerticalScrollRailMode),
        typeof(ScrollingRailMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingRailMode.Enabled, OnVerticalScrollRailModePropertyChanged));

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

    /// <summary>
    /// 2 seconds delay used to hide the indicators for example when OS animations are turned off.
    /// </summary>
    private const long s_noIndicatorCountdown = 2000 * 10000;

    private const string s_noIndicatorStateName = "NoIndicator";
    private const int s_noOpCorrelationId = -1;
    private const string s_noScrollPresenterPart = "No template part named PART_ScrollPresenter was loaded.";
    private const string s_rootPartName = "PART_Root";
    private const string s_scrollBarsSeparatorCollapsed = "ScrollBarsSeparatorCollapsed";
    private const string s_scrollBarsSeparatorCollapsedDisabled = "ScrollBarsSeparatorCollapsedDisabled";
    private const string s_scrollBarsSeparatorCollapsedWithoutAnimation = "ScrollBarsSeparatorCollapsedWithoutAnimation";
    private const string s_scrollBarsSeparatorDisplayedWithoutAnimation = "ScrollBarsSeparatorDisplayedWithoutAnimation";
    private const string s_scrollBarsSeparatorExpanded = "ScrollBarsSeparatorExpanded";
    private const string s_scrollBarsSeparatorExpandedWithoutAnimation = "ScrollBarsSeparatorExpandedWithoutAnimation";
    private const string s_scrollBarsSeparatorPartName = "PART_ScrollBarsSeparator";
    private const string s_scrollPresenterPartName = "PART_ScrollPresenter";
    private const string s_touchIndicatorStateName = "TouchIndicator";
    private const string s_verticalScrollBarPartName = "PART_VerticalScrollBar";

    /// <summary>
    /// List of temporary ScrollViewBringIntoViewOperation instances used to track expected
    /// ScrollPresenter::BringingIntoView occurrences due to navigation.
    /// </summary>
    private readonly List<ScrollViewBringIntoViewOperation> m_bringIntoViewOperations = new();

    private readonly DispatcherQueue m_dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private ScrollPresenter? _scrollPresenter;

    /// <summary>
    /// Used to detect changes for UISettings.AutoHideScrollBars
    /// </summary>
    private AutoHideScrollBarsState m_autoHideScrollBarsState;

    private bool m_autoHideScrollControllers = false;

    private bool m_autoHideScrollControllersValid = false;
    private FocusInputDeviceKind m_focusInputDeviceKind = FocusInputDeviceKind.None;

    /// <summary>
    /// Indicates whether the NoIndicator visual state has a Storyboard for which a completion event was hooked up.
    /// </summary>
    private bool m_hasNoIndicatorStateStoryboardCompletedHandler;

    private DispatcherTimer m_hideIndicatorsTimer;

    private int m_horizontalAddScrollVelocityDirection;

    private int m_horizontalAddScrollVelocityOffsetChangeCorrelationId = -1;

    private ScrollBarController? m_horizontalScrollBarController;

    private IScrollController? m_horizontalScrollController;

    private UIElement? m_horizontalScrollControllerElement;

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

    private object? m_onHorizontalScrollControllerPointerEnteredHandler;
    private object? m_onHorizontalScrollControllerPointerExitedHandler;
    private object? m_onPointerCanceledEventHandler;
    private object? m_onPointerEnteredEventHandler;
    private object? m_onPointerExitedEventHandler;
    private object? m_onPointerMovedEventHandler;
    private object? m_onPointerPressedEventHandler;
    private object? m_onPointerReleasedEventHandler;
    private object? m_onVerticalScrollControllerPointerEnteredHandler;
    private object? m_onVerticalScrollControllerPointerExitedHandler;

    /// <summary>
    /// Set to True to favor mouse indicators over panning indicators for the scroll controllers.
    /// </summary>
    private bool m_preferMouseIndicators = false;

    private UIElement? m_scrollControllersSeparatorElement;

    private long m_scrollPresenterComputedHorizontalScrollModeChangedToken;

    private long m_scrollPresenterComputedVerticalScrollModeChangedToken;

    /// <summary>
    /// Set to True when the mouse scrolling indicators are currently showing.
    /// </summary>
    private bool m_showingMouseIndicators = false;

    private int m_verticalAddScrollVelocityDirection;

    private int m_verticalAddScrollVelocityOffsetChangeCorrelationId = -1;

    private ScrollBarController? m_verticalScrollBarController;

    private IScrollController? m_verticalScrollController;

    private UIElement? m_verticalScrollControllerElement;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollView"/> class.
    /// </summary>
    public ScrollView()
    {
        m_autoHideScrollBarsState = MakeAutoHideScrollBarsState();

        DefaultStyleKey = typeof(ScrollView);
        HookUISettingsEvent();
        HookScrollViewEvents();
    }

    /// <summary>
    /// Occurs when the <see cref="ScrollView"/> is about to select an anchor element.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingAnchorRequestedEventArgs>? AnchorRequested;

    /// <summary>
    /// Occurs at the beginning of a bring-into-view request participation. Allows customization of that participation.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingBringingIntoViewEventArgs>? BringingIntoView;

    /// <summary>
    /// Occurs when either the <see cref="ExtentWidth"/> or <see cref="ExtentHeight"/> properties has changed.
    /// </summary>
    public event TypedEventHandler<ScrollView, object?>? ExtentChanged;

    /// <summary>
    /// Occurs when a call to <see cref="ScrollTo"/> or <see cref="ScrollBy"/> triggers an animation.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingScrollAnimationStartingEventArgs>? ScrollAnimationStarting;

    /// <summary>
    /// Occurs when a <see cref="ScrollTo"/>, <see cref="ScrollBy"/>, or <see cref="AddScrollVelocity"/> asynchronous operation ends. Provides the original correlation ID.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingScrollCompletedEventArgs>? ScrollCompleted;

    /// <summary>
    /// Occurs when the current interaction state of the control has changed.
    /// </summary>
    public event TypedEventHandler<ScrollView, object>? StateChanged;

    /// <summary>
    /// Occurs when manipulations such as scrolling and zooming have caused the view to change.
    /// </summary>
    public event TypedEventHandler<ScrollView, object>? ViewChanged;

    /// <summary>
    /// Occurs when a call to <see cref="ZoomTo"/> or <see cref="ZoomBy"/> triggers an animation.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingZoomAnimationStartingEventArgs>? ZoomAnimationStarting;

    /// <summary>
    /// Occurs when a <see cref="ZoomTo"/>, <see cref="ZoomBy"/>, or <see cref="AddZoomVelocity"/> asynchronous operation ends. Provides the original correlation ID.
    /// </summary>
    public event TypedEventHandler<ScrollView, ScrollingZoomCompletedEventArgs>? ZoomCompleted;

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
        get => (double)GetValue(VerticalAnchorRatioProperty);
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
        get => (ScrollingScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain vertical scrolling to an outer scroll control.
    /// </summary>
    public ScrollingChainMode VerticalScrollChainMode
    {
        get => (ScrollingChainMode)GetValue(VerticalScrollChainModeProperty);
        set => SetValue(VerticalScrollChainModeProperty, value);
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
        get => (ScrollingRailMode)GetValue(VerticalScrollRailModeProperty);
        set => SetValue(VerticalScrollRailModeProperty, value);
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
    /// Registers a <see cref="UIElement"/> as a potential scroll anchor.
    /// </summary>
    /// <param name="element">A <see cref="UIElement"/> within the subtree of the <see cref="ScrollView"/>.</param>
    public void RegisterAnchorCandidate(UIElement element)
    {
        if (_scrollPresenter is { } scrollPresenter)
        {
            if (scrollPresenter is IScrollAnchorProvider scrollPresenterAsAnchorProvider)
            {
                scrollPresenterAsAnchorProvider.RegisterAnchorCandidate(element);
                return;
            }

            throw new InvalidOperationException(s_IScrollAnchorProviderNotImpl);
        }

        throw new InvalidOperationException(s_noScrollPresenterPart);
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
    /// Unregisters a <see cref="UIElement"/> as a potential scroll anchor.
    /// </summary>
    /// <param name="element">A <see cref="UIElement"/> within the subtree of the <see cref="ScrollView"/>.</param>
    public void UnregisterAnchorCandidate(UIElement element)
    {
        if (_scrollPresenter is { } scrollPresenter)
        {
            if (scrollPresenter is IScrollAnchorProvider scrollPresenterAsAnchorProvider)
            {
                scrollPresenterAsAnchorProvider.UnregisterAnchorCandidate(element);
                return;
            }

            throw new InvalidOperationException(s_IScrollAnchorProviderNotImpl);
        }

        throw new InvalidOperationException(s_noScrollPresenterPart);
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
        ScrollBar? horizontalScrollBar = null;

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
        ScrollBar? verticalScrollBar = null;

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

    private static void OnComputedHorizontalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnComputedVerticalScrollBarVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnComputedVerticalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnContentOrientationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalAnchorRatioPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollView)sender;

        var value = (double)args.NewValue;
        ValidateAnchorRatio(value);
        if (double.IsNaN(value))
        {
            sender.SetValue(args.Property, 0d);
            return;
        }

        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalScrollBarVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalScrollChainModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalScrollRailModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnIgnoredInputKindsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnMaxZoomFactorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollView)sender;

        var value = (double)args.NewValue;
        ValidateZoomFactoryBoundary(value);
        if (double.IsNaN(value))
        {
            sender.SetValue(args.Property, 10d);
            return;
        }

        owner.OnPropertyChanged(args);
    }

    private static void OnMinZoomFactorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollView)sender;

        var value = (double)args.NewValue;
        ValidateZoomFactoryBoundary(value);
        if (double.IsNaN(value))
        {
            sender.SetValue(args.Property, 0.1d);
            return;
        }

        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalAnchorRatioPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollView)sender;

        var value = (double)args.NewValue;
        ValidateAnchorRatio(value);
        if (double.IsNaN(value))
        {
            sender.SetValue(args.Property, 0d);
            return;
        }

        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalScrollBarVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalScrollChainModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalScrollRailModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnZoomChainModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnZoomModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ScrollView owner = (ScrollView)sender;
        owner.OnPropertyChanged(args);
    }

    private static void ValidateAnchorRatio(double value)
    {
        ScrollPresenter.ValidateAnchorRatio(value);
    }

    private static void ValidateZoomFactoryBoundary(double value)
    {
        ScrollPresenter.ValidateZoomFactoryBoundary(value);
    }

    private bool AreAllScrollControllersCollapsed()
    {
        return !SharedHelpers.IsAncestor(m_horizontalScrollControllerElement /*child*/, this /*parent*/, true /*checkVisibility*/) &&
            !SharedHelpers.IsAncestor(m_verticalScrollControllerElement as DependencyObject /*child*/, this /*parent*/, true /*checkVisibility*/);
    }

    private bool AreBothScrollControllersVisible()
    {
        return SharedHelpers.IsAncestor(m_horizontalScrollControllerElement /*child*/, this /*parent*/, true /*checkVisibility*/) &&
        SharedHelpers.IsAncestor(m_verticalScrollControllerElement as DependencyObject /*child*/, this /*parent*/, true /*checkVisibility*/);
    }

    private bool AreScrollControllersAutoHiding()
    {
        // Use the cached value unless it was invalidated.
        if (m_autoHideScrollControllersValid)
        {
            return m_autoHideScrollControllers;
        }

        m_autoHideScrollControllersValid = true;

        if (ScrollViewTestHooks.GetGlobalTestHooks() is { } globalTestHooks)
        {
            var autoHideScrollControllers = ScrollViewTestHooks.GetAutoHideScrollControllers(this);

            if (autoHideScrollControllers is true)
            {
                // Test hook takes precedence over UISettings and registry key settings.
                m_autoHideScrollControllers = autoHideScrollControllers.Value;
                return m_autoHideScrollControllers;
            }
        }

        if (m_autoHideScrollBarsState.m_uiSettings5 is not null)
        {
            m_autoHideScrollControllers = m_autoHideScrollBarsState.m_uiSettings5.AutoHideScrollBars;
        }
        else
        {
            m_autoHideScrollControllers = RegUtil.UseDynamicScrollbars();
        }

        return m_autoHideScrollControllers;
    }

    private bool CanScrollDown()
    {
        return CanScrollVerticallyInDirection(inPositiveDirection: true);
    }

    private bool CanScrollHorizontallyInDirection(bool inPositiveDirection)
    {
        bool canScrollInDirection = false;

        if (FlowDirection == FlowDirection.RightToLeft)
        {
            inPositiveDirection = !inPositiveDirection;
        }

        if (_scrollPresenter is not null)
        {
            var scrollPresenter = _scrollPresenter;
            var horizontalScrollMode = ComputedHorizontalScrollMode;

            if (horizontalScrollMode == ScrollingScrollMode.Enabled)
            {
                var zoomedExtentWidth = scrollPresenter.ExtentWidth * scrollPresenter.ZoomFactor;
                var viewportWidth = scrollPresenter.ActualWidth;
                if (zoomedExtentWidth > viewportWidth)
                {
                    // Ignore distance to an edge smaller than 1/1000th of a pixel to account for rounding approximations.
                    // Otherwise a Left/Right arrow key may be processed and have no effect.
                    const double offsetEpsilon = 0.001;

                    if (inPositiveDirection)
                    {
                        var maxHorizontalOffset = zoomedExtentWidth - viewportWidth;
                        if (scrollPresenter.HorizontalOffset < maxHorizontalOffset - offsetEpsilon)
                        {
                            canScrollInDirection = true;
                        }
                    }
                    else
                    {
                        if (scrollPresenter.HorizontalOffset > offsetEpsilon)
                        {
                            canScrollInDirection = true;
                        }
                    }
                }
            }
        }

        return canScrollInDirection;
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
        bool canScrollInDirection = false;
        if (_scrollPresenter is not null)
        {
            var scrollPresenter = _scrollPresenter as ScrollPresenter;
            var verticalScrollMode = ComputedVerticalScrollMode;

            if (verticalScrollMode == ScrollingScrollMode.Enabled)
            {
                var zoomedExtentHeight = scrollPresenter.ExtentHeight * scrollPresenter.ZoomFactor;
                var viewportHeight = scrollPresenter.ActualHeight;
                if (zoomedExtentHeight > viewportHeight)
                {
                    // Ignore distance to an edge smaller than 1/1000th of a pixel to account for rounding approximations.
                    // Otherwise an Up/Down arrow key may be processed and have no effect.
                    const double offsetEpsilon = 0.001;

                    if (inPositiveDirection)
                    {
                        var maxVerticalOffset = zoomedExtentHeight - viewportHeight;
                        if (scrollPresenter.VerticalOffset < maxVerticalOffset - offsetEpsilon)
                        {
                            canScrollInDirection = true;
                        }
                    }
                    else
                    {
                        if (scrollPresenter.VerticalOffset > offsetEpsilon)
                        {
                            canScrollInDirection = true;
                        }
                    }
                }
            }
        }

        return canScrollInDirection;
    }

    private void DoScroll(double offset, Orientation orientation)
    {
        bool isVertical = orientation == Orientation.Vertical;

        if (_scrollPresenter is ScrollPresenter scrollPresenter)
        {
            if (SharedHelpers.IsAnimationsEnabled())
            {
                var inertiaDecayRate = new Vector2(0.9995f, 0.9995f);

                // A velocity less than or equal to this value has no effect.
                const double minVelocity = 30.0;

                // We need to add this much velocity over minVelocity per pixel we want to move:
                const double s_velocityNeededPerPixel = 7.600855902349023;

                var scrollDir = offset > 0 ? 1 : -1;

                // The minimum velocity required to move in the given direction.
                double baselineVelocity = minVelocity * scrollDir;

                // If there is already a scroll animation running for a previous key press, we want to take that into account
                // for calculating the baseline velocity.
                var previousScrollViewChangeCorrelationId = isVertical ? m_verticalAddScrollVelocityOffsetChangeCorrelationId : m_horizontalAddScrollVelocityOffsetChangeCorrelationId;
                if (previousScrollViewChangeCorrelationId != s_noOpCorrelationId)
                {
                    var directionOfPreviousScrollOperation = isVertical ? m_verticalAddScrollVelocityDirection : m_horizontalAddScrollVelocityDirection;
                    if (directionOfPreviousScrollOperation == 1)
                    {
                        baselineVelocity -= minVelocity;
                    }
                    else if (directionOfPreviousScrollOperation == -1)
                    {
                        baselineVelocity += minVelocity;
                    }
                }

                var velocity = (float)(baselineVelocity + (offset * s_velocityNeededPerPixel));

                if (isVertical)
                {
                    Vector2 offsetsVelocity = new(0.0f, velocity);
                    m_verticalAddScrollVelocityOffsetChangeCorrelationId = scrollPresenter.AddScrollVelocity(offsetsVelocity, inertiaDecayRate);
                    m_verticalAddScrollVelocityDirection = scrollDir;
                }
                else
                {
                    Vector2 offsetsVelocity = new(velocity, 0.0f);
                    m_horizontalAddScrollVelocityOffsetChangeCorrelationId = scrollPresenter.AddScrollVelocity(offsetsVelocity, inertiaDecayRate);
                    m_horizontalAddScrollVelocityDirection = scrollDir;
                }
            }
            else
            {
                if (isVertical)
                {
                    // Any horizontal AddScrollVelocity animation recently launched should be ignored by a potential subsequent AddScrollVelocity call.
                    m_verticalAddScrollVelocityOffsetChangeCorrelationId = s_noOpCorrelationId;

                    scrollPresenter.ScrollBy(0.0 /*horizontalOffsetDelta*/, offset /*verticalOffsetDelta*/);
                }
                else
                {
                    // Any vertical AddScrollVelocity animation recently launched should be ignored by a potential subsequent AddScrollVelocity call.
                    m_horizontalAddScrollVelocityOffsetChangeCorrelationId = s_noOpCorrelationId;

                    scrollPresenter.ScrollBy(offset /*horizontalOffsetDelta*/, 0.0 /*verticalOffsetDelta*/);
                }
            }
        }
    }

    private bool DoScrollForKey(VirtualKey key, double scrollProportion)
    {
        Debug.Assert(_scrollPresenter != null);

        const double offsetEpsilon = 0.001;
        bool isScrollTriggered = false;
        var scrollPresenter = _scrollPresenter as ScrollPresenter;

        if ((key == VirtualKey.PageDown || key == VirtualKey.Down) && CanScrollDown())
        {
            Debug.Assert(scrollPresenter.VerticalOffset < ScrollableHeight);

            // When getting close to the maximum vertical offset:
            //  - make sure the maximum is actually reached thanks for the epsilon addition.
            var maxScrollAmount = ScrollableHeight + offsetEpsilon - scrollPresenter.VerticalOffset;
            //  - do not automatically overbounce by limiting the offset change to the remaining scrollable height.
            var scrollAmount = Math.Min(maxScrollAmount, scrollPresenter.ActualHeight * (key == VirtualKey.PageDown ? 1.0 : scrollProportion));

            isScrollTriggered = true;
            DoScroll(scrollAmount, Orientation.Vertical);
        }
        else if ((key == VirtualKey.PageUp || key == VirtualKey.Up) && CanScrollUp())
        {
            Debug.Assert(scrollPresenter.VerticalOffset > 0);

            // When getting close to the minimum vertical offset 0.0:
            //  - make sure 0.0 is actually reached thanks for the epsilon addition.
            var maxScrollAmount = scrollPresenter.VerticalOffset + offsetEpsilon;
            //  - do not automatically overbounce by limiting the offset change to the remaining offset.
            var scrollAmount = Math.Max(-maxScrollAmount, scrollPresenter.ActualHeight * (key == VirtualKey.PageUp ? -1.0 : -scrollProportion));

            isScrollTriggered = true;
            DoScroll(scrollAmount, Orientation.Vertical);
        }
        else if (key == VirtualKey.Left || key == VirtualKey.Right)
        {
            double scrollAmount = scrollPresenter.ActualWidth * scrollProportion;
            bool isRTL = FlowDirection == FlowDirection.RightToLeft;

            if (isRTL)
            {
                scrollAmount *= -1;
            }

            if (key == VirtualKey.Right && CanScrollRight())
            {
                // When getting close to the maximum horizontal offset:
                //  - make sure the maximum is actually reached thanks for the epsilon addition.
                var maxScrollAmount = isRTL ?
                    -scrollPresenter.HorizontalOffset - offsetEpsilon :
                    ScrollableWidth + offsetEpsilon - scrollPresenter.HorizontalOffset;
                //  - do not automatically overbounce by limiting the offset change to the remaining scrollable width.
                scrollAmount = isRTL ?
                    Math.Max(maxScrollAmount, scrollAmount) :
                    Math.Min(maxScrollAmount, scrollAmount);

                Debug.Assert(scrollAmount != 0.0);

                isScrollTriggered = true;
                DoScroll(scrollAmount, Orientation.Horizontal);
            }
            else if (key == VirtualKey.Left && CanScrollLeft())
            {
                // When getting close to the minimum horizontal offset 0.0:
                //  - make sure 0.0 is actually reached thanks for the epsilon addition.
                var maxScrollAmount = isRTL ?
                    -ScrollableWidth - offsetEpsilon + scrollPresenter.HorizontalOffset :
                    scrollPresenter.HorizontalOffset + offsetEpsilon;
                //  - do not automatically overbounce by limiting the offset change to the remaining offset.
                scrollAmount =
                    isRTL ?
                    Math.Min(-maxScrollAmount, -scrollAmount) :
                    Math.Max(-maxScrollAmount, -scrollAmount);

                Debug.Assert(scrollAmount != 0.0);

                isScrollTriggered = true;
                DoScroll(scrollAmount, Orientation.Horizontal);
            }
        }
        else if (key == VirtualKey.Home)
        {
            bool canScrollUp = CanScrollUp();
            var verticalScrollMode = ComputedVerticalScrollMode;

            if (canScrollUp || (verticalScrollMode == ScrollingScrollMode.Disabled && CanScrollLeft()))
            {
                isScrollTriggered = true;
                var horizontalOffset = canScrollUp ? scrollPresenter.HorizontalOffset : 0.0;
                var verticalOffset = canScrollUp ? 0.0 : scrollPresenter.VerticalOffset;

                if (!canScrollUp && FlowDirection == FlowDirection.RightToLeft)
                {
                    horizontalOffset = scrollPresenter.ExtentWidth * scrollPresenter.ZoomFactor - scrollPresenter.ActualWidth;
                }

                scrollPresenter.ScrollTo(horizontalOffset, verticalOffset);
            }
        }
        else if (key == VirtualKey.End)
        {
            bool canScrollDown = CanScrollDown();
            var verticalScrollMode = ComputedVerticalScrollMode;

            if (canScrollDown || (verticalScrollMode == ScrollingScrollMode.Disabled && CanScrollRight()))
            {
                isScrollTriggered = true;
                var zoomedExtent = (canScrollDown ? scrollPresenter.ExtentHeight : scrollPresenter.ExtentWidth) * scrollPresenter.ZoomFactor;
                var horizontalOffset = canScrollDown ? scrollPresenter.HorizontalOffset : zoomedExtent - scrollPresenter.ActualWidth;
                var verticalOffset = canScrollDown ? zoomedExtent - scrollPresenter.ActualHeight : scrollPresenter.VerticalOffset;

                if (!canScrollDown && FlowDirection == FlowDirection.RightToLeft)
                {
                    horizontalOffset = 0.0;
                }

                scrollPresenter.ScrollTo(horizontalOffset, verticalOffset);
            }
        }

        return isScrollTriggered;
    }

    private DependencyObject GetNextFocusCandidate(FocusNavigationDirection navigationDirection, bool isPageNavigation)
    {
        Debug.Assert(_scrollPresenter != null);
        Debug.Assert(navigationDirection != FocusNavigationDirection.None);
        ScrollPresenter scrollPresenter = _scrollPresenter as ScrollPresenter;

        FocusNavigationDirection focusDirection = navigationDirection;

        FindNextElementOptions findNextElementOptions = new();
        findNextElementOptions.SearchRoot = scrollPresenter.Content;

        if (isPageNavigation)
        {
            Rect localBounds = new Rect(0, 0, (float)scrollPresenter.ActualWidth, (float)scrollPresenter.ActualHeight);
            var globalBounds = scrollPresenter.TransformToVisual(null).TransformBounds(localBounds);
            const int numPagesLookAhead = 2;

            var hintRect = globalBounds;
            switch (navigationDirection)
            {
                case FocusNavigationDirection.Down:
                    hintRect.Y += globalBounds.Height * numPagesLookAhead;
                    break;

                case FocusNavigationDirection.Up:
                    hintRect.Y -= globalBounds.Height * numPagesLookAhead;
                    break;

                case FocusNavigationDirection.Left:
                    hintRect.X -= globalBounds.Width * numPagesLookAhead;
                    break;

                case FocusNavigationDirection.Right:
                    hintRect.X += globalBounds.Width * numPagesLookAhead;
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            findNextElementOptions.HintRect = hintRect;
            findNextElementOptions.ExclusionRect = hintRect;
            focusDirection = FocusHelper.GetOppositeDirection(navigationDirection);
        }

        return FocusManager.FindNextElement(focusDirection, findNextElementOptions);
    }

    private void GoToState(string stateName, bool useTransitions = true)
    {
        VisualStateManager.GoToState(this, stateName, useTransitions);
    }

    private void HandleKeyDownForStandardScroll(KeyRoutedEventArgs args)
    {
        // Up/Down/Left/Right will scroll by 15% the size of the viewport.
        const double smallScrollProportion = 0.15;

        Debug.Assert(!args.Handled);
        Debug.Assert(_scrollPresenter != null);

        bool isHandled = DoScrollForKey(args.Key, smallScrollProportion);

        args.Handled = isHandled;
    }

    private void HandleKeyDownForXYNavigation(KeyRoutedEventArgs args)
    {
        Debug.Assert(!args.Handled);
        Debug.Assert(_scrollPresenter != null);

        bool isHandled = false;
        var originalKey = args.OriginalKey;
        var scrollPresenter = _scrollPresenter as ScrollPresenter;
        bool isPageNavigation = FocusHelper.IsGamepadPageNavigationDirection(originalKey);
        double scrollAmountProportion = isPageNavigation ? 1.0 : 0.5;
        bool shouldProcessKeyEvent = true;
        FocusNavigationDirection navigationDirection;

        if (isPageNavigation)
        {
            navigationDirection = FocusHelper.GetPageNavigationDirection(originalKey);

            // We should only handle page navigation if we can scroll in that direction.
            // Note: For non-paging navigation, we might want to move focus even if we cannot scroll.
            shouldProcessKeyEvent = CanScrollInDirection(navigationDirection);
        }
        else
        {
            navigationDirection = FocusHelper.GetNavigationDirection(originalKey);
        }

        if (shouldProcessKeyEvent)
        {
            bool shouldScroll = false;
            bool shouldMoveFocus = false;
            DependencyObject? nextElement = null;

            if (navigationDirection != FocusNavigationDirection.None)
            {
                nextElement = GetNextFocusCandidate(navigationDirection, isPageNavigation);
            }

            if (nextElement is not null && nextElement != FocusManager.GetFocusedElement(XamlRoot))
            {
                UIElement nextElementAsUIE = FocusHelper.GetUIElementForFocusCandidate(nextElement);
                Debug.Assert(nextElementAsUIE != null);

                var nextElementAsFe = nextElementAsUIE as FrameworkElement;
                var rect = new Rect(0, 0, (float)nextElementAsFe.ActualWidth, (float)nextElementAsFe.ActualHeight);
                var elementBounds = nextElementAsUIE.TransformToVisual(scrollPresenter).TransformBounds(rect);
                var viewport = new Rect(0, 0, (float)scrollPresenter.ActualWidth, (float)scrollPresenter.ActualHeight);

                // Extend the viewport in the direction we are moving:
                Rect extendedViewport = viewport;
                switch (navigationDirection)
                {
                    case FocusNavigationDirection.Down:
                        extendedViewport.Height += viewport.Height;
                        break;

                    case FocusNavigationDirection.Up:
                        extendedViewport.Y -= viewport.Height;
                        extendedViewport.Height += viewport.Height;
                        break;

                    case FocusNavigationDirection.Left:
                        extendedViewport.X -= viewport.Width;
                        extendedViewport.Width += viewport.Width;
                        break;

                    case FocusNavigationDirection.Right:
                        extendedViewport.Width += viewport.Width;
                        break;
                }

                bool isElementInExtendedViewport = RectHelper.Intersect(elementBounds, extendedViewport) != RectHelper.Empty;
                bool isElementFullyInExtendedViewport = RectHelper.Union(elementBounds, extendedViewport) == extendedViewport;

                if (isElementInExtendedViewport)
                {
                    if (isPageNavigation)
                    {
                        // Always scroll for page navigation
                        shouldScroll = true;

                        if (isElementFullyInExtendedViewport)
                        {
                            // Move focus:
                            shouldMoveFocus = true;
                        }
                    }
                    else
                    {
                        // Non-paging scroll allows partial candidates
                        shouldMoveFocus = true;
                    }
                }
                else
                {
                    // Element is outside extended viewport - scroll but don't focus.
                    shouldScroll = true;
                }
            }
            else
            {
                // No focus candidate: scroll
                shouldScroll = true;
            }

            if (shouldMoveFocus)
            {
                var focusAsyncOperation = FocusManager.TryFocusAsync(nextElement, FocusState.Keyboard);

                if (SharedHelpers.IsAnimationsEnabled()) // When system animations are turned off, the bring-into-view operations are not turned into animations.
                {
                    // By changing focus, we will trigger BringIntoView requests in ScrollPresenter. If we are not going to invoke a scroll below (i.e. shouldScroll = false)
                    // we should allow ScrollPresenter to animate any BringIntoView requests.
                    // If we ARE going to invoke a scroll below (i.e. shouldScroll = true) we want to prevent the case where both our scroll and the scroll triggered by the
                    // focus change are active at once. So in this case we want to cancel any BringIntoView operations, since we are already handling the scrolling.
                    bool cancelBringIntoView = shouldScroll;

                    var strongThis = this;
                    var targetElement = nextElement as UIElement;
                    focusAsyncOperation.Completed = new AsyncOperationCompletedHandler<FocusMovementResult>((asyncOperation, asyncStatus) =>
                    {
                        if (asyncStatus == AsyncStatus.Completed && asyncOperation.GetResults() is not null)
                        {
                            var bringIntoViewOperation = new ScrollViewBringIntoViewOperation(targetElement, cancelBringIntoView);

                            strongThis.m_bringIntoViewOperations.Add(bringIntoViewOperation);
                            strongThis.HookCompositionTargetRendering();
                        }
                    });
                }

                isHandled = true;
            }

            if (shouldScroll)
            {
                if (navigationDirection == FocusNavigationDirection.None)
                {
                    isHandled = DoScrollForKey(args.Key, scrollAmountProportion);
                }
                else
                {
                    if (navigationDirection == FocusNavigationDirection.Down && CanScrollDown())
                    {
                        isHandled = true;
                        DoScroll(scrollPresenter.ActualHeight * scrollAmountProportion, Orientation.Vertical);
                    }
                    else if (navigationDirection == FocusNavigationDirection.Up && CanScrollUp())
                    {
                        isHandled = true;
                        DoScroll(-scrollPresenter.ActualHeight * scrollAmountProportion, Orientation.Vertical);
                    }
                    else if (navigationDirection == FocusNavigationDirection.Right && CanScrollRight())
                    {
                        isHandled = true;
                        DoScroll(scrollPresenter.ActualWidth * scrollAmountProportion * (FlowDirection == FlowDirection.RightToLeft ? -1 : 1), Orientation.Horizontal);
                    }
                    else if (navigationDirection == FocusNavigationDirection.Left && CanScrollLeft())
                    {
                        isHandled = true;
                        DoScroll(-scrollPresenter.ActualWidth * scrollAmountProportion * (FlowDirection == FlowDirection.RightToLeft ? -1 : 1), Orientation.Horizontal);
                    }
                }
            }
        }

        args.Handled = isHandled;
    }

    private void HandleScrollControllerPointerEntered(bool isForHorizontalScrollController)
    {
        if (isForHorizontalScrollController)
        {
            m_isPointerOverHorizontalScrollController = true;
        }
        else
        {
            m_isPointerOverVerticalScrollController = true;
        }

        UpdateScrollControllersAutoHiding();
        if (AreScrollControllersAutoHiding() && !SharedHelpers.IsAnimationsEnabled())
        {
            HideIndicatorsAfterDelay();
        }
    }

    private void HandleScrollControllerPointerExited(bool isForHorizontalScrollController)
    {
        if (isForHorizontalScrollController)
        {
            m_isPointerOverHorizontalScrollController = false;
        }
        else
        {
            m_isPointerOverVerticalScrollController = false;
        }

        UpdateScrollControllersAutoHiding();
        if (AreScrollControllersAutoHiding())
        {
            HideIndicatorsAfterDelay();
        }
    }

    private void HideIndicators(bool useTransitions = true)
    {
        Debug.Assert(AreScrollControllersAutoHiding());

        if (!AreAllScrollControllersCollapsed() && !m_keepIndicatorsShowing)
        {
            GoToState(s_noIndicatorStateName, useTransitions);

            if (!m_hasNoIndicatorStateStoryboardCompletedHandler)
            {
                m_showingMouseIndicators = false;
            }
        }
    }

    private void HideIndicatorsAfterDelay()
    {
        Debug.Assert(AreScrollControllersAutoHiding());

        if (!m_keepIndicatorsShowing && IsLoaded)
        {
            DispatcherTimer? hideIndicatorsTimer = null;

            if (m_hideIndicatorsTimer is not null)
            {
                hideIndicatorsTimer = m_hideIndicatorsTimer;
                if (hideIndicatorsTimer.IsEnabled)
                {
                    hideIndicatorsTimer.Stop();
                }
            }
            else
            {
                hideIndicatorsTimer = new DispatcherTimer();
                hideIndicatorsTimer.Interval = TimeSpan.FromTicks(s_noIndicatorCountdown);
                hideIndicatorsTimer.Tick += OnHideIndicatorsTimerTick;
                m_hideIndicatorsTimer = hideIndicatorsTimer;
            }

            hideIndicatorsTimer.Start();
        }
    }

    private void HookCompositionTargetRendering()
    {
        Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
    }

    private void HookHorizontalScrollControllerEvents()
    {
        Debug.Assert(m_onHorizontalScrollControllerPointerEnteredHandler is null);
        Debug.Assert(m_onHorizontalScrollControllerPointerExitedHandler is null);

        if (m_horizontalScrollController is IScrollController horizontalScrollController)
        {
            horizontalScrollController.CanScrollChanged += OnScrollControllerCanScrollChanged;

            horizontalScrollController.IsScrollingWithMouseChanged += OnScrollControllerIsScrollingWithMouseChanged;
        }

        if (m_horizontalScrollControllerElement is UIElement horizontalScrollControllerElement)
        {
            m_onHorizontalScrollControllerPointerEnteredHandler = new PointerEventHandler(OnHorizontalScrollControllerPointerEntered);
            horizontalScrollControllerElement.AddHandler(UIElement.PointerEnteredEvent, m_onHorizontalScrollControllerPointerEnteredHandler, true);

            m_onHorizontalScrollControllerPointerExitedHandler = new PointerEventHandler(OnHorizontalScrollControllerPointerExited);
            horizontalScrollControllerElement.AddHandler(UIElement.PointerExitedEvent, m_onHorizontalScrollControllerPointerExitedHandler, true);
        }
    }

    private void HookScrollPresenterEvents()
    {
        Debug.Assert(m_scrollPresenterComputedHorizontalScrollModeChangedToken == 0);
        Debug.Assert(m_scrollPresenterComputedVerticalScrollModeChangedToken == 0);

        if (_scrollPresenter is { } scrollPresenter)
        {
            scrollPresenter.ExtentChanged += OnScrollPresenterExtentChanged;
            scrollPresenter.StateChanged += OnScrollPresenterStateChanged;
            scrollPresenter.ScrollAnimationStarting += OnScrollAnimationStarting;
            scrollPresenter.ZoomAnimationStarting += OnZoomAnimationStarting;
            scrollPresenter.ViewChanged += OnScrollPresenterViewChanged;
            scrollPresenter.ScrollCompleted += OnScrollPresenterScrollCompleted;
            scrollPresenter.ZoomCompleted += OnScrollPresenterZoomCompleted;
            scrollPresenter.BringingIntoView += OnScrollPresenterBringingIntoView;
            scrollPresenter.AnchorRequested += OnScrollPresenterAnchorRequested;

            DependencyObject scrollPresenterAsDO = scrollPresenter as DependencyObject;

            m_scrollPresenterComputedHorizontalScrollModeChangedToken = scrollPresenterAsDO.RegisterPropertyChangedCallback(
                ScrollPresenter.ComputedHorizontalScrollModeProperty, OnScrollPresenterPropertyChanged);

            m_scrollPresenterComputedVerticalScrollModeChangedToken = scrollPresenterAsDO.RegisterPropertyChangedCallback(
                ScrollPresenter.ComputedVerticalScrollModeProperty, OnScrollPresenterPropertyChanged);
        }
    }

    private void HookScrollViewEvents()
    {
        Debug.Assert(m_onPointerEnteredEventHandler is null);
        Debug.Assert(m_onPointerMovedEventHandler is null);
        Debug.Assert(m_onPointerExitedEventHandler is null);
        Debug.Assert(m_onPointerPressedEventHandler is null);
        Debug.Assert(m_onPointerReleasedEventHandler is null);
        Debug.Assert(m_onPointerCanceledEventHandler is null);

        GettingFocus += OnScrollViewGettingFocus;
        IsEnabledChanged += OnScrollViewIsEnabledChanged;
        Unloaded += OnScrollViewUnloaded;

        m_onPointerEnteredEventHandler = new PointerEventHandler(OnScrollViewPointerEntered);
        AddHandler(UIElement.PointerEnteredEvent, m_onPointerEnteredEventHandler, false);

        m_onPointerMovedEventHandler = new PointerEventHandler(OnScrollViewPointerMoved);
        AddHandler(UIElement.PointerMovedEvent, m_onPointerMovedEventHandler, false);

        m_onPointerExitedEventHandler = new PointerEventHandler(OnScrollViewPointerExited);
        AddHandler(UIElement.PointerExitedEvent, m_onPointerExitedEventHandler, false);

        m_onPointerPressedEventHandler = new PointerEventHandler(OnScrollViewPointerPressed);
        AddHandler(UIElement.PointerPressedEvent, m_onPointerPressedEventHandler, false);

        m_onPointerReleasedEventHandler = new PointerEventHandler(OnScrollViewPointerReleased);
        AddHandler(UIElement.PointerReleasedEvent, m_onPointerReleasedEventHandler, true);

        m_onPointerCanceledEventHandler = new PointerEventHandler(OnScrollViewPointerCanceled);
        AddHandler(UIElement.PointerCanceledEvent, m_onPointerCanceledEventHandler, true);
    }

    private void HookUISettingsEvent()
    {
        m_autoHideScrollBarsState.HookUISettingsEvent(this);
    }

    private void HookVerticalScrollControllerEvents()
    {
        Debug.Assert(m_onVerticalScrollControllerPointerEnteredHandler is null);
        Debug.Assert(m_onVerticalScrollControllerPointerExitedHandler is null);

        if (m_verticalScrollController is IScrollController verticalScrollController)
        {
            verticalScrollController.CanScrollChanged += OnScrollControllerCanScrollChanged;

            verticalScrollController.IsScrollingWithMouseChanged += OnScrollControllerIsScrollingWithMouseChanged;
        }

        if (m_verticalScrollControllerElement is UIElement verticalScrollControllerElement)
        {
            m_onVerticalScrollControllerPointerEnteredHandler = new PointerEventHandler(OnVerticalScrollControllerPointerEntered);
            verticalScrollControllerElement.AddHandler(UIElement.PointerEnteredEvent, m_onVerticalScrollControllerPointerEnteredHandler, true);

            m_onVerticalScrollControllerPointerExitedHandler = new PointerEventHandler(OnVerticalScrollControllerPointerExited);
            verticalScrollControllerElement.AddHandler(UIElement.PointerExitedEvent, m_onVerticalScrollControllerPointerExitedHandler, true);
        }
    }

    private bool IsInputKindIgnored(ScrollingInputKinds inputKind)
    {
        return (IgnoredInputKinds & inputKind) == inputKind;
    }

    private bool IsScrollControllersSeparatorVisible()
    {
        return m_scrollControllersSeparatorElement is not null && m_scrollControllersSeparatorElement.Visibility == Visibility.Visible;
    }

    private AutoHideScrollBarsState MakeAutoHideScrollBarsState()
    {
        return new AutoHideScrollBarsState();
    }

    private void OnAutoHideScrollBarsChanged(UISettings uiSettings, UISettingsAutoHideScrollBarsChangedEventArgs args)
    {
        // OnAutoHideScrollBarsChanged is called on a non-UI thread, process notification on the UI thread using a dispatcher.
        m_dispatcherQueue.TryEnqueue(() =>
        {
            this.m_autoHideScrollControllersValid = false;
            this.UpdateVisualStates(
                true  /*useTransitions*/,
                false /*showIndicators*/,
                false /*hideIndicators*/,
                true  /*scrollControllersAutoHidingChanged*/);
        });
    }

    private void OnCompositionTargetRendering(object sender, object args)
    {
        if (m_bringIntoViewOperations.Count > 0)
        {
            for (var i = 0; i < m_bringIntoViewOperations.Count; i++)
            {
                var bringIntoViewOperation = m_bringIntoViewOperations[i];

                if (bringIntoViewOperation.HasMaxTicksCount)
                {
                    // This ScrollView is no longer expected to receive BringingIntoView notifications from its ScrollPresenter,
                    // resulting from a FocusManager::TryFocusAsync call in ScrollView::HandleKeyDownForXYNavigation.
                    m_bringIntoViewOperations.Remove(bringIntoViewOperation);
                    i--;
                }
                else
                {
                    // Increment the number of ticks ellapsed since the FocusManager::TryFocusAsync call, and continue to wait for BringingIntoView notifications.
                    bringIntoViewOperation.TickOperation();
                }
            }
        }

        if (m_bringIntoViewOperations.Count <= 0)
        {
            UnhookCompositionTargetRendering();
        }
    }

    private void OnHideIndicatorsTimerTick(object sender, object args)
    {
        ResetHideIndicatorsTimer();

        if (AreScrollControllersAutoHiding())
        {
            HideIndicators();
        }
    }

    private void OnHorizontalScrollControllerPointerEntered(object sender, PointerRoutedEventArgs args)
    {
        HandleScrollControllerPointerEntered(true /*isForHorizontalScrollController*/);
    }

    private void OnHorizontalScrollControllerPointerExited(object sender, PointerRoutedEventArgs args)
    {
        HandleScrollControllerPointerExited(true /*isForHorizontalScrollController*/);
    }

    private void OnIndicatorStateStoryboardCompleted(
         object sender,
         object args)
    {
        // If the cursor is currently directly over either scroll controller then do not automatically hide the indicators
        if (AreScrollControllersAutoHiding() &&
            !m_keepIndicatorsShowing &&
            !m_isPointerOverVerticalScrollController &&
            !m_isPointerOverHorizontalScrollController)
        {
            UpdateScrollControllersVisualState(true /*useTransitions*/, false /*showIndicators*/, true /*hideIndicators*/);
        }
    }

    private void OnNoIndicatorStateStoryboardCompleted(object sender, object args)
    {
        Debug.Assert(m_hasNoIndicatorStateStoryboardCompletedHandler);

        m_showingMouseIndicators = false;
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

    private void OnScrollAnimationStarting(object sender, ScrollingScrollAnimationStartingEventArgs args)
    {
        ScrollAnimationStarting?.Invoke(this, args);
    }

    private void OnScrollControllerCanScrollChanged(IScrollController sender, object? args)
    {
        // IScrollController::CanScroll changed and affect the scroll controller's visibility when its visibility mode is Auto.
        if (m_horizontalScrollController is not null && m_horizontalScrollController == sender)
        {
            UpdateScrollControllersVisibility(true /*horizontalChange*/, false /*verticalChange*/);
        }
        else if (m_verticalScrollController is not null && m_verticalScrollController == sender)
        {
            UpdateScrollControllersVisibility(false /*horizontalChange*/, true /*verticalChange*/);
        }
    }

    private void OnScrollControllerIsScrollingWithMouseChanged(IScrollController sender, object? args)
    {
        bool isScrollControllerScrollingWithMouse = sender.IsScrollingWithMouse;
        bool showIndicators = false;
        bool hideIndicators = false;

        if (m_horizontalScrollController is not null && m_horizontalScrollController == sender)
        {
            UpdateScrollControllersAutoHiding();

            if (m_isHorizontalScrollControllerScrollingWithMouse != isScrollControllerScrollingWithMouse)
            {
                m_isHorizontalScrollControllerScrollingWithMouse = isScrollControllerScrollingWithMouse;

                if (isScrollControllerScrollingWithMouse)
                {
                    // Prevent the vertical scroll controller from fading out while the user is scrolling with mouse with the horizontal one.
                    m_keepIndicatorsShowing = true;
                    showIndicators = true;
                }
                else
                {
                    // Make the scroll controllers fade out, after the normal delay, if they are auto-hiding.
                    m_keepIndicatorsShowing = false;
                    hideIndicators = AreScrollControllersAutoHiding();
                }
            }

            // IScrollController::CanScroll might have changed and affect the scroll controller's visibility
            // when its visibility mode is Auto.
            UpdateScrollControllersVisibility(true /*horizontalChange*/, false /*verticalChange*/);
            UpdateVisualStates(true /*useTransitions*/, showIndicators, hideIndicators);
        }
        else if (m_verticalScrollController is not null && m_verticalScrollController == sender)
        {
            UpdateScrollControllersAutoHiding();

            if (m_isVerticalScrollControllerScrollingWithMouse != isScrollControllerScrollingWithMouse)
            {
                m_isVerticalScrollControllerScrollingWithMouse = isScrollControllerScrollingWithMouse;

                if (isScrollControllerScrollingWithMouse)
                {
                    // Prevent the horizontal scroll controller from fading out while the user is scrolling with mouse with the vertical one.
                    m_keepIndicatorsShowing = true;
                    showIndicators = true;
                }
                else
                {
                    // Make the scroll controllers fade out, after the normal delay, if they are auto-hiding.
                    m_keepIndicatorsShowing = false;
                    hideIndicators = AreScrollControllersAutoHiding();
                }
            }

            // IScrollController::CanScroll might have changed and affect the scroll controller's visibility
            // when its visibility mode is Auto.
            UpdateScrollControllersVisibility(false /*horizontalChange*/, true /*verticalChange*/);
            UpdateVisualStates(true /*useTransitions*/, showIndicators, hideIndicators);
        }
    }

    private void OnScrollPresenterAnchorRequested(object sender, ScrollingAnchorRequestedEventArgs args)
    {
        AnchorRequested?.Invoke(this, args);
    }

    private void OnScrollPresenterBringingIntoView(object sender, ScrollingBringingIntoViewEventArgs args)
    {
        if (m_bringIntoViewOperations.Count > 0)
        {
            var requestEventArgs = args.RequestEventArgs;

            foreach (var operationsIter in m_bringIntoViewOperations)
            {
                var bringIntoViewOperation = operationsIter;

                if (requestEventArgs.TargetElement == bringIntoViewOperation.TargetElement)
                {
                    // We either want to cancel this BringIntoView operation (because we are handling the scrolling ourselves) or we want to force the operation to be animated
                    if (bringIntoViewOperation.ShouldCancelBringIntoView())
                    {
                        args.Cancel = true;
                    }
                    else
                    {
                        requestEventArgs.AnimationDesired = (true);
                    }

                    break;
                }
            }
        }

        BringingIntoView?.Invoke(this, args);
    }

    private void OnScrollPresenterExtentChanged(object sender, object? args)
    {
        ExtentChanged?.Invoke(this, args);
    }

    private void OnScrollPresenterPropertyChanged(DependencyObject sender, DependencyProperty args)
    {
        if (args == ScrollPresenter.ComputedHorizontalScrollModeProperty)
        {
            SetValue(ComputedHorizontalScrollModeProperty, _scrollPresenter.ComputedHorizontalScrollMode);
        }
        else if (args == ScrollPresenter.ComputedVerticalScrollModeProperty)
        {
            SetValue(ComputedVerticalScrollModeProperty, _scrollPresenter.ComputedVerticalScrollMode);
        }
    }

    private void OnScrollPresenterScrollCompleted(object sender, ScrollingScrollCompletedEventArgs args)
    {
        if (args.CorrelationId == m_horizontalAddScrollVelocityOffsetChangeCorrelationId)
        {
            m_horizontalAddScrollVelocityOffsetChangeCorrelationId = s_noOpCorrelationId;
        }
        else if (args.CorrelationId == m_verticalAddScrollVelocityOffsetChangeCorrelationId)
        {
            m_verticalAddScrollVelocityOffsetChangeCorrelationId = s_noOpCorrelationId;
        }

        ScrollCompleted?.Invoke(this, args);
    }

    private void OnScrollPresenterStateChanged(object sender, object args)
    {
        if (_scrollPresenter is { } scrollPresenter)
        {
            if (scrollPresenter.State == ScrollingInteractionState.Interaction)
            {
                m_preferMouseIndicators = false;
            }
        }

        StateChanged?.Invoke(this, args);
    }

    private void OnScrollPresenterViewChanged(object sender, object args)
    {
        // Unless the control is still loading, show the scroll controller indicators when the view changes. For example,
        // when using Ctrl+/- to zoom, mouse-wheel to scroll or zoom, or any other input type. Keep the existing indicator type.
        if (IsLoaded)
        {
            UpdateVisualStates(
                true  /*useTransitions*/,
                true  /*showIndicators*/,
                false /*hideIndicators*/,
                false /*scrollControllersAutoHidingChanged*/,
                false /*updateScrollControllersAutoHiding*/,
                true  /*onlyForAutoHidingScrollControllers*/);
        }

        ViewChanged?.Invoke(this, args);
    }

    private void OnScrollPresenterZoomCompleted(object sender, ScrollingZoomCompletedEventArgs args)
    {
        ZoomCompleted?.Invoke(this, args);
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

    private void OnScrollViewPointerExited(
            object sender,
            PointerRoutedEventArgs args)
    {
        if (args.Pointer.PointerDeviceType != PointerDeviceType.Touch)
        {
            // Mouse/Pen inputs dominate. If touch panning indicators are shown, switch to mouse indicators.
            m_isPointerOverHorizontalScrollController = false;
            m_isPointerOverVerticalScrollController = false;
            m_preferMouseIndicators = true;

            UpdateVisualStates(
                true  /*useTransitions*/,
                true  /*showIndicators*/,
                false /*hideIndicators*/,
                false /*scrollControllersAutoHidingChanged*/,
                true  /*updateScrollControllersAutoHiding*/,
                true  /*onlyForAutoHidingScrollControllers*/);

            if (AreScrollControllersAutoHiding())
            {
                HideIndicatorsAfterDelay();
            }
        }
    }

    private void OnScrollViewPointerMoved(object sender, PointerRoutedEventArgs args)
    {
        // Don't process if this is a generated replay of the event.
        if (args.IsGenerated)
        {
            return;
        }

        if (args.Pointer.PointerDeviceType != PointerDeviceType.Touch)
        {
            // Mouse/Pen inputs dominate. If touch panning indicators are shown, switch to mouse indicators.
            m_preferMouseIndicators = true;

            UpdateVisualStates(
                true  /*useTransitions*/,
                true  /*showIndicators*/,
                false /*hideIndicators*/,
                false /*scrollControllersAutoHidingChanged*/,
                false /*updateScrollControllersAutoHiding*/,
                true  /*onlyForAutoHidingScrollControllers*/);

            if (AreScrollControllersAutoHiding() &&
                !SharedHelpers.IsAnimationsEnabled() &&
                m_hideIndicatorsTimer is not null &&
                (m_isPointerOverHorizontalScrollController || m_isPointerOverVerticalScrollController))
            {
                ResetHideIndicatorsTimer();
            }
        }
    }

    private void OnScrollViewPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
        {
            PointerPoint pointerPoint = args.GetCurrentPoint(null);
            PointerPointProperties pointerPointProperties = pointerPoint.Properties;

            m_isLeftMouseButtonPressedForFocus = pointerPointProperties.IsLeftButtonPressed;
        }

        // Show the scroll controller indicators as soon as a pointer is pressed on the ScrollView.
        UpdateVisualStates(
            true  /*useTransitions*/,
            true  /*showIndicators*/,
            false /*hideIndicators*/,
            false /*scrollControllersAutoHidingChanged*/,
            true  /*updateScrollControllersAutoHiding*/,
            true  /*onlyForAutoHidingScrollControllers*/);
    }

    private void OnScrollViewPointerReleased(object sender, PointerRoutedEventArgs args)
    {
        bool takeFocus = false;

        if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse && m_isLeftMouseButtonPressedForFocus)
        {
            m_isLeftMouseButtonPressedForFocus = false;
            takeFocus = true;
        }

        if (args.Handled)
        {
            return;
        }

        if (takeFocus)
        {
            bool tookFocus = Focus(FocusState.Pointer);
            args.Handled = (tookFocus);
        }
    }

    private void OnScrollViewUnloaded(object sender, RoutedEventArgs args)
    {
        m_showingMouseIndicators = false;
        m_keepIndicatorsShowing = false;
        m_bringIntoViewOperations.Clear();

        UnhookCompositionTargetRendering();
        ResetHideIndicatorsTimer();
    }

    private void OnVerticalScrollControllerPointerEntered(object sender, PointerRoutedEventArgs args)
    {
        HandleScrollControllerPointerEntered(false /*isForHorizontalScrollController*/);
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
        var hideIndicatorsTimer = m_hideIndicatorsTimer;

        if (hideIndicatorsTimer is not null && hideIndicatorsTimer.IsEnabled)
        {
            hideIndicatorsTimer.Stop();
            if (restart)
            {
                hideIndicatorsTimer.Start();
            }
        }
    }

    private void UnhookCompositionTargetRendering()
    {
        Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;
    }

    private void UnhookHorizontalScrollControllerEvents(bool isForDestructor)
    {
        if (isForDestructor)
        {
        }
        else
        {
        }

        if (m_horizontalScrollController is IScrollController horizontalScrollController)
        {
            horizontalScrollController.CanScrollChanged -= OnScrollControllerCanScrollChanged;
            horizontalScrollController.IsScrollingWithMouseChanged -= OnScrollControllerIsScrollingWithMouseChanged;
        }

        if (m_horizontalScrollControllerElement is UIElement horizontalScrollControllerElement)
        {
            if (m_onHorizontalScrollControllerPointerEnteredHandler is not null)
            {
                horizontalScrollControllerElement.RemoveHandler(UIElement.PointerEnteredEvent, m_onHorizontalScrollControllerPointerEnteredHandler);
                m_onHorizontalScrollControllerPointerEnteredHandler = null;
            }

            if (m_onHorizontalScrollControllerPointerExitedHandler is not null)
            {
                horizontalScrollControllerElement.RemoveHandler(UIElement.PointerExitedEvent, m_onHorizontalScrollControllerPointerExitedHandler);
                m_onHorizontalScrollControllerPointerExitedHandler = null;
            }
        }
    }

    private void UnhookScrollPresenterEvents(bool isForDestructor)
    {
        if (_scrollPresenter is { } scrollPresenter)
        {
            scrollPresenter.ExtentChanged -= OnScrollPresenterExtentChanged;
            scrollPresenter.StateChanged -= OnScrollPresenterStateChanged;
            scrollPresenter.ScrollAnimationStarting -= OnScrollAnimationStarting;
            scrollPresenter.ZoomAnimationStarting -= OnZoomAnimationStarting;
            scrollPresenter.ViewChanged -= OnScrollPresenterViewChanged;
            scrollPresenter.ScrollCompleted -= OnScrollPresenterScrollCompleted;
            scrollPresenter.ZoomCompleted -= OnScrollPresenterZoomCompleted;
            scrollPresenter.BringingIntoView -= OnScrollPresenterBringingIntoView;
            scrollPresenter.AnchorRequested -= OnScrollPresenterAnchorRequested;

            DependencyObject scrollPresenterAsDO = scrollPresenter as DependencyObject;

            if (m_scrollPresenterComputedHorizontalScrollModeChangedToken != 0)
            {
                scrollPresenterAsDO.UnregisterPropertyChangedCallback(ScrollPresenter.ComputedHorizontalScrollModeProperty, m_scrollPresenterComputedHorizontalScrollModeChangedToken);
                m_scrollPresenterComputedHorizontalScrollModeChangedToken = 0;
            }

            if (m_scrollPresenterComputedVerticalScrollModeChangedToken != 0)
            {
                scrollPresenterAsDO.UnregisterPropertyChangedCallback(ScrollPresenter.ComputedVerticalScrollModeProperty, m_scrollPresenterComputedVerticalScrollModeChangedToken);
                m_scrollPresenterComputedVerticalScrollModeChangedToken = 0;
            }
        }
    }

    private void UnhookScrollViewEvents()
    {
        GettingFocus -= OnScrollViewGettingFocus;
        IsEnabledChanged -= OnScrollViewIsEnabledChanged;
        Unloaded -= OnScrollViewUnloaded;

        if (m_onPointerEnteredEventHandler is not null)
        {
            RemoveHandler(UIElement.PointerEnteredEvent, m_onPointerEnteredEventHandler);
            m_onPointerEnteredEventHandler = null;
        }

        if (m_onPointerMovedEventHandler is not null)
        {
            RemoveHandler(UIElement.PointerMovedEvent, m_onPointerMovedEventHandler);
            m_onPointerMovedEventHandler = null;
        }

        if (m_onPointerExitedEventHandler is not null)
        {
            RemoveHandler(UIElement.PointerExitedEvent, m_onPointerExitedEventHandler);
            m_onPointerExitedEventHandler = null;
        }

        if (m_onPointerPressedEventHandler is not null)
        {
            RemoveHandler(UIElement.PointerPressedEvent, m_onPointerPressedEventHandler);
            m_onPointerPressedEventHandler = null;
        }

        if (m_onPointerReleasedEventHandler is not null)
        {
            RemoveHandler(UIElement.PointerReleasedEvent, m_onPointerReleasedEventHandler);
            m_onPointerReleasedEventHandler = null;
        }

        if (m_onPointerCanceledEventHandler is not null)
        {
            RemoveHandler(UIElement.PointerCanceledEvent, m_onPointerCanceledEventHandler);
            m_onPointerCanceledEventHandler = null;
        }
    }

    private void UnhookVerticalScrollControllerEvents(bool isForDestructor)
    {
        if (isForDestructor)
        {
        }
        else
        {
        }

        if (m_verticalScrollController is IScrollController verticalScrollController)
        {
            verticalScrollController.CanScrollChanged -= OnScrollControllerCanScrollChanged;
            verticalScrollController.IsScrollingWithMouseChanged -= OnScrollControllerIsScrollingWithMouseChanged;
        }

        if (m_verticalScrollControllerElement is UIElement verticalScrollControllerElement)
        {
            if (m_onVerticalScrollControllerPointerEnteredHandler is not null)
            {
                verticalScrollControllerElement.RemoveHandler(UIElement.PointerEnteredEvent, m_onVerticalScrollControllerPointerEnteredHandler);
                m_onVerticalScrollControllerPointerEnteredHandler = null;
            }

            if (m_onVerticalScrollControllerPointerExitedHandler is not null)
            {
                verticalScrollControllerElement.RemoveHandler(UIElement.PointerExitedEvent, m_onVerticalScrollControllerPointerExitedHandler);
                m_onVerticalScrollControllerPointerExitedHandler = null;
            }
        }
    }

    private void UpdateHorizontalScrollController(IScrollController horizontalScrollController, UIElement horizontalScrollControllerElement)
    {
        UnhookHorizontalScrollControllerEvents(false /*isForDestructor*/);

        m_horizontalScrollController = horizontalScrollController;
        m_horizontalScrollControllerElement = horizontalScrollControllerElement;
        HookHorizontalScrollControllerEvents();
        UpdateScrollPresenterHorizontalScrollController(horizontalScrollController);
    }

    private void UpdateScrollControllersAutoHiding(bool forceUpdate = false)
    {
        if ((forceUpdate || m_autoHideScrollBarsState.m_uiSettings5 is null) && m_autoHideScrollControllersValid)
        {
            m_autoHideScrollControllersValid = false;

            bool oldAutoHideScrollControllers = m_autoHideScrollControllers;
            bool newAutoHideScrollControllers = AreScrollControllersAutoHiding();

            if (oldAutoHideScrollControllers != newAutoHideScrollControllers)
            {
                UpdateVisualStates(
                    true  /*useTransitions*/,
                    false /*showIndicators*/,
                    false /*hideIndicators*/,
                    true  /*scrollControllersAutoHidingChanged*/);
            }
        }
    }

    private void UpdateScrollControllersSeparator(UIElement scrollControllersSeparator)
    {
        m_scrollControllersSeparatorElement = scrollControllersSeparator;
    }

    private void UpdateScrollControllersSeparatorVisualState(bool useTransitions = true, bool scrollControllersAutoHidingChanged = false)
    {
        if (!IsScrollControllersSeparatorVisible())
        {
            return;
        }

        bool isEnabled = IsEnabled;
        bool areScrollControllersAutoHiding = AreScrollControllersAutoHiding();
        bool showScrollControllersSeparator = !areScrollControllersAutoHiding;

        if (!showScrollControllersSeparator &&
            AreBothScrollControllersVisible() &&
            (m_preferMouseIndicators || m_showingMouseIndicators) &&
            (m_isPointerOverHorizontalScrollController || m_isPointerOverVerticalScrollController))
        {
            showScrollControllersSeparator = true;
        }

        // Select the proper state for the scroll controllers separator within the ScrollBarsSeparatorStates group:
        if (SharedHelpers.IsAnimationsEnabled())
        {
            // When OS animations are turned on, show the separator when a scroll controller is shown unless the ScrollView is disabled, using an animation.
            if (showScrollControllersSeparator && isEnabled)
            {
                GoToState(s_scrollBarsSeparatorExpanded, useTransitions);
            }
            else if (isEnabled)
            {
                GoToState(s_scrollBarsSeparatorCollapsed, useTransitions);
            }
            else
            {
                GoToState(s_scrollBarsSeparatorCollapsedDisabled, useTransitions);
            }
        }
        else
        {
            // OS animations are turned off. Show or hide the separator depending on the presence of scroll controllers, without an animation.
            // When the ScrollView is disabled, hide the separator in sync with the ScrollBar(s).
            if (showScrollControllersSeparator)
            {
                if (isEnabled)
                {
                    GoToState((areScrollControllersAutoHiding || scrollControllersAutoHidingChanged) ? s_scrollBarsSeparatorExpandedWithoutAnimation : s_scrollBarsSeparatorDisplayedWithoutAnimation, useTransitions);
                }
                else
                {
                    GoToState(s_scrollBarsSeparatorCollapsed, useTransitions);
                }
            }
            else
            {
                GoToState(isEnabled ? s_scrollBarsSeparatorCollapsedWithoutAnimation : s_scrollBarsSeparatorCollapsed, useTransitions);
            }
        }
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
                isHorizontalScrollControllerVisible = scrollBarVisibility == ScrollingScrollBarVisibility.Visible;
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
        UnhookScrollPresenterEvents(false /*isForDestructor*/);
        _scrollPresenter = null;

        SetValue(ScrollPresenterProperty, scrollPresenter);

        if (scrollPresenter is not null)
        {
            _scrollPresenter = (scrollPresenter);
            HookScrollPresenterEvents();
        }
    }

    private void UpdateScrollPresenterHorizontalScrollController(IScrollController horizontalScrollController)
    {
        if (_scrollPresenter is not null)
        {
            _scrollPresenter.HorizontalScrollController = (horizontalScrollController);
        }
    }

    private void UpdateScrollPresenterVerticalScrollController(IScrollController verticalScrollController)
    {
        if (_scrollPresenter is { } scrollPresenter)
        {
            scrollPresenter.VerticalScrollController = (verticalScrollController);
        }
    }

    private void UpdateVerticalScrollController(IScrollController verticalScrollController, UIElement verticalScrollControllerElement)
    {
        UnhookVerticalScrollControllerEvents(false /*isForDestructor*/);

        m_verticalScrollController = verticalScrollController;
        m_verticalScrollControllerElement = verticalScrollControllerElement;
        HookVerticalScrollControllerEvents();
        UpdateScrollPresenterVerticalScrollController(verticalScrollController);
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

    private struct AutoHideScrollBarsState
    {
        internal UISettings m_uiSettings5;

        internal void HookUISettingsEvent(ScrollView scrollView)
        {
            if (m_uiSettings5 is null)
            {
                if (ApiInformation.IsReadOnlyPropertyPresent("Windows.UI.ViewManagement.UISettings", "AutoHideScrollBars"))
                {
                    m_uiSettings5 = new UISettings();
                }

                if (m_uiSettings5 is not null)
                {
                    m_uiSettings5.AutoHideScrollBarsChanged += scrollView.OnAutoHideScrollBarsChanged;
                }
            }
        }
    }
}