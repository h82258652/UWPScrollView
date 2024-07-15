using Microsoft.UI.Xaml.Automation.Peers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents a primitive container that provides scroll, pan, and zoom support for its content.
/// </summary>
[ContentProperty(Name = nameof(Content))]
public class ScrollPresenter : Panel, IScrollAnchorProvider
{
    /// <summary>
    /// Identifies the <see cref="Background"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty;

    /// <summary>
    /// Identifies the <see cref="ComputedHorizontalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedHorizontalScrollModeProperty = DependencyProperty.Register(
        nameof(ComputedHorizontalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingScrollMode.Disabled, OnComputedHorizontalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ComputedVerticalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedVerticalScrollModeProperty = DependencyProperty.Register(
        nameof(ComputedVerticalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingScrollMode.Disabled, OnComputedVerticalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ContentOrientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentOrientationProperty = DependencyProperty.Register(
        nameof(ContentOrientation),
        typeof(ScrollingContentOrientation),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingContentOrientation.Both, OnContentOrientationPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="Content"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(UIElement),
        typeof(ScrollPresenter),
        new PropertyMetadata(null, OnContentPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalAnchorRatio"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalAnchorRatioProperty = DependencyProperty.Register(
        nameof(HorizontalAnchorRatio),
        typeof(double),
        typeof(ScrollPresenter),
        new PropertyMetadata(0d, OnHorizontalAnchorRatioPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollChainMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollChainModeProperty;

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollModeProperty;

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollRailMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollRailModeProperty;

    /// <summary>
    /// Identifies the <see cref="IgnoredInputKinds"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IgnoredInputKindsProperty;

    /// <summary>
    /// Identifies the <see cref="MaxZoomFactor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaxZoomFactorProperty;

    /// <summary>
    /// Identifies the <see cref="MinZoomFactor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinZoomFactorProperty;

    /// <summary>
    /// Identifies the <see cref="VerticalAnchorRatio"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalAnchorRatioProperty = DependencyProperty.Register(
        nameof(VerticalAnchorRatio),
        typeof(double),
        typeof(ScrollPresenter),
        new PropertyMetadata(0d, OnVerticalAnchorRatioPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="VerticalScrollChainMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollChainModeProperty;

    /// <summary>
    /// Identifies the <see cref="VerticalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollModeProperty;

    /// <summary>
    /// Identifies the <see cref="VerticalScrollRailMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollRailModeProperty;

    /// <summary>
    /// Identifies the <see cref="ZoomChainMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ZoomChainModeProperty;

    /// <summary>
    /// Identifies the <see cref="ZoomMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ZoomModeProperty = DependencyProperty.Register(
        nameof(ZoomMode),
        typeof(ScrollingZoomMode),
        typeof(ScrollView),
        new PropertyMetadata(ScrollingZoomMode.Disabled, OnZoomModePropertyChanged));

    private const uint E_ACCESSDENIED = 0x80070005;
    private const string s_extentSourcePropertyName = "Extent";
    private const string s_maxOffsetPropertyName = "MaxOffset";
    private const string s_maxPositionSourcePropertyName = "MaxPosition";

    private const string s_minOffsetPropertyName = "MinOffset";
    private const string s_minPositionSourcePropertyName = "MinPosition";

    private const string s_multiplierPropertyName = "Multiplier";
    private const int s_noOpCorrelationId = -1;
    private const string s_offsetPropertyName = "Offset";
    private const string s_offsetSourcePropertyName = "Offset";
    private const string s_positionSourcePropertyName = "Position";
    private const string s_viewportSourcePropertyName = "Viewport";
    private const string s_zoomFactorSourcePropertyName = "ZoomFactor";
    private List<UIElement> m_anchorCandidates;
    private UIElement m_anchorElement;

    private Rect m_anchorElementBounds;

    private float m_animationRestartZoomFactor = 1;

    private Size m_availableSize;

    private float m_contentLayoutOffsetX;

    private float m_contentLayoutOffsetY;

    private ScrollingContentOrientation m_contentOrientation = ScrollingContentOrientation.Both;

    private Vector2 m_endOfInertiaPosition;

    private float m_endOfInertiaZoomFactor = 1;

    private CompositionPropertySet m_expressionAnimationSources;

    private IScrollController m_horizontalScrollController;
    private CompositionPropertySet m_horizontalScrollControllerExpressionAnimationSources;
    private IScrollControllerPanningInfo m_horizontalScrollControllerPanningInfo;
    private InteractionTracker m_interactionTracker;

    private IInteractionTrackerOwner m_interactionTrackerOwner;

    /// <summary>
    /// False when m_anchorElement is up-to-date, True otherwise.
    /// </summary>
    private bool m_isAnchorElementDirty = true;

    private ExpressionAnimation m_maxPositionExpressionAnimation;

    private ExpressionAnimation m_maxPositionSourceExpressionAnimation;
    private ExpressionAnimation m_minPositionExpressionAnimation;

    private ExpressionAnimation m_minPositionSourceExpressionAnimation;
    private ExpressionAnimation m_positionSourceExpressionAnimation;
    private VisualInteractionSource m_scrollPresenterVisualInteractionSource;
    private ScrollingInteractionState m_state = ScrollingInteractionState.Idle;

    private ExpressionAnimation m_translationExpressionAnimation;

    private double m_unzoomedExtentHeight;

    private double m_unzoomedExtentWidth;

    private IScrollController m_verticalScrollController;
    private CompositionPropertySet m_verticalScrollControllerExpressionAnimationSources;
    private IScrollControllerPanningInfo m_verticalScrollControllerPanningInfo;
    private List<ScrollSnapPointBase> m_verticalSnapPoints;
    private double m_viewportHeight;

    private double m_viewportWidth;

    private double m_zoomedHorizontalOffset;

    private double m_zoomedVerticalOffset;

    private float m_zoomFactor = 1;

    private ExpressionAnimation m_zoomFactorExpressionAnimation;

    private ExpressionAnimation m_zoomFactorSourceExpressionAnimation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollPresenter"/> class.
    /// </summary>
    public ScrollPresenter()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Occurs when the <see cref="ScrollView"/> is about to select an anchor element.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, ScrollingAnchorRequestedEventArgs> AnchorRequested;

    /// <summary>
    /// Occurs at the beginning of a bring-into-view request participation. Allows customization of that participation.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, ScrollingBringingIntoViewEventArgs> BringingIntoView;

    /// <summary>
    /// Occurs when either the <see cref="ExtentWidth"/> or <see cref="ExtentHeight"/> properties has changed.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, object?>? ExtentChanged;

    /// <summary>
    /// Occurs when a call to <see cref="ScrollTo"/> or <see cref="ScrollBy"/> triggers an animation.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, ScrollingScrollAnimationStartingEventArgs> ScrollAnimationStarting;

    /// <summary>
    /// Occurs when a <see cref="ScrollTo"/>, <see cref="ScrollBy"/>, or <see cref="AddScrollVelocity"/> asynchronous operation ends. Provides the original correlation ID.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, ScrollingScrollCompletedEventArgs> ScrollCompleted;

    /// <summary>
    /// Occurs when the current interaction state of the control has changed.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, object> StateChanged;

    /// <summary>
    /// Occurs when manipulations such as scrolling and zooming have caused the view to change.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, object> ViewChanged;

    /// <summary>
    /// Occurs when a call to <see cref="ZoomTo"/> or <see cref="ZoomBy"/> triggers an animation.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, ScrollingZoomAnimationStartingEventArgs> ZoomAnimationStarting;

    /// <summary>
    /// Occurs when a <see cref="ZoomTo"/>, <see cref="ZoomBy"/>, or <see cref="AddZoomVelocity"/> asynchronous operation ends. Provides the original correlation ID.
    /// </summary>
    public event TypedEventHandler<ScrollPresenter, ScrollingZoomCompletedEventArgs> ZoomCompleted;

    /// <summary>
    /// Gets or sets a brush that provides the background of the <see cref="ScrollPresenter"/>.
    /// </summary>
    public Brush Background
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
    public UIElement? CurrentAnchor => throw new NotImplementedException();

    /// <summary>
    /// Gets a <see cref="CompositionPropertySet"/> of scrolling related property values.
    /// </summary>
    public CompositionPropertySet ExpressionAnimationSources
    {
        get
        {
            SetupInteractionTrackerBoundaries();
            EnsureExpressionAnimationSources();

            return m_expressionAnimationSources;
        }
    }

    /// <summary>
    /// Gets the vertical size of all the scrollable content in the <see cref="ScrollPresenter"/>.
    /// </summary>
    public double ExtentHeight => m_unzoomedExtentHeight;

    /// <summary>
    /// Gets the horizontal size of all the scrollable content in the <see cref="ScrollPresenter"/>.
    /// </summary>
    public double ExtentWidth => m_unzoomedExtentWidth;

    /// <summary>
    /// Gets or sets the ratio within the viewport where the anchor element is selected.
    /// </summary>
    public double HorizontalAnchorRatio
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
    /// Gets the distance the content has been scrolled horizontally.
    /// </summary>
    public double HorizontalOffset => m_zoomedHorizontalOffset;

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain horizontal scrolling to an outer scroll control.
    /// </summary>
    public ScrollingChainMode HorizontalScrollChainMode
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
    /// Gets or sets the <see cref="IScrollController"/> implementation that can drive the horizontal scrolling of the <see cref="ScrollPresenter"/>.
    /// </summary>
    public IScrollController? HorizontalScrollController
    {
        get
        {
            return m_horizontalScrollController;
        }
        set
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
    /// Gets the collection of snap points that affect the <see cref="HorizontalOffset"/> property.
    /// </summary>
    public IList<ScrollSnapPointBase> HorizontalSnapPoints
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets or sets the kinds of user input the control does not respond to.
    /// </summary>
    public ScrollingInputKinds IgnoredInputKinds
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
            return Math.Max(0.0, GetZoomedExtentHeight() - ViewportHeight);
        }
    }

    /// <summary>
    /// Gets the horizontal length of the content that can be scrolled.
    /// </summary>
    public double ScrollableWidth
    {
        get
        {
            return Math.Max(0.0, GetZoomedExtentWidth() - ViewportWidth);
        }
    }

    /// <summary>
    /// Gets the current interaction state of the control.
    /// </summary>
    public ScrollingInteractionState State => m_state;

    /// <summary>
    /// Determines the vertical position of the <see cref="ScrollPresenter"/>'s anchor point with respect to the viewport. By default, the <see cref="ScrollPresenter"/> selects an element as its <see cref="CurrentAnchor"/> by identifying the element in its viewport nearest to the anchor point.
    /// </summary>
    public double VerticalAnchorRatio
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
    /// Gets the distance the content has been scrolled vertically.
    /// </summary>
    public double VerticalOffset
    {
        get
        {
            return m_zoomedVerticalOffset;
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain vertical scrolling to an outer scroll control.
    /// </summary>
    public ScrollingChainMode VerticalScrollChainMode
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
    /// Gets or sets the <see cref="IScrollController"/> implementation that can drive the vertical scrolling of the <see cref="ScrollPresenter"/>.
    /// </summary>
    public IScrollController? VerticalScrollController
    {
        get
        {
            return m_verticalScrollController;
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets or sets a value that determines how manipulation input influences scrolling behavior on the vertical axis.
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
    /// Gets or sets a value that indicates whether the scroll rail is enabled for the vertical axis.
    /// </summary>
    public ScrollingRailMode VerticalScrollRailMode
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
    /// Gets the collection of snap points that affect the <see cref="VerticalOffset"/> property.
    /// </summary>
    public IList<ScrollSnapPointBase> VerticalSnapPoints
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the vertical size of the viewable content in the <see cref="ScrollPresenter"/>.
    /// </summary>
    public double ViewportHeight => m_viewportHeight;

    /// <summary>
    /// Gets the horizontal size of the viewable content in the <see cref="ScrollPresenter"/>.
    /// </summary>
    public double ViewportWidth => m_viewportWidth;

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
            return m_zoomFactor;
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates the ability to zoom in and out by means of user input.
    /// </summary>
    public ScrollingZoomMode ZoomMode
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
    /// Gets the collection of snap points that affect the <see cref="ZoomFactor"/> property.
    /// </summary>
    public IList<ZoomSnapPointBase> ZoomSnapPoints
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
        int viewChangeCorrelationId;
        ChangeOffsetsWithAdditionalVelocityPrivate(
            offsetsVelocity,
            Vector2.Zero /*anticipatedOffsetsChange*/,
            inertiaDecayRate,
            InteractionTrackerAsyncOperationTrigger.DirectViewChange,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
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
        int viewChangeCorrelationId;
        ChangeZoomFactorWithAdditionalVelocityPrivate(
            zoomFactorVelocity,
            0.0f /*anticipatedZoomFactorChange*/,
            centerPoint,
            inertiaDecayRate,
            InteractionTrackerAsyncOperationTrigger.DirectViewChange,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }

    /// <summary>
    /// Registers a <see cref="UIElement"/> as a potential scroll anchor.
    /// </summary>
    /// <param name="element">A <see cref="UIElement"/> within the subtree of the <see cref="ScrollPresenter"/>.</param>
    public void RegisterAnchorCandidate(UIElement element)
    {
        if (element is null)
        {
            throw new ArgumentException();
        }

        if (!double.IsNaN(HorizontalAnchorRatio) || !double.IsNaN(VerticalAnchorRatio))
        {
            m_anchorCandidates.Add(element);
            m_isAnchorElementDirty = true;
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
        return ScrollBy(horizontalOffsetDelta, verticalOffsetDelta, null);
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
        int viewChangeCorrelationId;
        ChangeOffsetsPrivate(
            horizontalOffsetDelta /*zoomedHorizontalOffset*/,
            verticalOffsetDelta /*zoomedVerticalOffset*/,
            ScrollPresenterViewKind.RelativeToCurrentView,
            options,
            null /*bringIntoViewRequestedEventArgs*/,
            InteractionTrackerAsyncOperationTrigger.DirectViewChange,
            s_noOpCorrelationId /*existingViewChangeCorrelationId*/,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }

    /// <summary>
    /// Asynchronously scrolls to the specified offsets with animations enabled and snap points respected.
    /// </summary>
    /// <param name="horizontalOffset">The horizontal offset to scroll to.</param>
    /// <param name="verticalOffset">The vertical offset to scroll to.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ScrollTo(double horizontalOffset, double verticalOffset)
    {
        return ScrollTo(horizontalOffset, verticalOffset, null);
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
        int viewChangeCorrelationId;
        ChangeOffsetsPrivate(
            horizontalOffset /*zoomedHorizontalOffset*/,
            verticalOffset /*zoomedVerticalOffset*/,
            ScrollPresenterViewKind.Absolute,
            options,
            null /*bringIntoViewRequestedEventArgs*/,
            InteractionTrackerAsyncOperationTrigger.DirectViewChange,
            s_noOpCorrelationId /*existingViewChangeCorrelationId*/,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }

    /// <summary>
    /// Unregisters a <see cref="UIElement"/> as a potential scroll anchor.
    /// </summary>
    /// <param name="element">A <see cref="UIElement"/> within the subtree of the <see cref="ScrollView"/>.</param>
    public void UnregisterAnchorCandidate(UIElement element)
    {
        if (element is null)
        {
            throw new ArgumentException();
        }

        UIElement anchorCandidate = element;
        var it = m_anchorCandidates.FirstOrDefault(a => a == anchorCandidate);
        if (it is not null)
        {
            m_anchorCandidates.Remove(it);
            m_isAnchorElementDirty = true;
        }
    }

    /// <summary>
    /// Asynchronously zooms by the specified delta amount with animations enabled and snap point respected.
    /// </summary>
    /// <param name="zoomFactorDelta">The amount by which to change the zoom factor.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint)
    {
        return ZoomBy(zoomFactorDelta, centerPoint, null);
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
        int viewChangeCorrelationId;
        ChangeZoomFactorPrivate(
            zoomFactorDelta,
            centerPoint,
            ScrollPresenterViewKind.RelativeToCurrentView,
            options,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }

    /// <summary>
    /// Asynchronously zooms to the specified zoom factor with animations enabled and snap points respected.
    /// </summary>
    /// <param name="zoomFactor">The amount to scale the content.</param>
    /// <param name="centerPoint">The center point of the zoom factor change.</param>
    /// <returns>A correlation ID number used to associate this method call with corresponding events.</returns>
    public int ZoomTo(float zoomFactor, Vector2? centerPoint)
    {
        return ZoomTo(zoomFactor, centerPoint, null);
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
        int viewChangeCorrelationId;
        ChangeZoomFactorPrivate(
            zoomFactor,
            centerPoint,
            ScrollPresenterViewKind.Absolute,
            options,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }

    internal static void ValidateAnchorRatio(double value)
    {
        if (!IsAnchorRatioValid(value))
        {
            throw new ArgumentException();
        }
    }

    internal static void ValidateZoomFactoryBoundary(double value)
    {
        if (!IsZoomFactorBoundaryValid(value))
        {
            throw new ArgumentException();
        }
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        m_availableSize = availableSize;

        Size contentDesiredSize = new Size(0.0f, 0.0f);
        UIElement content = Content;

        if (content is not null)
        {
            // The content is measured with infinity in the directions in which it is not constrained, enabling this ScrollPresenter
            // to be scrollable in those directions.
            Size contentAvailableSize = new Size(
                (m_contentOrientation == ScrollingContentOrientation.Vertical || m_contentOrientation == ScrollingContentOrientation.None) ?
                    availableSize.Width : double.PositiveInfinity,
            (m_contentOrientation == ScrollingContentOrientation.Horizontal || m_contentOrientation == ScrollingContentOrientation.None) ?
                availableSize.Height : double.PositiveInfinity
        );

            if (m_contentOrientation != ScrollingContentOrientation.Both)
            {
                FrameworkElement contentAsFE = content as FrameworkElement;

                if (contentAsFE is not null)
                {
                    Thickness contentMargin = contentAsFE.Margin;

                    if (m_contentOrientation == ScrollingContentOrientation.Vertical || m_contentOrientation == ScrollingContentOrientation.None)
                    {
                        // Even though the content's Width is constrained, take into account the MinWidth, Width and MaxWidth values
                        // potentially set on the content so it is allowed to grow accordingly.
                        contentAvailableSize.Width = (float)(GetComputedMaxWidth(availableSize.Width, contentAsFE));
                    }
                    if (m_contentOrientation == ScrollingContentOrientation.Horizontal || m_contentOrientation == ScrollingContentOrientation.None)
                    {
                        // Even though the content's Height is constrained, take into account the MinHeight, Height and MaxHeight values
                        // potentially set on the content so it is allowed to grow accordingly.
                        contentAvailableSize.Height = (float)(GetComputedMaxHeight(availableSize.Height, contentAsFE));
                    }
                }
            }

            content.Measure(contentAvailableSize);
            contentDesiredSize = content.DesiredSize;
        }

        return contentDesiredSize;
    }

    private static string GetVisualTargetedPropertyName(ScrollPresenterDimension dimension)
    {
        throw new NotImplementedException();
    }

    private static bool IsAnchorRatioValid(double value)
    {
        return double.IsNaN(value) || (!double.IsInfinity(value) && value >= 0 && value <= 1);
    }

    private static bool IsZoomFactorBoundaryValid(double value)
    {
        throw new NotImplementedException();
    }

    private static void OnComputedHorizontalScrollModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnComputedVerticalScrollModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnContentOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnHorizontalAnchorRatioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnVerticalAnchorRatioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void OnZoomModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private void ChangeOffsetsPrivate(
        double zoomedHorizontalOffset,
        double zoomedVerticalOffset,
        ScrollPresenterViewKind offsetsKind,
        ScrollingScrollOptions? options,
        BringIntoViewRequestedEventArgs? bringIntoViewRequestedEventArgs,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        int existingViewChangeCorrelationId,
        out int viewChangeCorrelationId)
    {
        throw new NotImplementedException();
    }

    private void ChangeOffsetsWithAdditionalVelocityPrivate(
        Vector2 offsetsVelocity,
        Vector2 anticipatedOffsetsChange,
        Vector2? inertiaDecayRate,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        out int viewChangeCorrelationId)
    {
        throw new NotImplementedException();
    }

    private void ChangeZoomFactorPrivate(
        float zoomFactor,
        Vector2? centerPoint,
        ScrollPresenterViewKind zoomFactorKind,
        ScrollingZoomOptions? options,
        out int viewChangeCorrelationId)
    {
        throw new NotImplementedException();
    }

    private void ChangeZoomFactorWithAdditionalVelocityPrivate(
        float zoomFactorVelocity,
        float anticipatedZoomFactorChange,
        Vector2? centerPoint,
        float? inertiaDecayRate,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        out int viewChangeCorrelationId)
    {
        throw new NotImplementedException();
    }

    private void CompleteInteractionTrackerOperations(
        int requestId,
        ScrollPresenterViewChangeResult operationResult,
        ScrollPresenterViewChangeResult priorNonAnimatedOperationsResult,
        ScrollPresenterViewChangeResult priorAnimatedOperationsResult,
        bool completeNonAnimatedOperation,
        bool completeAnimatedOperation,
        bool completePriorNonAnimatedOperations,
        bool completePriorAnimatedOperations)
    {
        throw new NotImplementedException();
    }

    private void CustomAnimationStateEntered(InteractionTrackerCustomAnimationStateEnteredArgs args)
    {
        UpdateState(ScrollingInteractionState.Animation);
    }

    private void EnsureExpressionAnimationSources()
    {
        if (m_expressionAnimationSources is null)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            m_expressionAnimationSources = compositor.CreatePropertySet();
            m_expressionAnimationSources.InsertVector2(s_extentSourcePropertyName, new Vector2(0.0f, 0.0f));
            m_expressionAnimationSources.InsertVector2(s_viewportSourcePropertyName, new Vector2(0.0f, 0.0f));
            m_expressionAnimationSources.InsertVector2(s_offsetSourcePropertyName, new Vector2(m_contentLayoutOffsetX, m_contentLayoutOffsetY));
            m_expressionAnimationSources.InsertVector2(s_positionSourcePropertyName, new Vector2(0.0f, 0.0f));
            m_expressionAnimationSources.InsertVector2(s_minPositionSourcePropertyName, new Vector2(0.0f, 0.0f));
            m_expressionAnimationSources.InsertVector2(s_maxPositionSourcePropertyName, new Vector2(0.0f, 0.0f));
            m_expressionAnimationSources.InsertScalar(s_zoomFactorSourcePropertyName, 0.0f);

            Debug.Assert(m_interactionTracker is not null);
            Debug.Assert(m_positionSourceExpressionAnimation is null);
            Debug.Assert(m_minPositionSourceExpressionAnimation is null);
            Debug.Assert(m_maxPositionSourceExpressionAnimation is null);
            Debug.Assert(m_zoomFactorSourceExpressionAnimation is null);

            m_positionSourceExpressionAnimation = compositor.CreateExpressionAnimation("Vector2(it.Position.X, it.Position.Y)");
            m_positionSourceExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);

            m_minPositionSourceExpressionAnimation = compositor.CreateExpressionAnimation("Vector2(it.MinPosition.X, it.MinPosition.Y)");
            m_minPositionSourceExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);

            m_maxPositionSourceExpressionAnimation = compositor.CreateExpressionAnimation("Vector2(it.MaxPosition.X, it.MaxPosition.Y)");
            m_maxPositionSourceExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);

            m_zoomFactorSourceExpressionAnimation = compositor.CreateExpressionAnimation("it.Scale");
            m_zoomFactorSourceExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);

            StartExpressionAnimationSourcesAnimations();
            UpdateExpressionAnimationSources();
        }
    }

    private void EnsureInteractionTracker()
    {
        if (m_interactionTracker is null)
        {
            Debug.Assert(m_interactionTrackerOwner is null);
            m_interactionTrackerOwner = new InteractionTrackerOwner(this) as IInteractionTrackerOwner;

            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            m_interactionTracker = InteractionTracker.CreateWithOwner(compositor, m_interactionTrackerOwner);
        }
    }

    private void EnsurePositionBoundariesExpressionAnimations()
    {
        if (m_minPositionExpressionAnimation is null || m_maxPositionExpressionAnimation is null)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            if (m_minPositionExpressionAnimation is null)
            {
                m_minPositionExpressionAnimation = compositor.CreateExpressionAnimation();
            }
            if (m_maxPositionExpressionAnimation is null)
            {
                m_maxPositionExpressionAnimation = compositor.CreateExpressionAnimation();
            }
        }
    }

    private void EnsureScrollPresenterVisualInteractionSource()
    {
        throw new NotImplementedException();
    }

    private void EnsureTransformExpressionAnimations()
    {
        if (m_translationExpressionAnimation is null || m_zoomFactorExpressionAnimation is null)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            if (m_translationExpressionAnimation is null)
            {
                m_translationExpressionAnimation = compositor.CreateExpressionAnimation();
            }

            if (m_zoomFactorExpressionAnimation is null)
            {
                m_zoomFactorExpressionAnimation = compositor.CreateExpressionAnimation();
            }
        }
    }

    private Vector2 GetArrangeRenderSizesDelta(UIElement content)
    {
        Debug.Assert(content is not null);

        double deltaX = m_unzoomedExtentWidth - content.RenderSize.Width;
        double deltaY = m_unzoomedExtentHeight - content.RenderSize.Height;

        FrameworkElement? contentAsFE = content as FrameworkElement;

        if (contentAsFE is not null)
        {
            HorizontalAlignment horizontalAlignment = contentAsFE.HorizontalAlignment;
            VerticalAlignment verticalAlignment = contentAsFE.VerticalAlignment;
            Thickness contentMargin = contentAsFE.Margin;

            if (horizontalAlignment == HorizontalAlignment.Left)
            {
                deltaX = 0.0f;
            }
            else
            {
                deltaX -= contentMargin.Left + contentMargin.Right;
            }

            if (verticalAlignment == VerticalAlignment.Top)
            {
                deltaY = 0.0f;
            }
            else
            {
                deltaY -= contentMargin.Top + contentMargin.Bottom;
            }

            if (horizontalAlignment == HorizontalAlignment.Center ||
                horizontalAlignment == HorizontalAlignment.Stretch)
            {
                deltaX /= 2.0f;
            }

            if (verticalAlignment == VerticalAlignment.Center ||
                verticalAlignment == VerticalAlignment.Stretch)
            {
                deltaY /= 2.0f;
            }

            deltaX += contentMargin.Left;
            deltaY += contentMargin.Top;
        }

        return new Vector2((float)(deltaX), (float)(deltaY));
    }

    private double GetComputedMaxHeight(double defaultMaxHeight, FrameworkElement content)
    {
        Debug.Assert(content is not null);

        Thickness contentMargin = content.Margin;
        double marginHeight = contentMargin.Top + contentMargin.Bottom;
        double computedMaxHeight = defaultMaxHeight;
        double height = content.Height;
        double minHeight = content.MinHeight;
        double maxHeight = content.MaxHeight;

        if (!double.IsNaN(height))
        {
            height = Math.Max(0.0, height + marginHeight);
            computedMaxHeight = height;
        }
        if (!double.IsNaN(minHeight))
        {
            minHeight = Math.Max(0.0, minHeight + marginHeight);
            computedMaxHeight = Math.Max(computedMaxHeight, minHeight);
        }
        if (!double.IsNaN(maxHeight))
        {
            maxHeight = Math.Max(0.0, maxHeight + marginHeight);
            computedMaxHeight = Math.Min(computedMaxHeight, maxHeight);
        }

        return computedMaxHeight;
    }

    private double GetComputedMaxWidth(double defaultMaxWidth, FrameworkElement content)
    {
        throw new NotImplementedException();
    }

    private ScrollingScrollMode GetComputedScrollMode(ScrollPresenterDimension dimension, bool ignoreZoomMode = false)
    {
        throw new NotImplementedException();
    }

    private double GetZoomedExtentHeight()
    {
        throw new NotImplementedException();
    }

    private double GetZoomedExtentWidth()
    {
        throw new NotImplementedException();
    }

    private void HookCompositionTargetRendering()
    {
        Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
    }

    private void HookContentPropertyChanged(UIElement content)
    {
        throw new NotImplementedException();
    }

    private void MaximizeInteractionTrackerOperationsTicksCountdown()
    {
        throw new NotImplementedException();
    }

    private void OnCompositionTargetRendering(object sender, object args)
    {
        throw new NotImplementedException();
    }

    private void OnContentLayoutOffsetChanged(ScrollPresenterDimension dimension)
    {
        throw new NotImplementedException();
    }

    private void OnContentSizeChanged(
        UIElement content)
    {
        throw new NotImplementedException();
    }

    private void OnFlowDirectionChanged(DependencyObject sender, DependencyProperty args)
    {
        throw new NotImplementedException();
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        SetupInteractionTrackerBoundaries();

        EnsureScrollPresenterVisualInteractionSource();
        SetupScrollPresenterVisualInteractionSource();
        SetupScrollControllerVisualInterationSource(ScrollPresenterDimension.HorizontalScroll);
        SetupScrollControllerVisualInterationSource(ScrollPresenterDimension.VerticalScroll);

        if (m_horizontalScrollControllerExpressionAnimationSources is not null)
        {
            Debug.Assert(m_horizontalScrollControllerPanningInfo is not null);

            m_horizontalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                m_horizontalScrollControllerExpressionAnimationSources,
                s_minOffsetPropertyName,
                s_maxOffsetPropertyName,
                s_offsetPropertyName,
                s_multiplierPropertyName);
        }
        if (m_verticalScrollControllerExpressionAnimationSources is not null)
        {
            Debug.Assert(m_verticalScrollControllerPanningInfo is not null);

            m_verticalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                m_verticalScrollControllerExpressionAnimationSources,
                s_minOffsetPropertyName,
                s_maxOffsetPropertyName,
                s_offsetPropertyName,
                s_multiplierPropertyName);
        }

        UIElement content = Content;

        if (content is not null)
        {
            if (m_translationExpressionAnimation is null || m_zoomFactorExpressionAnimation is null)
            {
                EnsureTransformExpressionAnimations();
                SetupTransformExpressionAnimations(content);
            }

            // Process the potentially delayed operation in the OnCompositionTargetRendering handler.
            HookCompositionTargetRendering();
        }
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(m_scrollPresenterVisualInteractionSource is not null);

        if (m_horizontalScrollController is not null && m_horizontalScrollController.IsScrollingWithMouse)
        {
            return;
        }

        if (m_verticalScrollController is not null && m_verticalScrollController.IsScrollingWithMouse)
        {
            return;
        }

        UIElement content = Content;
        ScrollingScrollMode horizontalScrollMode = GetComputedScrollMode(ScrollPresenterDimension.HorizontalScroll);
        ScrollingScrollMode verticalScrollMode = GetComputedScrollMode(ScrollPresenterDimension.VerticalScroll);

        if (content is null ||
            (horizontalScrollMode == ScrollingScrollMode.Disabled &&
                verticalScrollMode == ScrollingScrollMode.Disabled &&
                ZoomMode == ScrollingZoomMode.Disabled))
        {
            return;
        }

        switch (args.Pointer.PointerDeviceType)
        {
            case PointerDeviceType.Touch:
                if (IsInputKindIgnored(ScrollingInputKinds.Touch))
                    return;
                break;

            case PointerDeviceType.Pen:
                if (IsInputKindIgnored(ScrollingInputKinds.Pen))
                    return;
                break;

            default:
                return;
        }

        // All UIElement instances between the touched one and the ScrollPresenter must include ManipulationModes.System in their
        // ManipulationMode property in order to trigger a manipulation. This allows to turn off touch interactions in particular.
        object source = args.OriginalSource;
        Debug.Assert(source is not null);

        DependencyObject sourceAsDO = source as DependencyObject;

        UIElement thisAsUIElement = this; // Need to have exactly the same interface as we're comparing below for object equality

        while (sourceAsDO is not null)
        {
            UIElement sourceAsUIE = sourceAsDO as UIElement;
            if (sourceAsUIE is not null)
            {
                ManipulationModes mm = sourceAsUIE.ManipulationMode;

                if ((mm & ManipulationModes.System) == ManipulationModes.None)
                {
                    return;
                }

                if (sourceAsUIE == thisAsUIElement)
                {
                    break;
                }
            }

            sourceAsDO = VisualTreeHelper.GetParent(sourceAsDO);
        };

        try
        {
            m_scrollPresenterVisualInteractionSource.TryRedirectForManipulation(args.GetCurrentPoint(null));
        }
        catch (Exception e)
        {
            // Swallowing Access Denied error because of InteractionTracker bug 17434718 which has been
            // causing crashes at least in RS3, RS4 and RS5.
            // TODO - Stop eating the error in future OS versions that include a fix for 17434718 if any.
            if ((uint)(e.HResult) != E_ACCESSDENIED)
            {
                throw;
            }
        }
    }

    private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        if (!IsLoaded)
        {
            Debug.Assert(RenderSize.Width == 0.0);
            Debug.Assert(RenderSize.Height == 0.0);

            // All potential pending operations are interrupted when the ScrollPresenter unloads.
            CompleteInteractionTrackerOperations(
                -1 /*requestId*/,
                ScrollPresenterViewChangeResult.Interrupted /*operationResult*/,
                ScrollPresenterViewChangeResult.Ignored     /*unused priorNonAnimatedOperationsResult*/,
                ScrollPresenterViewChangeResult.Ignored     /*unused priorAnimatedOperationsResult*/,
                true  /*completeNonAnimatedOperation*/,
                true  /*completeAnimatedOperation*/,
                false /*completePriorNonAnimatedOperations*/,
                false /*completePriorAnimatedOperations*/);

            // Unhook the potential OnCompositionTargetRendering handler since there are no pending operations.
            UnhookCompositionTargetRendering();

            UIElement? content = Content;

            UpdateUnzoomedExtentAndViewport(
                false /*renderSizeChanged*/,
                content is not null ? m_unzoomedExtentWidth : 0.0,
                content is not null ? m_unzoomedExtentHeight : 0.0,
                0.0 /*viewportWidth*/,
                0.0 /*viewportHeight*/);
        }
    }

    private void ProcessOffsetsChange(
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        OffsetsChange offsetsChange,
        int offsetsChangeCorrelationId,
        bool isForAsyncOperation)
    {
        throw new NotImplementedException();
    }

    private void ProcessOffsetsChange(
          InteractionTrackerAsyncOperationTrigger operationTrigger,
          OffsetsChangeWithAdditionalVelocity offsetsChangeWithAdditionalVelocity)
    {
        throw new NotImplementedException();
    }

    private bool RaiseBringingIntoView(
                                                                                                                                                                                                                                                                                                                                                                              double targetZoomedHorizontalOffset,
      double targetZoomedVerticalOffset,
        BringIntoViewRequestedEventArgs requestEventArgs,
        int offsetsChangeCorrelationId,
        ref ScrollingSnapPointsMode snapPointsMode)
    {
        throw new NotImplementedException();
    }

    private void RaiseExpressionAnimationStatusChanged(
        bool isExpressionAnimationStarted,
        string propertyName)
    {
        throw new NotImplementedException();
    }

    private void RaiseExtentChanged()
    {
        ExtentChanged?.Invoke(this, null);
    }

    private void RaisePostArrange()
    {
        throw new NotImplementedException();
    }

    private void RaiseViewChangeCompleted(
       bool isForScroll,
       ScrollPresenterViewChangeResult result,
       int viewChangeCorrelationId)
    {
        throw new NotImplementedException();
    }

    private CompositionAnimation RaiseZoomAnimationStarting(
                            ScalarKeyFrameAnimation zoomFactorAnimation,
            float endZoomFactor,
            Vector2 centerPoint,
            int zoomFactorChangeCorrelationId)
    {
        throw new NotImplementedException();
    }

    private void ResetAnchorElement()
    {
        throw new NotImplementedException();
    }

    private void ScrollToOffsets(double horizontalOffset, double verticalOffset)
    {
        if (m_interactionTracker is not null)
        {
            ScrollingScrollOptions options =
                new ScrollingScrollOptions(
                    ScrollingAnimationMode.Disabled,
                    ScrollingSnapPointsMode.Ignore);

            OffsetsChange offsetsChange =
                new OffsetsChange(
                    horizontalOffset,
                    verticalOffset,
                    ScrollPresenterViewKind.Absolute,
                    options); // NOTE: Using explicit cast to winrt::IInspectable to work around 17532876

            ProcessOffsetsChange(
                InteractionTrackerAsyncOperationTrigger.DirectViewChange,
                offsetsChange,
                s_noOpCorrelationId /*offsetsChangeCorrelationId*/,
                false /*isForAsyncOperation*/);
        }
    }

    private void SetupInteractionTrackerBoundaries()
    {
        if (m_interactionTracker is null)
        {
            EnsureInteractionTracker();
            SetupInteractionTrackerZoomFactorBoundaries(
                MinZoomFactor,
                MaxZoomFactor);
        }

        UIElement content = Content;

        if (content is not null && (m_minPositionExpressionAnimation is null || m_maxPositionExpressionAnimation is null))
        {
            EnsurePositionBoundariesExpressionAnimations();
            SetupPositionBoundariesExpressionAnimations(content);
        }
    }

    private void SetupInteractionTrackerZoomFactorBoundaries(double minZoomFactor, double maxZoomFactor)
    {
        throw new NotImplementedException();
    }

    private void SetupPositionBoundariesExpressionAnimations(UIElement content)
    {
        throw new NotImplementedException();
    }

    private void SetupScrollControllerVisualInterationSource(ScrollPresenterDimension dimension)
    {
        throw new NotImplementedException();
    }

    private void SetupScrollPresenterVisualInteractionSource()
    {
        throw new NotImplementedException();
    }

    private void SetupTransformExpressionAnimations(UIElement content)
    {
        Debug.Assert(content is not null);
        Debug.Assert(m_translationExpressionAnimation is not null);
        Debug.Assert(m_zoomFactorExpressionAnimation is not null);
        Debug.Assert(m_interactionTracker is not null);

        Vector2 arrangeRenderSizesDelta = GetArrangeRenderSizesDelta(content);
        bool isRightToLeftDirection = FlowDirection == FlowDirection.RightToLeft;
        bool isContentImage = content is Image;
        string translationExpression;

        if (isRightToLeftDirection)
        {
            if (isContentImage)
            {
                m_translationExpressionAnimation.SetScalarParameter("contentSizeX", (float)(m_unzoomedExtentWidth));
                translationExpression = "Vector3(it.Position.X + (it.Scale - 1.0f) * (adjustment.X + contentSizeX), -it.Position.Y + (it.Scale - 1.0f) * adjustment.Y, 0.0f)";
            }
            else
            {
                translationExpression = "Vector3(it.Position.X + (it.Scale - 1.0f) * adjustment.X, -it.Position.Y + (it.Scale - 1.0f) * adjustment.Y, 0.0f)";
            }
        }
        else
        {
            translationExpression = "Vector3(-it.Position.X + (it.Scale - 1.0f) * adjustment.X, -it.Position.Y + (it.Scale - 1.0f) * adjustment.Y, 0.0f)";
        }

        m_translationExpressionAnimation.Expression = (translationExpression);

        m_translationExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);
        m_translationExpressionAnimation.SetVector2Parameter("adjustment", arrangeRenderSizesDelta);

        m_zoomFactorExpressionAnimation.Expression = ("Vector3(it.Scale, it.Scale, 1.0f)");
        m_zoomFactorExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);

        StartTransformExpressionAnimations(content);
    }

    private void SetupVisualInteractionSourceCenterPointModifier(
        VisualInteractionSource visualInteractionSource,
        ScrollPresenterDimension dimension,
        bool flowDirectionChanged)
    {
        throw new NotImplementedException();
    }

    private void SetupVisualInteractionSourceMode(
            VisualInteractionSource visualInteractionSource,
            ScrollPresenterDimension dimension,
            ScrollingScrollMode scrollMode)
    {
        throw new NotImplementedException();
    }

    private void SetupVisualInteractionSourceMode(
            VisualInteractionSource visualInteractionSource,
            ScrollingZoomMode zoomMode)
    {
        throw new NotImplementedException();
    }

    private void SetupVisualInteractionSourceRailingMode(
                                VisualInteractionSource visualInteractionSource,
        ScrollPresenterDimension dimension,
        ScrollingRailMode railingMode)
    {
        Debug.Assert(visualInteractionSource is not null);
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            visualInteractionSource.IsPositionXRailsEnabled = (railingMode == ScrollingRailMode.Enabled);
        }
        else
        {
            visualInteractionSource.IsPositionYRailsEnabled = (railingMode == ScrollingRailMode.Enabled);
        }
    }

    private void StartExpressionAnimationSourcesAnimations()
    {
        throw new NotImplementedException();
    }

    private void StartTransformExpressionAnimations(UIElement content)
    {
        if (content is not null)
        {
            var zoomFactorPropertyName = GetVisualTargetedPropertyName(ScrollPresenterDimension.ZoomFactor);
            var scrollPropertyName = GetVisualTargetedPropertyName(ScrollPresenterDimension.Scroll);

            m_translationExpressionAnimation.Target = (scrollPropertyName);
            m_zoomFactorExpressionAnimation.Target = (zoomFactorPropertyName);

            content.StartAnimation(m_translationExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, scrollPropertyName /*propertyName*/);

            content.StartAnimation(m_zoomFactorExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, zoomFactorPropertyName /*propertyName*/);
        }
    }

    private void StopTransformExpressionAnimations(UIElement content)
    {
        if (content is not null)
        {
            var scrollPropertyName = GetVisualTargetedPropertyName(ScrollPresenterDimension.Scroll);

            content.StopAnimation(m_translationExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(false /*isExpressionAnimationStarted*/, scrollPropertyName /*propertyName*/);

            var zoomFactorPropertyName = GetVisualTargetedPropertyName(ScrollPresenterDimension.ZoomFactor);

            content.StopAnimation(m_zoomFactorExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(false /*isExpressionAnimationStarted*/, zoomFactorPropertyName /*propertyName*/);
        }
    }

    private void UnhookCompositionTargetRendering()
    {
        Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;
    }

    private void UnhookContentPropertyChanged(UIElement content)
    {
        throw new NotImplementedException();
    }

    private void UpdateContent(UIElement oldContent, UIElement newContent)
    {
        var children = Children;
        children.Clear();

        UnhookContentPropertyChanged(oldContent);

        if (newContent is not null)
        {
            children.Add(newContent);

            if (m_minPositionExpressionAnimation is not null && m_maxPositionExpressionAnimation is not null)
            {
                UpdatePositionBoundaries(newContent);
            }
            else if (m_interactionTracker is not null)
            {
                EnsurePositionBoundariesExpressionAnimations();
                SetupPositionBoundariesExpressionAnimations(newContent);
            }

            if (m_translationExpressionAnimation is not null && m_zoomFactorExpressionAnimation is not null)
            {
                UpdateTransformSource(oldContent, newContent);
            }
            else if (m_interactionTracker is not null)
            {
                EnsureTransformExpressionAnimations();
                SetupTransformExpressionAnimations(newContent);
            }

            HookContentPropertyChanged(newContent);
        }
        else
        {
            if (m_contentLayoutOffsetX != 0.0f)
            {
                m_contentLayoutOffsetX = 0.0f;
                OnContentLayoutOffsetChanged(ScrollPresenterDimension.HorizontalScroll);
            }

            if (m_contentLayoutOffsetY != 0.0f)
            {
                m_contentLayoutOffsetY = 0.0f;
                OnContentLayoutOffsetChanged(ScrollPresenterDimension.VerticalScroll);
            }

            if (m_interactionTracker is null || (m_zoomedHorizontalOffset == 0.0 && m_zoomedVerticalOffset == 0.0))
            {
                // Complete all active or delayed operations when there is no InteractionTracker, when the old content
                // was already at offsets (0,0). The ScrollToOffsets request below will result in their completion otherwise.
                CompleteInteractionTrackerOperations(
                    -1 /*requestId*/,
                    ScrollPresenterViewChangeResult.Interrupted /*operationResult*/,
                    ScrollPresenterViewChangeResult.Ignored     /*unused priorNonAnimatedOperationsResult*/,
                    ScrollPresenterViewChangeResult.Ignored     /*unused priorAnimatedOperationsResult*/,
                    true  /*completeNonAnimatedOperation*/,
                    true  /*completeAnimatedOperation*/,
                    false /*completePriorNonAnimatedOperations*/,
                    false /*completePriorAnimatedOperations*/);
            }

            if (m_interactionTracker is not null)
            {
                if (m_minPositionExpressionAnimation is not null && m_maxPositionExpressionAnimation is not null)
                {
                    UpdatePositionBoundaries(null);
                }
                if (m_translationExpressionAnimation is not null && m_zoomFactorExpressionAnimation is not null)
                {
                    StopTransformExpressionAnimations(oldContent);
                }
                ScrollToOffsets(0.0 /*zoomedHorizontalOffset*/, 0.0 /*zoomedVerticalOffset*/);
            }
        }
    }

    private void UpdateExpressionAnimationSources()
    {
        throw new NotImplementedException();
    }

    private void UpdatePositionBoundaries(UIElement content)
    {
        Debug.Assert(m_minPositionExpressionAnimation is not null);
        Debug.Assert(m_maxPositionExpressionAnimation is not null);
        Debug.Assert(m_interactionTracker is not null);

        if (content is null)
        {
            Vector3 boundaryPosition = new Vector3(0.0f);

            m_interactionTracker.MinPosition = (boundaryPosition);
            m_interactionTracker.MaxPosition = (boundaryPosition);
        }
        else
        {
            m_minPositionExpressionAnimation.SetScalarParameter("contentSizeX", (float)(m_unzoomedExtentWidth));
            m_maxPositionExpressionAnimation.SetScalarParameter("contentSizeX", (float)(m_unzoomedExtentWidth));
            m_minPositionExpressionAnimation.SetScalarParameter("contentSizeY", (float)(m_unzoomedExtentHeight));
            m_maxPositionExpressionAnimation.SetScalarParameter("contentSizeY", (float)(m_unzoomedExtentHeight));

            m_minPositionExpressionAnimation.SetScalarParameter("contentLayoutOffsetX", m_contentLayoutOffsetX);
            m_maxPositionExpressionAnimation.SetScalarParameter("contentLayoutOffsetX", m_contentLayoutOffsetX);
            m_minPositionExpressionAnimation.SetScalarParameter("contentLayoutOffsetY", m_contentLayoutOffsetY);
            m_maxPositionExpressionAnimation.SetScalarParameter("contentLayoutOffsetY", m_contentLayoutOffsetY);

            m_interactionTracker.StartAnimation(s_minPositionSourcePropertyName, m_minPositionExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_minPositionSourcePropertyName /*propertyName*/);

            m_interactionTracker.StartAnimation(s_maxPositionSourcePropertyName, m_maxPositionExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_maxPositionSourcePropertyName /*propertyName*/);
        }
    }

    private void UpdateScrollAutomationPatternProperties()
    {
        if (FrameworkElementAutomationPeer.FromElement(this) is AutomationPeer automationPeer)
        {
            ScrollPresenterAutomationPeer scrollPresenterAutomationPeer = automationPeer as ScrollPresenterAutomationPeer;
            if (scrollPresenterAutomationPeer is not null)
            {
                scrollPresenterAutomationPeer.UpdateScrollPatternProperties();
            }
        }
    }

    private void UpdateScrollControllerIsScrollable(ScrollPresenterDimension dimension)
    {
        throw new NotImplementedException();
    }

    private void UpdateScrollControllerValues(ScrollPresenterDimension dimension)
    {
        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            if (m_horizontalScrollController is not null)
            {
                m_horizontalScrollController.SetValues(
                    0.0 /*minOffset*/,
                    ScrollableWidth /*maxOffset*/,
                    m_zoomedHorizontalOffset /*offset*/,
                    ViewportWidth /*viewportLength*/);
            }
        }
        else
        {
            Debug.Assert(dimension == ScrollPresenterDimension.VerticalScroll);

            if (m_verticalScrollController is not null)
            {
                m_verticalScrollController.SetValues(
                    0.0 /*minOffset*/,
                    ScrollableHeight /*maxOffset*/,
                    m_zoomedVerticalOffset /*offset*/,
                    ViewportHeight /*viewportLength*/);
            }
        }
    }

    private void UpdateState(ScrollingInteractionState state)
    {
        throw new NotImplementedException();
    }

    private void UpdateTransformSource(UIElement oldContent, UIElement newContent)
    {
        Debug.Assert(m_translationExpressionAnimation is not null && m_zoomFactorExpressionAnimation is not null);
        Debug.Assert(m_interactionTracker is not null);

        StopTransformExpressionAnimations(oldContent);
        StartTransformExpressionAnimations(newContent);
    }

    private void UpdateUnzoomedExtentAndViewport(
        bool renderSizeChanged,
        double unzoomedExtentWidth,
        double unzoomedExtentHeight,
        double viewportWidth,
        double viewportHeight)
    {
        throw new NotImplementedException();
    }

    private void UpdateVisualInteractionSourceMode(ScrollPresenterDimension dimension)
    {
        ScrollingScrollMode scrollMode = GetComputedScrollMode(dimension);

        if (m_scrollPresenterVisualInteractionSource is not null)
        {
            SetupVisualInteractionSourceMode(
                m_scrollPresenterVisualInteractionSource,
                dimension,
                scrollMode);
        }

        UpdateScrollControllerIsScrollable(dimension);
    }
}