using Microsoft.UI.Xaml.Automation.Peers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    internal void InertiaStateEntered(InteractionTrackerInertiaStateEnteredArgs args)
    {

       Vector3? modifiedRestingPosition = args.ModifiedRestingPosition;
        Vector3 naturalRestingPosition = args.NaturalRestingPosition;
        float? modifiedRestingScale = args.ModifiedRestingScale;
        float naturalRestingScale = args.NaturalRestingScale;
        bool isTracingEnabled = false;
        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = GetInteractionTrackerOperationFromRequestId(args.RequestId);

        // Record the end-of-inertia view for this inertial phase. It may be needed for
        // custom pointer wheel processing.

        if (modifiedRestingPosition.HasValue)
        {
           Vector3 endOfInertiaPosition = modifiedRestingPosition.Value;
            m_endOfInertiaPosition = new Vector2( endOfInertiaPosition.X, endOfInertiaPosition.Y );
        }
        else
        {
            m_endOfInertiaPosition = new Vector2( naturalRestingPosition.X, naturalRestingPosition.Y );
        }

        if (modifiedRestingScale.HasValue)
        {
            m_endOfInertiaZoomFactor = modifiedRestingScale.Value;
        }
        else
        {
            m_endOfInertiaZoomFactor = naturalRestingScale;
        }

        if (interactionTrackerAsyncOperation is not null)
        {
            ViewChangeBase viewChangeBase = interactionTrackerAsyncOperation.GetViewChangeBase();

            if (viewChangeBase is not null && interactionTrackerAsyncOperation.GetOperationType() == InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity)
            {
                OffsetsChangeWithAdditionalVelocity offsetsChangeWithAdditionalVelocity = (viewChangeBase) as OffsetsChangeWithAdditionalVelocity;

                if (offsetsChangeWithAdditionalVelocity is not null)
                {
                    offsetsChangeWithAdditionalVelocity.AnticipatedOffsetsChange(Vector2.Zero);
                }
            }
        }

        UpdateState(ScrollingInteractionState.Inertia);
    }

    internal void IdleStateEntered(InteractionTrackerIdleStateEnteredArgs args)
    {
        UpdateState(ScrollingInteractionState.Idle);

        if (m_interactionTrackerAsyncOperations.Count > 0)
        {
            int requestId = args.RequestId;

            // Complete all operations recorded through ChangeOffsetsPrivate/ChangeOffsetsWithAdditionalVelocityPrivate
            // and ChangeZoomFactorPrivate/ChangeZoomFactorWithAdditionalVelocityPrivate calls.
            if (requestId != 0)
            {
                CompleteInteractionTrackerOperations(
                    requestId,
                    ScrollPresenterViewChangeResult.Completed   /*operationResult*/,
                    ScrollPresenterViewChangeResult.Completed   /*priorNonAnimatedOperationsResult*/,
                    ScrollPresenterViewChangeResult.Interrupted /*priorAnimatedOperationsResult*/,
                    true  /*completeNonAnimatedOperation*/,
                    true  /*completeAnimatedOperation*/,
                    true  /*completePriorNonAnimatedOperations*/,
                    true  /*completePriorAnimatedOperations*/);
            }
        }

        // Check if resting position corresponds to a non-unique mandatory snap point, for the three dimensions
        UpdateSnapPointsIgnoredValue(m_sortedConsolidatedHorizontalSnapPoints, ScrollPresenterDimension.HorizontalScroll);
        UpdateSnapPointsIgnoredValue(m_sortedConsolidatedVerticalSnapPoints, ScrollPresenterDimension.VerticalScroll);
        UpdateSnapPointsIgnoredValue(m_sortedConsolidatedZoomSnapPoints, ScrollPresenterDimension.ZoomFactor);

        // Stop Translation and Scale animations if needed, to trigger rasterization of Content & avoid fuzzy text rendering for instance.
        StopTranslationAndZoomFactorExpressionAnimations();
    }

    private bool UpdateSnapPointsIgnoredValue<T>(HashSet<SnapPointWrapper<T>> snapPointsSet,
    double newIgnoredValue)
    {
        bool ignoredValueUpdated = false;

        foreach (var snapPointWrapper in snapPointsSet)
        {
            if (snapPointWrapper.ResetIgnoredValue())
            {
                ignoredValueUpdated = true;
                break;
            }
        }

        int snapCount = 0;

        foreach (var snapPointWrapper in snapPointsSet)
        {
            SnapPointBase snapPoint = SnapPointWrapper < T >.GetSnapPointFromWrapper(snapPointWrapper);

            snapCount += snapPoint.SnapCount();

            if (snapCount > 1)
            {
                break;
            }
        }

        if (snapCount > 1)
        {
            foreach (var snapPointWrapper in snapPointsSet)
            {
                if (snapPointWrapper.SnapsAt(newIgnoredValue))
                {
                    snapPointWrapper.SetIgnoredValue(newIgnoredValue);
                    ignoredValueUpdated = true;
                    break;
                }
            }
        }

        return ignoredValueUpdated;

    }

    private void UpdateSnapPointsRanges<T>(
        HashSet<SnapPointWrapper<T>> snapPointsSet,
        bool forImpulseOnly)
    { 
throw new NotImplementedException();
}


    private void UpdateSnapPointsIgnoredValue<T>(
        HashSet<SnapPointWrapper<T>> snapPointsSet,
        ScrollPresenterDimension dimension)
    {
        double newIgnoredValue = new Func<double>(() => {

            switch (dimension)
            {
                case ScrollPresenterDimension.VerticalScroll:
                    return m_zoomedVerticalOffset / m_zoomFactor;
                case ScrollPresenterDimension.HorizontalScroll:
                    return m_zoomedHorizontalOffset / m_zoomFactor;
                case ScrollPresenterDimension.ZoomFactor:
                    return (double)(m_zoomFactor);
                default:
                    Debug.Assert(false);
                    return 0.0;
            }
        })();
        
        

        if (UpdateSnapPointsIgnoredValue(snapPointsSet, newIgnoredValue))
        {
            // The ignored snap point value has changed.
            UpdateSnapPointsRanges(snapPointsSet, true /*forImpulseOnly*/);

            Compositor compositor = m_interactionTracker.Compositor;
            IList<InteractionTrackerInertiaModifier> modifiers = new List<InteractionTrackerInertiaModifier>();

            foreach (var snapPointWrapper in snapPointsSet)
            {
                var modifier = InteractionTrackerInertiaRestingValue.Create(compositor);
                var (conditionExpressionAnimation, restingValueExpressionAnimation) = snapPointWrapper.GetUpdatedExpressionAnimationsForImpulse();

                modifier.Condition = (conditionExpressionAnimation);
                modifier.RestingValue = (restingValueExpressionAnimation);

                modifiers.Add(modifier);
            }

            switch (dimension)
            {
                case ScrollPresenterDimension.VerticalScroll:
                    m_interactionTracker.ConfigurePositionYInertiaModifiers(modifiers);
                    break;
                case ScrollPresenterDimension.HorizontalScroll:
                    m_interactionTracker.ConfigurePositionXInertiaModifiers(modifiers);
                    break;
                case ScrollPresenterDimension.ZoomFactor:
                    m_interactionTracker.ConfigureScaleInertiaModifiers(modifiers);
                    break;
            }
        }
    }

    /// <summary>
    /// Identifies the <see cref="Background"/> dependency property.
    /// </summary>
    public new static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
        nameof(Background),
        typeof(Brush),
        typeof(ScrollPresenter),
        new PropertyMetadata(null, OnBackgroundPropertyChanged));

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
        new PropertyMetadata(null, OnContentPropertyChangedX));

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
    public static readonly DependencyProperty HorizontalScrollChainModeProperty = DependencyProperty.Register(
        nameof(HorizontalScrollChainMode),
        typeof(ScrollingChainMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingChainMode.Auto, OnHorizontalScrollChainModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollModeProperty = DependencyProperty.Register(
        nameof(HorizontalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingScrollMode.Auto, OnHorizontalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollRailMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HorizontalScrollRailModeProperty = DependencyProperty.Register(
        nameof(HorizontalScrollRailMode),
        typeof(ScrollingRailMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingRailMode.Enabled, OnHorizontalScrollRailModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="IgnoredInputKinds"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IgnoredInputKindsProperty = DependencyProperty.Register(
        nameof(IgnoredInputKinds),
        typeof(ScrollingInputKinds),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingInputKinds.None, OnIgnoredInputKindsPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="MaxZoomFactor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaxZoomFactorProperty = DependencyProperty.Register(
        nameof(MaxZoomFactor),
        typeof(double),
        typeof(ScrollPresenter),
        new PropertyMetadata(10d, OnMaxZoomFactorPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="MinZoomFactor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinZoomFactorProperty = DependencyProperty.Register(
        nameof(MinZoomFactor),
        typeof(double),
        typeof(ScrollPresenter),
        new PropertyMetadata(0.1d, OnMinZoomFactorPropertyChanged));

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
    public static readonly DependencyProperty VerticalScrollChainModeProperty = DependencyProperty.Register(
        nameof(VerticalScrollChainMode),
        typeof(ScrollingChainMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingChainMode.Auto, OnVerticalScrollChainModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="VerticalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollModeProperty = DependencyProperty.Register(
        nameof(VerticalScrollMode),
        typeof(ScrollingScrollMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingScrollMode.Auto, OnVerticalScrollModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="VerticalScrollRailMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollRailModeProperty = DependencyProperty.Register(
        nameof(VerticalScrollRailMode),
        typeof(ScrollingRailMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingRailMode.Enabled, OnVerticalScrollRailModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ZoomChainMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ZoomChainModeProperty = DependencyProperty.Register(
        nameof(ZoomChainMode),
        typeof(ScrollingChainMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingChainMode.Auto, OnZoomChainModePropertyChanged));

    /// <summary>
    /// Identifies the <see cref="ZoomMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ZoomModeProperty = DependencyProperty.Register(
        nameof(ZoomMode),
        typeof(ScrollingZoomMode),
        typeof(ScrollPresenter),
        new PropertyMetadata(ScrollingZoomMode.Disabled, OnZoomModePropertyChanged));

    private const float c_scrollPresenterDefaultInertiaDecayRate = 0.95f;

    /// <summary>
    /// Number of pixels scrolled when the automation peer requests a line-type change.
    /// </summary>
    private const double c_scrollPresenterLineDelta = 16;

    private const uint E_ACCESSDENIED = 0x80070005;

    private const string s_extentSourcePropertyName = "Extent";

    private const string s_maxOffsetPropertyName = "MaxOffset";

    private const string s_maxPositionSourcePropertyName = "MaxPosition";

    private const string s_minOffsetPropertyName = "MinOffset";

    private const string s_minPositionSourcePropertyName = "MinPosition";

    private const string s_multiplierPropertyName = "Multiplier";

    private const int s_noOpCorrelationId = -1;

    private const string s_offsetPropertyName = "Offset";

    private const int s_offsetsChangeMaxMs = 1000;
    private const int s_offsetsChangeMinMs = 50;
    private const int s_offsetsChangeMsPerUnit = 5;
    private const string s_offsetSourcePropertyName = "Offset";

    private const string s_positionSourcePropertyName = "Position";

    private const string s_scalePropertyName = "Scale";

    /// <summary>
    /// Number of ticks ellapsed before restarting the Translation and Scale animations to allow the Content
    /// rasterization to be triggered after the Idle State is reached or a zoom factor change operation completed.
    /// </summary>
    private const int s_translationAndZoomFactorAnimationsRestartTicks = 4;

    private const string s_translationPropertyName = "Translation";

    private const string s_viewportSourcePropertyName = "Viewport";

    private const int s_zoomFactorChangeMaxMs = 1000;
    private const int s_zoomFactorChangeMinMs = 50;
    private const int s_zoomFactorChangeMsPerUnit = 250;
    private const string s_zoomFactorSourcePropertyName = "ZoomFactor";

    private List<UIElement> m_anchorCandidates;

    private UIElement m_anchorElement;

    private Rect m_anchorElementBounds;

    private ScrollingAnchorRequestedEventArgs m_anchorRequestedEventArgs;

    private float m_animationRestartZoomFactor = 1;

    private Size m_availableSize;

    private long m_contentHeightChangedRevoker;
    private long m_contentHorizontalAlignmentChangedRevoker;
    private float m_contentLayoutOffsetX;

    private float m_contentLayoutOffsetY;

    private long m_contentMaxHeightChangedRevoker;
    private long m_contentMaxWidthChangedRevoker;
    private long m_contentMinHeightChangedRevoker;
    private long m_contentMinWidthChangedRevoker;
    private ScrollingContentOrientation m_contentOrientation = ScrollingContentOrientation.Both;

    private long m_contentVerticalAlignmentChangedRevoker;
    private long m_contentWidthChangedRevoker;
    private Vector2 m_endOfInertiaPosition;

    private float m_endOfInertiaZoomFactor = 1;

    private CompositionPropertySet m_expressionAnimationSources;

    private long m_flowDirectionChangedRevoker;

    private IScrollController m_horizontalScrollController;

    private CompositionPropertySet m_horizontalScrollControllerExpressionAnimationSources;

    private ExpressionAnimation m_horizontalScrollControllerMaxOffsetExpressionAnimation;

    private ExpressionAnimation m_horizontalScrollControllerOffsetExpressionAnimation;

    private IScrollControllerPanningInfo m_horizontalScrollControllerPanningInfo;

    private VisualInteractionSource m_horizontalScrollControllerVisualInteractionSource;

    private IList<ScrollSnapPointBase> m_horizontalSnapPoints;
    private bool m_horizontalSnapPointsNeedViewportUpdates;
    private InteractionTracker? m_interactionTracker;

    private List<InteractionTrackerAsyncOperation> m_interactionTrackerAsyncOperations = new List<InteractionTrackerAsyncOperation>();

    private IInteractionTrackerOwner m_interactionTrackerOwner;

    /// <summary>
    /// False when m_anchorElement is up-to-date, True otherwise.
    /// </summary>
    private bool m_isAnchorElementDirty = true;

    private InteractionTrackerAsyncOperationType m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.None;

    private int m_latestInteractionTrackerRequest;

    private int m_latestViewChangeCorrelationId;

    private ExpressionAnimation m_maxPositionExpressionAnimation;

    private ExpressionAnimation m_maxPositionSourceExpressionAnimation;

    private ExpressionAnimation m_minPositionExpressionAnimation;

    private ExpressionAnimation m_minPositionSourceExpressionAnimation;

    private object? m_pointerPressedEventHandler;

    private ExpressionAnimation m_positionSourceExpressionAnimation;

    private VisualInteractionSource m_scrollPresenterVisualInteractionSource;

    private HashSet<SnapPointWrapper<ScrollSnapPointBase>> m_sortedConsolidatedHorizontalSnapPoints = new HashSet<SnapPointWrapper<ScrollSnapPointBase>>();

    private HashSet<SnapPointWrapper<ScrollSnapPointBase>> m_sortedConsolidatedVerticalSnapPoints = new HashSet<SnapPointWrapper<ScrollSnapPointBase>>();

    private HashSet<SnapPointWrapper<ZoomSnapPointBase>> m_sortedConsolidatedZoomSnapPoints = new HashSet<SnapPointWrapper<ZoomSnapPointBase>>();
    private ScrollingInteractionState m_state = ScrollingInteractionState.Idle;

    private short m_translationAndZoomFactorAnimationsRestartTicksCountdown;

    private ExpressionAnimation m_translationExpressionAnimation;

    private double m_unzoomedExtentHeight;

    private double m_unzoomedExtentWidth;

    private IScrollController m_verticalScrollController;

    private CompositionPropertySet m_verticalScrollControllerExpressionAnimationSources;

    private ExpressionAnimation m_verticalScrollControllerMaxOffsetExpressionAnimation;

    private ExpressionAnimation m_verticalScrollControllerOffsetExpressionAnimation;

    private IScrollControllerPanningInfo m_verticalScrollControllerPanningInfo;

    private VisualInteractionSource m_verticalScrollControllerVisualInteractionSource;

    private IList<ScrollSnapPointBase> m_verticalSnapPoints;
    private bool m_verticalSnapPointsNeedViewportUpdates;
    private double m_viewportHeight;

    private double m_viewportWidth;

    private double m_zoomedHorizontalOffset;

    private double m_zoomedVerticalOffset;

    private float m_zoomFactor = 1;

    private ExpressionAnimation m_zoomFactorExpressionAnimation;

    private ExpressionAnimation m_zoomFactorSourceExpressionAnimation;

    private IList<ZoomSnapPointBase>? m_zoomSnapPoints;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollPresenter"/> class.
    /// </summary>
    public ScrollPresenter()
    {
        UIElement.RegisterAsScrollPort(this);

        HookScrollPresenterEvents();

        // Set the default Transparent background so that hit-testing allows to start a touch manipulation
        // outside the boundaries of the Content, when it's smaller than the ScrollPresenter.
        Background = (new SolidColorBrush(Colors.Transparent));
    }

    private delegate void PostArrangeEventHandler(object sender);

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
    public event TypedEventHandler<ScrollPresenter, object?>? StateChanged;

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

    private event PostArrangeEventHandler m_postArrange;

    /// <summary>
    /// Gets or sets a brush that provides the background of the <see cref="ScrollPresenter"/>.
    /// </summary>
    public new Brush Background
    {
        get
        {
            return (Brush)GetValue(BackgroundProperty);
        }
        set
        {
            SetValue(BackgroundProperty, value);
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
    public UIElement? CurrentAnchor
    {
        get
        {
            return AnchorElement;
        }
    }

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
    public double HorizontalOffset => m_zoomedHorizontalOffset;

    /// <summary>
    /// Gets or sets a value that indicates whether or not to chain horizontal scrolling to an outer scroll control.
    /// </summary>
    public ScrollingChainMode HorizontalScrollChainMode
    {
        get => (ScrollingChainMode)GetValue(HorizontalScrollChainModeProperty);
        set => SetValue(HorizontalScrollChainModeProperty, value);
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
            if (m_horizontalScrollController is not null)
            {
                UnhookHorizontalScrollControllerEvents();

                if (m_horizontalScrollControllerPanningInfo is not null)
                {
                    UnhookHorizontalScrollControllerPanningInfoEvents();

                    if (m_horizontalScrollControllerExpressionAnimationSources is not null)
                    {
                        m_horizontalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                            null /*scrollControllerExpressionAnimationSources*/,
                            s_minOffsetPropertyName,
                            s_maxOffsetPropertyName,
                            s_offsetPropertyName,
                            s_multiplierPropertyName);
                    }
                }
            }

            m_horizontalScrollController = (value);
            m_horizontalScrollControllerPanningInfo = (value is not null ? value.PanningInfo : null);

            if (m_interactionTracker is not null)
            {
                SetupScrollControllerVisualInterationSource(ScrollPresenterDimension.HorizontalScroll);
            }

            if (m_horizontalScrollController is not null)
            {
                HookHorizontalScrollControllerEvents(
                    m_horizontalScrollController);

                if (m_horizontalScrollControllerPanningInfo is not null)
                {
                    HookHorizontalScrollControllerPanningInfoEvents(
                        m_horizontalScrollControllerPanningInfo,
                        m_horizontalScrollControllerVisualInteractionSource != null /*hasInteractionSource*/);
                }

                UpdateScrollControllerValues(ScrollPresenterDimension.HorizontalScroll);
                UpdateScrollControllerIsScrollable(ScrollPresenterDimension.HorizontalScroll);

                if (m_horizontalScrollControllerPanningInfo is not null && m_horizontalScrollControllerExpressionAnimationSources is not null)
                {
                    m_horizontalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                        m_horizontalScrollControllerExpressionAnimationSources,
                        s_minOffsetPropertyName,
                        s_maxOffsetPropertyName,
                        s_offsetPropertyName,
                        s_multiplierPropertyName);
                }
            }
        }
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
        get => (ScrollingInputKinds)GetValue(IgnoredInputKindsProperty);
        set => SetValue(IgnoredInputKindsProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum value for the read-only <see cref="ZoomFactor"/> property.
    /// </summary>
    public double MaxZoomFactor
    {
        get
        {
            return (double)GetValue(MaxZoomFactorProperty);
        }
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
        get
        {
            return (double)GetValue(MinZoomFactorProperty);
        }
        set
        {
            ValidateZoomFactoryBoundary(value);
            SetValue(MinZoomFactorProperty, value);
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
            return (ScrollingChainMode)GetValue(VerticalScrollChainModeProperty);
        }
        set
        {
            SetValue(VerticalScrollChainModeProperty, value);
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
            if (m_verticalScrollController is not null)
            {
                UnhookVerticalScrollControllerEvents();
            }

            if (m_verticalScrollControllerPanningInfo is not null)
            {
                UnhookVerticalScrollControllerPanningInfoEvents();

                if (m_verticalScrollControllerExpressionAnimationSources is not null)
                {
                    m_verticalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                        null /*scrollControllerExpressionAnimationSources*/,
                        s_minOffsetPropertyName,
                        s_maxOffsetPropertyName,
                        s_offsetPropertyName,
                        s_multiplierPropertyName);
                }
            }

            m_verticalScrollController = (value);
            m_verticalScrollControllerPanningInfo = (value is not null ? value.PanningInfo : null);

            if (m_interactionTracker is not null)
            {
                SetupScrollControllerVisualInterationSource(ScrollPresenterDimension.VerticalScroll);
            }

            if (m_verticalScrollController is not null)
            {
                HookVerticalScrollControllerEvents(
                    m_verticalScrollController);

                if (m_verticalScrollControllerPanningInfo is not null)
                {
                    HookVerticalScrollControllerPanningInfoEvents(
                        m_verticalScrollControllerPanningInfo,
                        m_verticalScrollControllerVisualInteractionSource != null /*hasInteractionSource*/);
                }

                UpdateScrollControllerValues(ScrollPresenterDimension.VerticalScroll);
                UpdateScrollControllerIsScrollable(ScrollPresenterDimension.VerticalScroll);

                if (m_verticalScrollControllerPanningInfo is not null && m_verticalScrollControllerExpressionAnimationSources is not null)
                {
                    m_verticalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                        m_verticalScrollControllerExpressionAnimationSources,
                        s_minOffsetPropertyName,
                        s_maxOffsetPropertyName,
                        s_offsetPropertyName,
                        s_multiplierPropertyName);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that determines how manipulation input influences scrolling behavior on the vertical axis.
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
        get => (ScrollingChainMode)GetValue(ZoomChainModeProperty);
        set => SetValue(ZoomChainModeProperty, value);
    }

    /// <summary>
    /// Gets a value that indicates the amount of scaling currently applied to content.
    /// </summary>
    public float ZoomFactor => m_zoomFactor;

    /// <summary>
    /// Gets or sets a value that indicates the ability to zoom in and out by means of user input.
    /// </summary>
    public ScrollingZoomMode ZoomMode
    {
        get => (ScrollingZoomMode)GetValue(ZoomModeProperty);
        set => SetValue(ZoomModeProperty, value);
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

    private UIElement AnchorElement
    {
        get
        {
            bool isAnchoringElementHorizontally = false;
            bool isAnchoringElementVertically = false;

            IsAnchoring(out isAnchoringElementHorizontally, out isAnchoringElementVertically, out _, out _);

            if (isAnchoringElementHorizontally || isAnchoringElementVertically)
            {
                EnsureAnchorElementSelection();
            }

            var value = m_anchorElement;

            return value;
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

    internal double GetZoomedExtentHeight()
    {
        return m_unzoomedExtentHeight * m_zoomFactor;
    }

    internal double GetZoomedExtentWidth()
    {
        return m_unzoomedExtentWidth * m_zoomFactor;
    }

    internal void LineDown()
    {
        ScrollToVerticalOffset(m_zoomedVerticalOffset + c_scrollPresenterLineDelta);
    }

    internal void LineLeft()
    {
        ScrollToHorizontalOffset(m_zoomedHorizontalOffset - c_scrollPresenterLineDelta);
    }

    internal void LineRight()
    {
        ScrollToHorizontalOffset(m_zoomedHorizontalOffset + c_scrollPresenterLineDelta);
    }

    internal void LineUp()
    {
        ScrollToVerticalOffset(m_zoomedVerticalOffset - c_scrollPresenterLineDelta);
    }

    internal void PageDown()
    {
        ScrollToVerticalOffset(m_zoomedVerticalOffset + ViewportHeight);
    }

    internal void PageLeft()
    {
        ScrollToHorizontalOffset(m_zoomedHorizontalOffset - ViewportWidth);
    }

    internal void PageRight()
    {
        ScrollToHorizontalOffset(m_zoomedHorizontalOffset + ViewportWidth);
    }

    internal void PageUp()
    {
        ScrollToVerticalOffset(m_zoomedVerticalOffset - ViewportHeight);
    }

    internal void ScrollToHorizontalOffset(double offset)
    {
        ScrollToOffsets(offset /*zoomedHorizontalOffset*/, m_zoomedVerticalOffset /*zoomedVerticalOffset*/);
    }

    internal void ScrollToOffsets(double horizontalOffset, double verticalOffset)
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

    internal void ScrollToVerticalOffset(double offset)
    {
        ScrollToOffsets(m_zoomedHorizontalOffset /*zoomedHorizontalOffset*/, offset /*zoomedVerticalOffset*/);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        UIElement content = Content;
        Rect finalContentRect = default;

        // Possible cases:
        // 1. m_availableSize is infinite, the ScrollPresenter is not constrained and takes its Content DesiredSize.
        //    viewport thus is finalSize.
        // 2. m_availableSize > finalSize, the ScrollPresenter is constrained and its Content is smaller than the available size.
        //    No matter the ScrollPresenter's alignment, it does not grow larger than finalSize. viewport is finalSize again.
        // 3. m_availableSize <= finalSize, the ScrollPresenter is constrained and its Content is larger than or equal to
        //    the available size. viewport is the smaller & constrained m_availableSize.
        Size viewport =
         new Size(
        Math.Min(finalSize.Width, m_availableSize.Width),
        Math.Min(finalSize.Height, m_availableSize.Height));

        bool renderSizeChanged = false;
        double newUnzoomedExtentWidth = 0.0;
        double newUnzoomedExtentHeight = 0.0;

        if (content is not null)
        {
            float contentLayoutOffsetXDelta = 0.0f;
            float contentLayoutOffsetYDelta = 0.0f;
            bool isAnchoringElementHorizontally = false;
            bool isAnchoringElementVertically = false;
            bool isAnchoringFarEdgeHorizontally = false;
            bool isAnchoringFarEdgeVertically = false;
            Size oldRenderSize = content.RenderSize;
            Size contentArrangeSize = content.DesiredSize;

            FrameworkElement contentAsFE = content as FrameworkElement;

            Thickness contentMargin = contentAsFE is not null ? contentAsFE.Margin : new Thickness(0);

            bool wasContentArrangeWidthStretched = contentAsFE is not null &&
                    contentAsFE.HorizontalAlignment == HorizontalAlignment.Stretch &&
                    double.IsNaN(contentAsFE.Width) &&
                    contentArrangeSize.Width < viewport.Width;

            bool wasContentArrangeHeightStretched = contentAsFE is not null &&
                    contentAsFE.VerticalAlignment == VerticalAlignment.Stretch &&
                    double.IsNaN(contentAsFE.Height) &&
                    contentArrangeSize.Height < viewport.Height;

            if (wasContentArrangeWidthStretched)
            {
                // Allow the content to stretch up to the larger viewport width.
                contentArrangeSize.Width = viewport.Width;
            }

            if (wasContentArrangeHeightStretched)
            {
                // Allow the content to stretch up to the larger viewport height.
                contentArrangeSize.Height = viewport.Height;
            }

            finalContentRect = new Rect(
                m_contentLayoutOffsetX,
            m_contentLayoutOffsetY,
            contentArrangeSize.Width,
            contentArrangeSize.Height
);

            IsAnchoring(out isAnchoringElementHorizontally, out isAnchoringElementVertically, out isAnchoringFarEdgeHorizontally, out isAnchoringFarEdgeVertically);

            Debug.Assert(!(isAnchoringElementHorizontally && isAnchoringFarEdgeHorizontally));
            Debug.Assert(!(isAnchoringElementVertically && isAnchoringFarEdgeVertically));

            if (isAnchoringElementHorizontally || isAnchoringElementVertically || isAnchoringFarEdgeHorizontally || isAnchoringFarEdgeVertically)
            {
                Debug.Assert(m_interactionTracker is not null);

                Size preArrangeViewportToElementAnchorPointsDistance = new Size(float.NaN, float.NaN);

                if (isAnchoringElementHorizontally || isAnchoringElementVertically)
                {
                    EnsureAnchorElementSelection();
                    preArrangeViewportToElementAnchorPointsDistance = ComputeViewportToElementAnchorPointsDistance(
                        m_viewportWidth,
                        m_viewportHeight,
                        true /*isForPreArrange*/);
                }
                else
                {
                    ResetAnchorElement();
                }

                contentArrangeSize = ArrangeContent(
                    content,
                    contentMargin,
                    finalContentRect,
                    wasContentArrangeWidthStretched,
                    wasContentArrangeHeightStretched);

                if (!double.IsNaN(preArrangeViewportToElementAnchorPointsDistance.Width) || !double.IsNaN(preArrangeViewportToElementAnchorPointsDistance.Height))
                {
                    // Using the new viewport sizes to handle the cases where an adjustment needs to be performed because of a ScrollPresenter size change.
                    Size postArrangeViewportToElementAnchorPointsDistance = ComputeViewportToElementAnchorPointsDistance(
                        viewport.Width /*viewportWidth*/,
                        viewport.Height /*viewportHeight*/,
                        false /*isForPreArrange*/);

                    if (isAnchoringElementHorizontally &&
                        !double.IsNaN(preArrangeViewportToElementAnchorPointsDistance.Width) &&
                        !double.IsNaN(postArrangeViewportToElementAnchorPointsDistance.Width) &&
                        preArrangeViewportToElementAnchorPointsDistance.Width != postArrangeViewportToElementAnchorPointsDistance.Width)
                    {
                        // Perform horizontal offset adjustment due to element anchoring
                        contentLayoutOffsetXDelta = ComputeContentLayoutOffsetDelta(
                            ScrollPresenterDimension.HorizontalScroll,
                            (float)(postArrangeViewportToElementAnchorPointsDistance.Width - preArrangeViewportToElementAnchorPointsDistance.Width) /*unzoomedDelta*/);
                    }

                    if (isAnchoringElementVertically &&
                        !double.IsNaN(preArrangeViewportToElementAnchorPointsDistance.Height) &&
                        !double.IsNaN(postArrangeViewportToElementAnchorPointsDistance.Height) &&
                        preArrangeViewportToElementAnchorPointsDistance.Height != postArrangeViewportToElementAnchorPointsDistance.Height)
                    {
                        // Perform vertical offset adjustment due to element anchoring
                        contentLayoutOffsetYDelta = ComputeContentLayoutOffsetDelta(
                            ScrollPresenterDimension.VerticalScroll,
                            (float)(postArrangeViewportToElementAnchorPointsDistance.Height - preArrangeViewportToElementAnchorPointsDistance.Height) /*unzoomedDelta*/);
                    }
                }
            }
            else
            {
                ResetAnchorElement();

                contentArrangeSize = ArrangeContent(
                    content,
                    contentMargin,
                    finalContentRect,
                    wasContentArrangeWidthStretched,
                    wasContentArrangeHeightStretched);
            }

            newUnzoomedExtentWidth = contentArrangeSize.Width;
            newUnzoomedExtentHeight = contentArrangeSize.Height;

            double maxUnzoomedExtentWidth = double.PositiveInfinity;
            double maxUnzoomedExtentHeight = double.PositiveInfinity;

            if (contentAsFE is not null)
            {
                // Determine the maximum size directly set on the content, if any.
                maxUnzoomedExtentWidth = GetComputedMaxWidth(maxUnzoomedExtentWidth, contentAsFE);
                maxUnzoomedExtentHeight = GetComputedMaxHeight(maxUnzoomedExtentHeight, contentAsFE);
            }

            // Take into account the actual resulting rendering size, in case it's larger than the desired size.
            // But the extent must not exceed the size explicitly set on the content, if any.
            newUnzoomedExtentWidth = Math.Max(
                newUnzoomedExtentWidth,
                Math.Max(0.0, content.RenderSize.Width + contentMargin.Left + contentMargin.Right));
            newUnzoomedExtentWidth = Math.Min(
                newUnzoomedExtentWidth,
                maxUnzoomedExtentWidth);

            newUnzoomedExtentHeight = Math.Max(
                newUnzoomedExtentHeight,
                Math.Max(0.0, content.RenderSize.Height + contentMargin.Top + contentMargin.Bottom));
            newUnzoomedExtentHeight = Math.Min(
                newUnzoomedExtentHeight,
                maxUnzoomedExtentHeight);

            if (isAnchoringFarEdgeHorizontally)
            {
                float unzoomedDelta = 0.0f;

                if (newUnzoomedExtentWidth > m_unzoomedExtentWidth ||                                 // ExtentWidth grew
                    m_zoomedHorizontalOffset + m_viewportWidth > m_zoomFactor * m_unzoomedExtentWidth) // ExtentWidth shrank while overpanning
                {
                    // Perform horizontal offset adjustment due to edge anchoring
                    unzoomedDelta = (float)(newUnzoomedExtentWidth - m_unzoomedExtentWidth);
                }

                if ((float)(m_viewportWidth) > viewport.Width)
                {
                    // Viewport width shrank: Perform horizontal offset adjustment due to edge anchoring
                    unzoomedDelta += (float)(((float)(m_viewportWidth) - viewport.Width) / m_zoomFactor);
                }

                if (unzoomedDelta != 0.0f)
                {
                    Debug.Assert(contentLayoutOffsetXDelta == 0.0f);
                    contentLayoutOffsetXDelta = ComputeContentLayoutOffsetDelta(ScrollPresenterDimension.HorizontalScroll, unzoomedDelta);
                }
            }

            if (isAnchoringFarEdgeVertically)
            {
                float unzoomedDelta = 0.0f;

                if (newUnzoomedExtentHeight > m_unzoomedExtentHeight ||                               // ExtentHeight grew
                    m_zoomedVerticalOffset + m_viewportHeight > m_zoomFactor * m_unzoomedExtentHeight) // ExtentHeight shrank while overpanning
                {
                    // Perform vertical offset adjustment due to edge anchoring
                    unzoomedDelta = (float)(newUnzoomedExtentHeight - m_unzoomedExtentHeight);
                }

                if ((float)(m_viewportHeight) > viewport.Height)
                {
                    // Viewport height shrank: Perform vertical offset adjustment due to edge anchoring
                    unzoomedDelta += (float)(((float)(m_viewportHeight) - viewport.Height) / m_zoomFactor);
                }

                if (unzoomedDelta != 0.0f)
                {
                    Debug.Assert(contentLayoutOffsetYDelta == 0.0f);
                    contentLayoutOffsetYDelta = ComputeContentLayoutOffsetDelta(ScrollPresenterDimension.VerticalScroll, unzoomedDelta);
                }
            }

            if (contentLayoutOffsetXDelta != 0.0f || contentLayoutOffsetYDelta != 0.0f)
            {
                Rect contentRectWithDelta = new Rect(
                m_contentLayoutOffsetX + contentLayoutOffsetXDelta,
                m_contentLayoutOffsetY + contentLayoutOffsetYDelta,
                contentArrangeSize.Width,
                contentArrangeSize.Height
            );

                content.Arrange(contentRectWithDelta);

                if (contentLayoutOffsetXDelta != 0.0f)
                {
                    m_contentLayoutOffsetX += contentLayoutOffsetXDelta;
                    UpdateOffset(ScrollPresenterDimension.HorizontalScroll, m_zoomedHorizontalOffset - contentLayoutOffsetXDelta);
                    OnContentLayoutOffsetChanged(ScrollPresenterDimension.HorizontalScroll);
                }

                if (contentLayoutOffsetYDelta != 0.0f)
                {
                    m_contentLayoutOffsetY += contentLayoutOffsetYDelta;
                    UpdateOffset(ScrollPresenterDimension.VerticalScroll, m_zoomedVerticalOffset - contentLayoutOffsetYDelta);
                    OnContentLayoutOffsetChanged(ScrollPresenterDimension.VerticalScroll);
                }

                OnViewChanged(contentLayoutOffsetXDelta != 0.0f /*horizontalOffsetChanged*/, contentLayoutOffsetYDelta != 0.0f /*verticalOffsetChanged*/);
            }

            renderSizeChanged = content.RenderSize != oldRenderSize;
        }

        // Set a rectangular clip on this ScrollPresenter the same size as the arrange
        // rectangle so the content does not render beyond it.
        var rectangleGeometry = Clip as RectangleGeometry;

        if (rectangleGeometry is null)
        {
            // Ensure that this ScrollPresenter has a rectangular clip.
            RectangleGeometry newRectangleGeometry = new RectangleGeometry();
            Clip = (newRectangleGeometry);

            rectangleGeometry = newRectangleGeometry;
        }

        Rect newClipRect = new Rect(0.0f, 0.0f, viewport.Width, viewport.Height);
        rectangleGeometry.Rect = (newClipRect);

        UpdateUnzoomedExtentAndViewport(
            renderSizeChanged,
            newUnzoomedExtentWidth  /*unzoomedExtentWidth*/,
            newUnzoomedExtentHeight /*unzoomedExtentHeight*/,
            viewport.Width          /*viewportWidth*/,
            viewport.Height         /*viewportHeight*/);

        m_isAnchorElementDirty = true;
        return viewport;
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

    private static double ComputeZoomedOffsetWithMinimalChange(
        double viewportStart,
        double viewportEnd,
        double childStart,
        double childEnd)
    {
        bool above = childStart < viewportStart && childEnd < viewportEnd;
        bool below = childEnd > viewportEnd && childStart > viewportStart;
        bool larger = (childEnd - childStart) > (viewportEnd - viewportStart);

        // # CHILD POSITION   CHILD SIZE   SCROLL   REMEDY
        // 1 Above viewport   <= viewport  Down     Align top edge of content & viewport
        // 2 Above viewport   >  viewport  Down     Align bottom edge of content & viewport
        // 3 Below viewport   <= viewport  Up       Align bottom edge of content & viewport
        // 4 Below viewport   >  viewport  Up       Align top edge of content & viewport
        // 5 Entirely within viewport      NA       No change
        // 6 Spanning viewport             NA       No change
        if ((above && !larger) || (below && larger))
        {
            // Cases 1 & 4
            return childStart;
        }
        else if (above || below)
        {
            // Cases 2 & 3
            return childEnd - viewportEnd + viewportStart;
        }

        // cases 5 & 6
        return viewportStart;
    }

    private static ScrollingAnimationMode GetComputedAnimationMode(
        ScrollingAnimationMode animationMode)
    {
        if (animationMode == ScrollingAnimationMode.Auto)
        {
            bool isAnimationsEnabled = new Func<bool>(() =>
            {
                var globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();

                if (globalTestHooks is not null && ScrollPresenterTestHooks.IsAnimationsEnabledOverride() is true)
                {
                    return ScrollPresenterTestHooks.IsAnimationsEnabledOverride().Value;
                }
                else
                {
                    return SharedHelpers.IsAnimationsEnabled();
                }
            })();

            return isAnimationsEnabled ? ScrollingAnimationMode.Enabled : ScrollingAnimationMode.Disabled;
        }

        return animationMode;
    }

    private static Rect GetDescendantBounds(
        UIElement content,
        UIElement descendant,
        Rect descendantRect)
    {
        Debug.Assert(content is not null);

        FrameworkElement contentAsFE = content as FrameworkElement;
        GeneralTransform transform = descendant.TransformToVisual(content);
        Thickness contentMargin = default;

        if (contentAsFE is not null)
        {
            contentMargin = contentAsFE.Margin;
        }

        return transform.TransformBounds(new Rect(
            (float)(contentMargin.Left + descendantRect.X),
        (float)(contentMargin.Top + descendantRect.Y),
        descendantRect.Width,
        descendantRect.Height));
    }

    private static Rect GetDescendantBounds(
        UIElement content,
        UIElement descendant)
    {
        Debug.Assert(content is not null);
        Debug.Assert(IsElementValidAnchor(descendant, content));

        FrameworkElement descendantAsFE = descendant as FrameworkElement;
        Rect descendantRect = new Rect(
            0.0f,
        0.0f,
        descendantAsFE is not null ? (float)(descendantAsFE.ActualWidth) : 0.0f,
        descendantAsFE is not null ? (float)(descendantAsFE.ActualHeight) : 0.0f
        );

        return GetDescendantBounds(content, descendant, descendantRect);
    }

    private static string GetVisualTargetedPropertyName(ScrollPresenterDimension dimension)
    {
        switch (dimension)
        {
            case ScrollPresenterDimension.Scroll:
                return s_translationPropertyName;

            default:
                Debug.Assert(dimension == ScrollPresenterDimension.ZoomFactor);
                return s_scalePropertyName;
        }
    }

    private static InteractionChainingMode InteractionChainingModeFromChainingMode(
        ScrollingChainMode chainingMode)
    {
        switch (chainingMode)
        {
            case ScrollingChainMode.Always:
                return InteractionChainingMode.Always;

            case ScrollingChainMode.Auto:
                return InteractionChainingMode.Auto;

            default:
                return InteractionChainingMode.Never;
        }
    }

    private static InteractionSourceMode InteractionSourceModeFromScrollMode(ScrollingScrollMode scrollMode)
    {
        return scrollMode == ScrollingScrollMode.Enabled ? InteractionSourceMode.EnabledWithInertia : InteractionSourceMode.Disabled;
    }

    private static InteractionSourceMode InteractionSourceModeFromZoomMode(ScrollingZoomMode zoomMode)
    {
        return zoomMode == ScrollingZoomMode.Enabled ? InteractionSourceMode.EnabledWithInertia : InteractionSourceMode.Disabled;
    }

    private static bool IsAnchorRatioValid(double value)
    {
        return double.IsNaN(value) || (!double.IsInfinity(value) && value >= 0 && value <= 1);
    }

    private static bool IsElementValidAnchor(
        UIElement element,
        UIElement content)
    {
        Debug.Assert(element is not null);
        Debug.Assert(content is not null);

        return element.Visibility == Visibility.Visible && (element == content || SharedHelpers.IsAncestor(element, content));
    }

    private static bool IsZoomFactorBoundaryValid(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static void OnBackgroundPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnComputedHorizontalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnComputedVerticalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnContentOrientationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnContentPropertyChangedX(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalAnchorRatioPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;

        var value = (double)args.NewValue;
        ValidateAnchorRatio(value);
        if (double.IsNaN(value))
        {
            sender.SetValue(args.Property, 0d);
            return;
        }

        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalScrollChainModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnHorizontalScrollRailModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnIgnoredInputKindsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnMaxZoomFactorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;

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
        var owner = (ScrollPresenter)sender;

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
        var owner = (ScrollPresenter)sender;

        var value = (double)args.NewValue;
        ValidateAnchorRatio(value);
        if (double.IsNaN(value))
        {
            sender.SetValue(args.Property, 0d);
            return;
        }

        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalScrollChainModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalScrollModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnVerticalScrollRailModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnZoomChainModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private static void OnZoomModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (ScrollPresenter)sender;
        owner.OnPropertyChanged(args);
    }

    private Size ArrangeContent(
        UIElement content,
        Thickness contentMargin,
        Rect finalContentRect,
        bool wasContentArrangeWidthStretched,
        bool wasContentArrangeHeightStretched)
    {
        Debug.Assert(content is not null);

        Size contentArrangeSize =
        new Size(
        finalContentRect.Width,
        finalContentRect.Height
    );

        content.Arrange(finalContentRect);

        if (wasContentArrangeWidthStretched || wasContentArrangeHeightStretched)
        {
            bool reArrangeNeeded = false;
            var renderWidth = content.RenderSize.Width;
            var renderHeight = content.RenderSize.Height;
            var marginWidth = (float)(contentMargin.Left + contentMargin.Right);
            var marginHeight = (float)(contentMargin.Top + contentMargin.Bottom);
            var scaleFactorRounding = 0.5f / (float)(XamlRoot.RasterizationScale);

            if (wasContentArrangeWidthStretched &&
                renderWidth > 0.0f &&
                renderWidth + marginWidth < finalContentRect.Width * (1.0f - float.Epsilon) - scaleFactorRounding)
            {
                // Content stretched partially horizontally.
                contentArrangeSize.Width = finalContentRect.Width = renderWidth + marginWidth;
                reArrangeNeeded = true;
            }

            if (wasContentArrangeHeightStretched &&
                renderHeight > 0.0f &&
                renderHeight + marginHeight < finalContentRect.Height * (1.0f - float.Epsilon) - scaleFactorRounding)
            {
                // Content stretched partially vertically.
                contentArrangeSize.Height = finalContentRect.Height = renderHeight + marginHeight;
                reArrangeNeeded = true;
            }

            if (reArrangeNeeded)
            {
                content.Arrange(finalContentRect);
            }
        }

        return contentArrangeSize;
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
        viewChangeCorrelationId = s_noOpCorrelationId;

        ScrollingAnimationMode animationMode = options is not null ? options.AnimationMode : ScrollingAnimationMode.Auto;
        ScrollingSnapPointsMode snapPointsMode = options is not null ? options.SnapPointsMode : ScrollingSnapPointsMode.Default;
        InteractionTrackerAsyncOperationType operationType = default;

        animationMode = GetComputedAnimationMode(animationMode);

        switch (animationMode)
        {
            case ScrollingAnimationMode.Disabled:
                {
                    switch (offsetsKind)
                    {
                        case ScrollPresenterViewKind.Absolute:
                            {
                                operationType = InteractionTrackerAsyncOperationType.TryUpdatePosition;
                                break;
                            }
                        case ScrollPresenterViewKind.RelativeToCurrentView:
                            {
                                operationType = InteractionTrackerAsyncOperationType.TryUpdatePositionBy;
                                break;
                            }
                    }
                    break;
                }
            case ScrollingAnimationMode.Enabled:
                {
                    switch (offsetsKind)
                    {
                        case ScrollPresenterViewKind.Absolute:
                        case ScrollPresenterViewKind.RelativeToCurrentView:
                            {
                                operationType = InteractionTrackerAsyncOperationType.TryUpdatePositionWithAnimation;
                                break;
                            }
                    }
                    break;
                }
        }

        if (Content is null)
        {
            // When there is no content, skip the view change request and return -1, indicating that no action was taken.
            return;
        }

        // When the ScrollPresenter is not loaded or not set up yet, delay the offsets change request until it gets loaded.
        // OnCompositionTargetRendering will launch the delayed changes at that point.
        bool delayOperation = !IsLoadedAndSetUp();

        ScrollingScrollOptions optionsClone = null;

        // Clone the options for this request if needed. The clone or original options will be used if the operation ever gets processed.
        bool isScrollControllerRequest = (
            (char)(operationTrigger) &
            ((char)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest) |
                (char)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest))) > 0;

        if (options is not null && !isScrollControllerRequest)
        {
            // Options are cloned so that they can be modified by the caller after this offsets change call without affecting the outcome of the operation.
            optionsClone = new ScrollingScrollOptions(
                animationMode,
                snapPointsMode);
        }

        if (!delayOperation)
        {
            Debug.Assert(m_interactionTracker is not null);

            // Prevent any existing delayed operation from being processed after this request and overriding it.
            // All delayed operations are completed with the Interrupted result.
            CompleteDelayedOperations();

            HookCompositionTargetRendering();
        }

        ViewChange offsetsChange = null;

        if (((char)(operationTrigger) & (char)(InteractionTrackerAsyncOperationTrigger.BringIntoViewRequest)) > 0)
        {
            // Bring-into-view operations use a richer version of OffsetsChange which includes
            // information extracted from the BringIntoViewRequestedEventArgs instance.
            // This allows to use ComputeBringIntoViewUpdatedTargetOffsets just before invoking
            // the InteractionTracker's TryUpdatePosition.
            offsetsChange = new BringIntoViewOffsetsChange(
                this,
                zoomedHorizontalOffset,
                zoomedVerticalOffset,
                offsetsKind,
                optionsClone is not null ? optionsClone : (options),
                bringIntoViewRequestedEventArgs.TargetElement,
                bringIntoViewRequestedEventArgs.TargetRect,
                bringIntoViewRequestedEventArgs.HorizontalAlignmentRatio,
                bringIntoViewRequestedEventArgs.VerticalAlignmentRatio,
                bringIntoViewRequestedEventArgs.HorizontalOffset,
                bringIntoViewRequestedEventArgs.VerticalOffset);
        }
        else
        {
            offsetsChange = new OffsetsChange(
                zoomedHorizontalOffset,
                zoomedVerticalOffset,
                offsetsKind,
                optionsClone is not null ? optionsClone : options);
        }

        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = new InteractionTrackerAsyncOperation(
                operationType,
                operationTrigger,
                delayOperation,
                offsetsChange);

        if (operationTrigger != InteractionTrackerAsyncOperationTrigger.DirectViewChange)
        {
            // User-triggered or bring-into-view operations are processed as quickly as possible by minimizing their TicksCountDown
            int ticksCountdown = GetInteractionTrackerOperationsTicksCountdown();

            interactionTrackerAsyncOperation.SetTicksCountdown(Math.Max(1, ticksCountdown));
        }

        m_interactionTrackerAsyncOperations.Add(interactionTrackerAsyncOperation);

        if (viewChangeCorrelationId > 0)
        {
            if (existingViewChangeCorrelationId != s_noOpCorrelationId)
            {
                interactionTrackerAsyncOperation.SetViewChangeCorrelationId(existingViewChangeCorrelationId);
                viewChangeCorrelationId = existingViewChangeCorrelationId;
            }
            else
            {
                m_latestViewChangeCorrelationId = GetNextViewChangeCorrelationId();
                interactionTrackerAsyncOperation.SetViewChangeCorrelationId(m_latestViewChangeCorrelationId);
                viewChangeCorrelationId = m_latestViewChangeCorrelationId;
            }
        }
        else if (existingViewChangeCorrelationId != s_noOpCorrelationId)
        {
            interactionTrackerAsyncOperation.SetViewChangeCorrelationId(existingViewChangeCorrelationId);
        }
    }

    private void ChangeOffsetsWithAdditionalVelocityPrivate(
        Vector2 offsetsVelocity,
        Vector2 anticipatedOffsetsChange,
        Vector2? inertiaDecayRate,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        out int viewChangeCorrelationId)
    {
        viewChangeCorrelationId = s_noOpCorrelationId;

        if (Content is null)
        {
            // When there is no content, skip the view change request and return -1, indicating that no action was taken.
            return;
        }

        // When the ScrollPresenter is not loaded or not set up yet, delay the offsets change request until it gets loaded.
        // OnCompositionTargetRendering will launch the delayed changes at that point.
        bool delayOperation = !IsLoadedAndSetUp();

        ViewChangeBase offsetsChangeWithAdditionalVelocity =
            new OffsetsChangeWithAdditionalVelocity(
                offsetsVelocity, anticipatedOffsetsChange, inertiaDecayRate);

        if (!delayOperation)
        {
            Debug.Assert(m_interactionTracker is not null);

            // Prevent any existing delayed operation from being processed after this request and overriding it.
            // All delayed operations are completed with the Interrupted result.
            CompleteDelayedOperations();

            HookCompositionTargetRendering();
        }

        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = new InteractionTrackerAsyncOperation(
                InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity,
                operationTrigger,
                delayOperation,
                offsetsChangeWithAdditionalVelocity);

        if (operationTrigger != InteractionTrackerAsyncOperationTrigger.DirectViewChange)
        {
            // User-triggered or bring-into-view operations are processed as quickly as possible by minimizing their TicksCountDown
            int ticksCountdown = GetInteractionTrackerOperationsTicksCountdown();

            interactionTrackerAsyncOperation.SetTicksCountdown(Math.Max(1, ticksCountdown));
        }

        m_interactionTrackerAsyncOperations.Add(interactionTrackerAsyncOperation);

        if (viewChangeCorrelationId != 0)
        {
            m_latestViewChangeCorrelationId = GetNextViewChangeCorrelationId();
            interactionTrackerAsyncOperation.SetViewChangeCorrelationId(m_latestViewChangeCorrelationId);
            viewChangeCorrelationId = m_latestViewChangeCorrelationId;
        }
    }

    private void ChangeZoomFactorPrivate(
        float zoomFactor,
        Vector2? centerPoint,
        ScrollPresenterViewKind zoomFactorKind,
        ScrollingZoomOptions? options,
        out int viewChangeCorrelationId)
    {
        viewChangeCorrelationId = s_noOpCorrelationId;

        if (Content is null)
        {
            // When there is no content, skip the view change request and return -1, indicating that no action was taken.
            return;
        }

        ScrollingAnimationMode animationMode = options is not null ? options.AnimationMode : ScrollingAnimationMode.Auto;
        ScrollingSnapPointsMode snapPointsMode = options is not null ? options.SnapPointsMode : ScrollingSnapPointsMode.Default;
        InteractionTrackerAsyncOperationType operationType = default;

        animationMode = GetComputedAnimationMode(animationMode);

        switch (animationMode)
        {
            case ScrollingAnimationMode.Disabled:
                {
                    switch (zoomFactorKind)
                    {
                        case ScrollPresenterViewKind.Absolute:
                        case ScrollPresenterViewKind.RelativeToCurrentView:
                            {
                                operationType = InteractionTrackerAsyncOperationType.TryUpdateScale;
                                break;
                            }
                    }
                    break;
                }
            case ScrollingAnimationMode.Enabled:
                {
                    switch (zoomFactorKind)
                    {
                        case ScrollPresenterViewKind.Absolute:
                        case ScrollPresenterViewKind.RelativeToCurrentView:
                            {
                                operationType = InteractionTrackerAsyncOperationType.TryUpdateScaleWithAnimation;
                                break;
                            }
                    }
                    break;
                }
        }

        // When the ScrollPresenter is not loaded or not set up yet (delayOperation==True), delay the zoomFactor change request until it gets loaded.
        // OnCompositionTargetRendering will launch the delayed changes at that point.
        bool delayOperation = !IsLoadedAndSetUp();

        // Set to True when workaround for RS5 InteractionTracker bug 18827625 was applied (i.e. on-going TryUpdateScaleWithAnimation operation
        // is interrupted with TryUpdateScale operation).
        bool scaleChangeWithAnimationInterrupted = false;

        ScrollingZoomOptions optionsClone = null;

        // Clone the original options if any. The clone will be used if the operation ever gets processed.
        if (options is not null)
        {
            // Options are cloned so that they can be modified by the caller after this zoom factor change call without affecting the outcome of the operation.
            optionsClone = new ScrollingZoomOptions(
                animationMode,
                snapPointsMode);
        }

        if (!delayOperation)
        {
            Debug.Assert(m_interactionTracker is not null);

            // Prevent any existing delayed operation from being processed after this request and overriding it.
            // All delayed operations are completed with the Interrupted result.
            CompleteDelayedOperations();

            HookCompositionTargetRendering();
        }

        ViewChange zoomFactorChange =
            new ZoomFactorChange(
                zoomFactor,
                centerPoint,
                zoomFactorKind,
                optionsClone is not null ? optionsClone : (options));

        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = new InteractionTrackerAsyncOperation(
                operationType,
                InteractionTrackerAsyncOperationTrigger.DirectViewChange,
                delayOperation,
                zoomFactorChange);

        m_interactionTrackerAsyncOperations.Add(interactionTrackerAsyncOperation);

        // Workaround for InteractionTracker bug 22414894 - calling TryUpdateScale after a non-animated view change during the same tick results in an incorrect position.
        // That non-animated view change needs to complete before this TryUpdateScale gets invoked.
        interactionTrackerAsyncOperation.SetRequiredOperation(GetLastNonAnimatedInteractionTrackerOperation(interactionTrackerAsyncOperation));

        if (viewChangeCorrelationId != 0)
        {
            m_latestViewChangeCorrelationId = GetNextViewChangeCorrelationId();
            interactionTrackerAsyncOperation.SetViewChangeCorrelationId(m_latestViewChangeCorrelationId);
            viewChangeCorrelationId = m_latestViewChangeCorrelationId;
        }
    }

    private void ChangeZoomFactorWithAdditionalVelocityPrivate(
        float zoomFactorVelocity,
        float anticipatedZoomFactorChange,
        Vector2? centerPoint,
        float? inertiaDecayRate,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        out int viewChangeCorrelationId)
    {
        viewChangeCorrelationId = s_noOpCorrelationId;

        if (Content is null)
        {
            // When there is no content, skip the view change request and return -1, indicating that no action was taken.
            return;
        }

        // When the ScrollPresenter is not loaded or not set up yet (delayOperation==True), delay the zoom factor change request until it gets loaded.
        // OnCompositionTargetRendering will launch the delayed changes at that point.
        bool delayOperation = !IsLoadedAndSetUp();

        ViewChangeBase zoomFactorChangeWithAdditionalVelocity =
            new ZoomFactorChangeWithAdditionalVelocity(
                zoomFactorVelocity, anticipatedZoomFactorChange, centerPoint, inertiaDecayRate);

        if (!delayOperation)
        {
            Debug.Assert(m_interactionTracker is not null);

            // Prevent any existing delayed operation from being processed after this request and overriding it.
            // All delayed operations are completed with the Interrupted result.
            CompleteDelayedOperations();

            HookCompositionTargetRendering();
        }

        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = new InteractionTrackerAsyncOperation(
                InteractionTrackerAsyncOperationType.TryUpdateScaleWithAdditionalVelocity,
                operationTrigger,
                delayOperation,
                zoomFactorChangeWithAdditionalVelocity);

        if (operationTrigger != InteractionTrackerAsyncOperationTrigger.DirectViewChange)
        {
            // User-triggered operations are processed as quickly as possible by minimizing their TicksCountDown
            int ticksCountdown = GetInteractionTrackerOperationsTicksCountdown();

            interactionTrackerAsyncOperation.SetTicksCountdown(Math.Max(1, ticksCountdown));
        }

        m_interactionTrackerAsyncOperations.Add(interactionTrackerAsyncOperation);

        if (viewChangeCorrelationId != 0)
        {
            m_latestViewChangeCorrelationId = GetNextViewChangeCorrelationId();
            interactionTrackerAsyncOperation.SetViewChangeCorrelationId(m_latestViewChangeCorrelationId);
            viewChangeCorrelationId = m_latestViewChangeCorrelationId;
        }
    }

    private void CompleteDelayedOperations()
    {
        if (m_interactionTrackerAsyncOperations.Count <= 0)
        {
            return;
        }

        for (var i = 0; i < m_interactionTrackerAsyncOperations.Count; i++)
        {
            var interactionTrackerAsyncOperation = m_interactionTrackerAsyncOperations[i];

            if (interactionTrackerAsyncOperation.IsDelayed())
            {
                CompleteViewChange(interactionTrackerAsyncOperation, ScrollPresenterViewChangeResult.Interrupted);
                m_interactionTrackerAsyncOperations.Remove(interactionTrackerAsyncOperation);
                i--;
            }
        }
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
        Debug.Assert(requestId != 0);
        Debug.Assert(completeNonAnimatedOperation || completeAnimatedOperation || completePriorNonAnimatedOperations || completePriorAnimatedOperations);

        if (m_interactionTrackerAsyncOperations.Count <= 0)
        {
            return;
        }

        for (var i = 0; i < m_interactionTrackerAsyncOperations.Count; i++)
        {
            var interactionTrackerAsyncOperation = m_interactionTrackerAsyncOperations[i];

            bool isMatch = requestId == -1 || requestId == interactionTrackerAsyncOperation.GetRequestId();
            bool isPriorMatch = requestId > interactionTrackerAsyncOperation.GetRequestId() && -1 != interactionTrackerAsyncOperation.GetRequestId();

            if ((isPriorMatch && (completePriorNonAnimatedOperations || completePriorAnimatedOperations)) ||
                (isMatch && (completeNonAnimatedOperation || completeAnimatedOperation)))
            {
                bool isOperationAnimated = interactionTrackerAsyncOperation.IsAnimated();
                bool complete =
                    (isMatch && completeNonAnimatedOperation && !isOperationAnimated) ||
                    (isMatch && completeAnimatedOperation && isOperationAnimated) ||
                    (isPriorMatch && completePriorNonAnimatedOperations && !isOperationAnimated) ||
                    (isPriorMatch && completePriorAnimatedOperations && isOperationAnimated);

                if (complete)
                {
                    CompleteViewChange(
                        interactionTrackerAsyncOperation,
                        isMatch ? operationResult : (isOperationAnimated ? priorAnimatedOperationsResult : priorNonAnimatedOperationsResult));

                    var interactionTrackerAsyncOperationRemoved = interactionTrackerAsyncOperation;

                    m_interactionTrackerAsyncOperations.Remove(interactionTrackerAsyncOperationRemoved);
                    i--;

                    switch (interactionTrackerAsyncOperationRemoved.GetOperationType())
                    {
                        case InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity:
                            PostProcessOffsetsChange(interactionTrackerAsyncOperationRemoved);
                            break;

                        case InteractionTrackerAsyncOperationType.TryUpdateScaleWithAdditionalVelocity:
                            PostProcessZoomFactorChange(interactionTrackerAsyncOperationRemoved);
                            break;
                    }
                }
            }
        }
    }

    private void CompleteViewChange(
            InteractionTrackerAsyncOperation interactionTrackerAsyncOperation,
        ScrollPresenterViewChangeResult result)
    {
        int viewChangeCorrelationId = interactionTrackerAsyncOperation.GetViewChangeCorrelationId();

        interactionTrackerAsyncOperation.SetIsCompleted(true);

        bool onHorizontalOffsetChangeCompleted = false;
        bool onVerticalOffsetChangeCompleted = false;

        switch ((int)(interactionTrackerAsyncOperation.GetOperationTrigger()))
        {
            case (int)(InteractionTrackerAsyncOperationTrigger.DirectViewChange):
            case (int)(InteractionTrackerAsyncOperationTrigger.BringIntoViewRequest):
                switch (interactionTrackerAsyncOperation.GetOperationType())
                {
                    case InteractionTrackerAsyncOperationType.TryUpdatePosition:
                    case InteractionTrackerAsyncOperationType.TryUpdatePositionBy:
                    case InteractionTrackerAsyncOperationType.TryUpdatePositionWithAnimation:
                    case InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity:
                        RaiseViewChangeCompleted(true /*isForScroll*/, result, viewChangeCorrelationId);
                        break;

                    default:
                        // Stop Translation and Scale animations if needed, to trigger rasterization of Content & avoid fuzzy text rendering for instance.
                        StopTranslationAndZoomFactorExpressionAnimations();

                        RaiseViewChangeCompleted(false /*isForScroll*/, result, viewChangeCorrelationId);
                        break;
                }
                break;

            case (int)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest):
                onHorizontalOffsetChangeCompleted = true;
                break;

            case (int)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest):
                onVerticalOffsetChangeCompleted = true;
                break;

            case (int)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest) |
                 (int)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest):
                onHorizontalOffsetChangeCompleted = true;
                onVerticalOffsetChangeCompleted = true;
                break;
        }

        if (onHorizontalOffsetChangeCompleted && m_horizontalScrollController is not null)
        {
            m_horizontalScrollController.NotifyRequestedScrollCompleted(viewChangeCorrelationId);
        }

        if (onVerticalOffsetChangeCompleted && m_verticalScrollController is not null)
        {
            m_verticalScrollController.NotifyRequestedScrollCompleted(viewChangeCorrelationId);
        }
    }

    private void ComputeAnchorPoint(
        Rect anchorBounds,
        out double anchorPointX,
        out double anchorPointY)
    {
        throw new NotImplementedException();
    }

    private void ComputeBringIntoViewTargetOffsets(
            UIElement content,
        UIElement element,
        Rect elementRect,
        ScrollingSnapPointsMode snapPointsMode,
        double horizontalAlignmentRatio,
        double verticalAlignmentRatio,
        double horizontalOffset,
        double verticalOffset,
        out double targetZoomedHorizontalOffset,
        out double targetZoomedVerticalOffset,
        out double appliedOffsetX,
        out double appliedOffsetY,
        out Rect targetRect)
    {
        Debug.Assert(content is not null);
        Debug.Assert(element is not null);

        targetZoomedHorizontalOffset = 0.0;
        targetZoomedVerticalOffset = 0.0;

        appliedOffsetX = 0.0;

        appliedOffsetY = 0.0;

        targetRect = default;

        Rect transformedRect = GetDescendantBounds(content, element, elementRect);

        double targetX = transformedRect.X;
        double targetWidth = transformedRect.Width;
        double targetY = transformedRect.Y;
        double targetHeight = transformedRect.Height;

        if (!double.IsNaN(horizontalAlignmentRatio))
        {
            // Account for the horizontal alignment ratio
            Debug.Assert(horizontalAlignmentRatio >= 0.0 && horizontalAlignmentRatio <= 1.0);

            targetX += (targetWidth - m_viewportWidth / m_zoomFactor) * horizontalAlignmentRatio;
            targetWidth = m_viewportWidth / m_zoomFactor;
        }

        if (!double.IsNaN(verticalAlignmentRatio))
        {
            // Account for the vertical alignment ratio
            Debug.Assert(verticalAlignmentRatio >= 0.0 && verticalAlignmentRatio <= 1.0);

            targetY += (targetHeight - m_viewportHeight / m_zoomFactor) * verticalAlignmentRatio;
            targetHeight = m_viewportHeight / m_zoomFactor;
        }

        double targetZoomedHorizontalOffsetTmp = ComputeZoomedOffsetWithMinimalChange(
            m_zoomedHorizontalOffset,
            m_zoomedHorizontalOffset + m_viewportWidth,
            targetX * m_zoomFactor,
            (targetX + targetWidth) * m_zoomFactor);
        double targetZoomedVerticalOffsetTmp = ComputeZoomedOffsetWithMinimalChange(
            m_zoomedVerticalOffset,
            m_zoomedVerticalOffset + m_viewportHeight,
            targetY * m_zoomFactor,
            (targetY + targetHeight) * m_zoomFactor);

        double scrollableWidth = ScrollableWidth;
        double scrollableHeight = ScrollableHeight;

        targetZoomedHorizontalOffsetTmp = Math.Clamp(targetZoomedHorizontalOffsetTmp, 0.0, scrollableWidth);
        targetZoomedVerticalOffsetTmp = Math.Clamp(targetZoomedVerticalOffsetTmp, 0.0, scrollableHeight);

        double offsetX = horizontalOffset;
        double offsetY = verticalOffset;
        double appliedOffsetXTmp = 0.0;
        double appliedOffsetYTmp = 0.0;

        // If the target offset is within bounds and an offset was provided, apply as much of it as possible while remaining within bounds.
        if (offsetX != 0.0 && targetZoomedHorizontalOffsetTmp >= 0.0)
        {
            if (targetZoomedHorizontalOffsetTmp <= scrollableWidth)
            {
                if (offsetX > 0.0)
                {
                    appliedOffsetXTmp = Math.Min(targetZoomedHorizontalOffsetTmp, offsetX);
                }
                else
                {
                    appliedOffsetXTmp = -Math.Min(scrollableWidth - targetZoomedHorizontalOffsetTmp, -offsetX);
                }
                targetZoomedHorizontalOffsetTmp -= appliedOffsetXTmp;
            }
        }

        if (offsetY != 0.0 && targetZoomedVerticalOffsetTmp >= 0.0)
        {
            if (targetZoomedVerticalOffsetTmp <= scrollableHeight)
            {
                if (offsetY > 0.0)
                {
                    appliedOffsetYTmp = Math.Min(targetZoomedVerticalOffsetTmp, offsetY);
                }
                else
                {
                    appliedOffsetYTmp = -Math.Min(scrollableHeight - targetZoomedVerticalOffsetTmp, -offsetY);
                }
                targetZoomedVerticalOffsetTmp -= appliedOffsetYTmp;
            }
        }

        Debug.Assert(targetZoomedHorizontalOffsetTmp >= 0.0);
        Debug.Assert(targetZoomedVerticalOffsetTmp >= 0.0);
        Debug.Assert(targetZoomedHorizontalOffsetTmp <= scrollableWidth);
        Debug.Assert(targetZoomedVerticalOffsetTmp <= scrollableHeight);

        if (snapPointsMode == ScrollingSnapPointsMode.Default)
        {
            // Finally adjust the target offsets based on snap points
            targetZoomedHorizontalOffsetTmp = ComputeValueAfterSnapPoints<ScrollSnapPointBase>(
                targetZoomedHorizontalOffsetTmp, m_sortedConsolidatedHorizontalSnapPoints);
            targetZoomedVerticalOffsetTmp = ComputeValueAfterSnapPoints<ScrollSnapPointBase>(
                targetZoomedVerticalOffsetTmp, m_sortedConsolidatedVerticalSnapPoints);

            // Make sure the target offsets are within the scrollable boundaries
            targetZoomedHorizontalOffsetTmp = Math.Clamp(targetZoomedHorizontalOffsetTmp, 0.0, scrollableWidth);
            targetZoomedVerticalOffsetTmp = Math.Clamp(targetZoomedVerticalOffsetTmp, 0.0, scrollableHeight);

            Debug.Assert(targetZoomedHorizontalOffsetTmp >= 0.0);
            Debug.Assert(targetZoomedVerticalOffsetTmp >= 0.0);
            Debug.Assert(targetZoomedHorizontalOffsetTmp <= scrollableWidth);
            Debug.Assert(targetZoomedVerticalOffsetTmp <= scrollableHeight);
        }

        targetZoomedHorizontalOffset = targetZoomedHorizontalOffsetTmp;
        targetZoomedVerticalOffset = targetZoomedVerticalOffsetTmp;

        if (appliedOffsetX != 0)
        {
            appliedOffsetX = appliedOffsetXTmp;
        }

        if (appliedOffsetY != 0)
        {
            appliedOffsetY = appliedOffsetYTmp;
        }

        if (targetRect != default)
        {
            targetRect = new Rect(
                (float)(targetX),
            (float)(targetY),
            (float)(targetWidth),
            (float)(targetHeight)
            );
        }
    }

    private void ComputeBringIntoViewTargetOffsetsFromRequestEventArgs(
        UIElement content,
        ScrollingSnapPointsMode snapPointsMode,
        BringIntoViewRequestedEventArgs requestEventArgs,
        out double targetZoomedHorizontalOffset,
        out double targetZoomedVerticalOffset,
        out double appliedOffsetX,
        out double appliedOffsetY,
        out Rect targetRect)
    {
        ComputeBringIntoViewTargetOffsets(
            content,
            requestEventArgs.TargetElement,
            requestEventArgs.TargetRect,
            snapPointsMode,
            requestEventArgs.HorizontalAlignmentRatio,
            requestEventArgs.VerticalAlignmentRatio,
            requestEventArgs.HorizontalOffset,
            requestEventArgs.VerticalOffset,
            out targetZoomedHorizontalOffset,
            out targetZoomedVerticalOffset,
            out appliedOffsetX,
            out appliedOffsetY,
            out targetRect);
    }

    private void ComputeBringIntoViewUpdatedTargetOffsets(
                UIElement content,
        UIElement element,
        Rect elementRect,
        ScrollingSnapPointsMode snapPointsMode,
        double horizontalAlignmentRatio,
        double verticalAlignmentRatio,
        double horizontalOffset,
        double verticalOffset,
        out double targetZoomedHorizontalOffset,
        out double targetZoomedVerticalOffset)
    {
        ComputeBringIntoViewTargetOffsets(
            content,
            element,
            elementRect,
            snapPointsMode,
            horizontalAlignmentRatio,
            verticalAlignmentRatio,
            horizontalOffset,
            verticalOffset,
            out targetZoomedHorizontalOffset,
            out targetZoomedVerticalOffset,
            out _ /*appliedOffsetX*/,
            out _ /*appliedOffsetY*/,
            out _ /*targetRect*/);
    }

    private float ComputeContentLayoutOffsetDelta(ScrollPresenterDimension dimension, float unzoomedDelta)
    {
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);

        float zoomedDelta = unzoomedDelta * m_zoomFactor;

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            if (zoomedDelta < 0.0f && -zoomedDelta > m_zoomedHorizontalOffset)
            {
                // Do not let m_zoomedHorizontalOffset step into negative territory.
                zoomedDelta = (float)(-m_zoomedHorizontalOffset);
            }
        }
        else
        {
            if (zoomedDelta < 0.0f && -zoomedDelta > m_zoomedVerticalOffset)
            {
                // Do not let m_zoomedVerticalOffset step into negative territory.
                zoomedDelta = (float)(-m_zoomedVerticalOffset);
            }
        }

        return -zoomedDelta;
    }

    private void ComputeElementAnchorPoint(
        bool isForPreArrange,
        out double elementAnchorPointHorizontalOffset,
        out double elementAnchorPointVerticalOffset)
    {
        throw new NotImplementedException();
    }

    private void ComputeMinMaxPositions(float zoomFactor, out Vector2 minPosition, out Vector2 maxPosition)
    {
        minPosition = Vector2.Zero;
        maxPosition = Vector2.Zero;

        UIElement content = Content;

        if (content is null)
        {
            return;
        }

        FrameworkElement contentAsFE = content as FrameworkElement;

        if (contentAsFE is null)
        {
            return;
        }

        bool isRightToLeftDirection = FlowDirection == FlowDirection.RightToLeft;
        Visual scrollPresenterVisual = ElementCompositionPreview.GetElementVisual(this);
        float minPosX = 0.0f;
        float minPosY = 0.0f;
        float maxPosX = 0.0f;
        float maxPosY = 0.0f;
        float extentWidth = (float)(m_unzoomedExtentWidth);
        float extentHeight = (float)(m_unzoomedExtentHeight);

        if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Center ||
            contentAsFE.HorizontalAlignment == HorizontalAlignment.Stretch)
        {
            float scrollableWidth = extentWidth * zoomFactor - scrollPresenterVisual.Size.X;

            if ((minPosition != default) || (isRightToLeftDirection && (maxPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableWidth < 0, minPosX is scrollableWidth / 2 so it is centered at idle.
                // When the zoomed content is larger than the viewport, scrollableWidth > 0, minPosX is 0.
                minPosX = Math.Min(0.0f, scrollableWidth / 2.0f);
            }

            if ((maxPosition != default) || (isRightToLeftDirection && (minPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableWidth < 0, maxPosX is scrollableWidth / 2 so it is centered at idle.
                // When the zoomed content is larger than the viewport, scrollableWidth > 0, maxPosX is scrollableWidth.
                maxPosX = scrollableWidth;
                if (maxPosX < 0.0f)
                {
                    maxPosX /= 2.0f;
                }
            }
        }
        else if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Right)
        {
            float scrollableWidth = extentWidth * zoomFactor - scrollPresenterVisual.Size.X;

            if ((minPosition != default) || (isRightToLeftDirection && (maxPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableWidth < 0, minPosX is scrollableWidth so it is right-aligned at idle.
                // When the zoomed content is larger than the viewport, scrollableWidth > 0, minPosX is 0.
                minPosX = Math.Min(0.0f, scrollableWidth);
            }

            if ((maxPosition != default) || (isRightToLeftDirection && (minPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableWidth < 0, maxPosX is -scrollableWidth so it is right-aligned at idle.
                // When the zoomed content is larger than the viewport, scrollableWidth > 0, maxPosX is scrollableWidth.
                maxPosX = scrollableWidth;
                if (maxPosX < 0.0f)
                {
                    maxPosX *= -1.0f;
                }
            }
        }

        if (contentAsFE.VerticalAlignment == VerticalAlignment.Center ||
            contentAsFE.VerticalAlignment == VerticalAlignment.Stretch)
        {
            float scrollableHeight = extentHeight * zoomFactor - scrollPresenterVisual.Size.Y;

            if ((minPosition != default) || (isRightToLeftDirection && (maxPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableHeight < 0, minPosY is scrollableHeight / 2 so it is centered at idle.
                // When the zoomed content is larger than the viewport, scrollableHeight > 0, minPosY is 0.
                minPosY = Math.Min(0.0f, scrollableHeight / 2.0f);
            }

            if ((maxPosition != default) || (isRightToLeftDirection && (minPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableHeight < 0, maxPosY is scrollableHeight / 2 so it is centered at idle.
                // When the zoomed content is larger than the viewport, scrollableHeight > 0, maxPosY is scrollableHeight.
                maxPosY = scrollableHeight;
                if (maxPosY < 0.0f)
                {
                    maxPosY /= 2.0f;
                }
            }
        }
        else if (contentAsFE.VerticalAlignment == VerticalAlignment.Bottom)
        {
            float scrollableHeight = extentHeight * zoomFactor - scrollPresenterVisual.Size.Y;

            if ((minPosition != default) || (isRightToLeftDirection && (maxPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableHeight < 0, minPosY is scrollableHeight so it is bottom-aligned at idle.
                // When the zoomed content is larger than the viewport, scrollableHeight > 0, minPosY is 0.
                minPosY = Math.Min(0.0f, scrollableHeight);
            }

            if ((maxPosition != default) || (isRightToLeftDirection && (minPosition != default)))
            {
                // When the zoomed content is smaller than the viewport, scrollableHeight < 0, maxPosY is -scrollableHeight so it is bottom-aligned at idle.
                // When the zoomed content is larger than the viewport, scrollableHeight > 0, maxPosY is scrollableHeight.
                maxPosY = scrollableHeight;
                if (maxPosY < 0.0f)
                {
                    maxPosY *= -1.0f;
                }
            }
        }

        if (minPosition != default)
        {
            if (isRightToLeftDirection)
            {
                minPosition = new Vector2(-maxPosX - m_contentLayoutOffsetX, minPosY + m_contentLayoutOffsetY);
            }
            else
            {
                minPosition = new Vector2(minPosX + m_contentLayoutOffsetX, minPosY + m_contentLayoutOffsetY);
            }
        }

        if (maxPosition != default)
        {
            if (isRightToLeftDirection)
            {
                maxPosition = new Vector2(-minPosX - m_contentLayoutOffsetX, maxPosY + m_contentLayoutOffsetY);
            }
            else
            {
                maxPosition = new Vector2(maxPosX + m_contentLayoutOffsetX, maxPosY + m_contentLayoutOffsetY);
            }
        }
    }

    private Vector2 ComputePositionFromOffsets(double zoomedHorizontalOffset, double zoomedVerticalOffset)
    {
        bool isRightToLeftDirection = FlowDirection == FlowDirection.RightToLeft;
        Vector2 minPosition = new Vector2();
        Vector2 maxPosition = new Vector2();

        if (isRightToLeftDirection)
        {
            ComputeMinMaxPositions(m_zoomFactor, out minPosition, out maxPosition);
        }
        else
        {
            ComputeMinMaxPositions(m_zoomFactor, out minPosition, out _);
        }

        if (isRightToLeftDirection)
        {
            return new Vector2((float)(maxPosition.X - zoomedHorizontalOffset), (float)(zoomedVerticalOffset + minPosition.Y));
        }
        else
        {
            return new Vector2((float)(zoomedHorizontalOffset + minPosition.X), (float)(zoomedVerticalOffset + minPosition.Y));
        }
    }

    private double ComputeValueAfterSnapPoints<T>(double value, HashSet<SnapPointWrapper<T>> snapPointsSet)
    {
        throw new NotImplementedException();
    }

    private void ComputeViewportAnchorPoint(
        double viewportWidth,
        double viewportHeight,
        out double viewportAnchorPointHorizontalOffset,
        out double viewportAnchorPointVerticalOffset)
    {
        viewportAnchorPointHorizontalOffset = double.NaN;
        viewportAnchorPointVerticalOffset = double.NaN;

        Rect viewportAnchorBounds = new Rect(
            (float)(m_zoomedHorizontalOffset / m_zoomFactor),
        (float)(m_zoomedVerticalOffset / m_zoomFactor),
        (float)(viewportWidth / m_zoomFactor),
        (float)(viewportHeight / m_zoomFactor)
        );

        ComputeAnchorPoint(viewportAnchorBounds, out viewportAnchorPointHorizontalOffset, out viewportAnchorPointVerticalOffset);
    }

    private Size ComputeViewportToElementAnchorPointsDistance(
        double viewportWidth,
        double viewportHeight,
        bool isForPreArrange)
    {
        if (m_anchorElement is not null)
        {
            Debug.Assert(!isForPreArrange || IsElementValidAnchor(m_anchorElement));

            if (!isForPreArrange && !IsElementValidAnchor(m_anchorElement))
            {
                return new Size(float.NaN, float.NaN);
            }

            double elementAnchorPointHorizontalOffset = 0.0;
            double elementAnchorPointVerticalOffset = 0.0;
            double viewportAnchorPointHorizontalOffset = 0.0;
            double viewportAnchorPointVerticalOffset = 0.0;

            ComputeElementAnchorPoint(
                isForPreArrange,
                out elementAnchorPointHorizontalOffset,
                out elementAnchorPointVerticalOffset);
            ComputeViewportAnchorPoint(
                viewportWidth,
                viewportHeight,
                out viewportAnchorPointHorizontalOffset,
                out viewportAnchorPointVerticalOffset);

            Debug.Assert(!double.IsNaN(viewportAnchorPointHorizontalOffset) || !double.IsNaN(viewportAnchorPointVerticalOffset));
            Debug.Assert(double.IsNaN(viewportAnchorPointHorizontalOffset) == double.IsNaN(elementAnchorPointHorizontalOffset));
            Debug.Assert(double.IsNaN(viewportAnchorPointVerticalOffset) == double.IsNaN(elementAnchorPointVerticalOffset));

            // Rounding the distance to 6 precision digits to avoid layout cycles due to float/double conversions.
            Size viewportToElementAnchorPointsDistance = new Size(
                double.IsNaN(viewportAnchorPointHorizontalOffset) ?
                    float.NaN : (float)(Math.Round((elementAnchorPointHorizontalOffset - viewportAnchorPointHorizontalOffset) * 1000000) / 1000000),
            double.IsNaN(viewportAnchorPointVerticalOffset) ?
                float.NaN : (float)(Math.Round((elementAnchorPointVerticalOffset - viewportAnchorPointVerticalOffset) * 1000000) / 1000000)
            );

            return viewportToElementAnchorPointsDistance;
        }
        else
        {
            return new Size(float.NaN, float.NaN);
        }
    }

    internal void CustomAnimationStateEntered(InteractionTrackerCustomAnimationStateEnteredArgs args)
    {
        UpdateState(ScrollingInteractionState.Animation);
    }

    private void EnsureAnchorElementSelection()
    {
        if (!m_isAnchorElementDirty)
        {
            return;
        }

        m_anchorElement = (null);
        m_anchorElementBounds = new Rect();
        m_isAnchorElementDirty = false;

        ScrollPresenterTestHooks globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();
        double viewportAnchorPointHorizontalOffset = 0.0;
        double viewportAnchorPointVerticalOffset = 0.0;

        ComputeViewportAnchorPoint(
            m_viewportWidth,
            m_viewportHeight,
            out viewportAnchorPointHorizontalOffset,
            out viewportAnchorPointVerticalOffset);

        Debug.Assert(!double.IsNaN(viewportAnchorPointHorizontalOffset) || !double.IsNaN(viewportAnchorPointVerticalOffset));

        RaiseAnchorRequested();

        var anchorRequestedEventArgs = m_anchorRequestedEventArgs;
        UIElement requestedAnchorElement = null;
        IList<UIElement> anchorCandidates = null;
        UIElement content = Content;

        if (anchorRequestedEventArgs is not null)
        {
            requestedAnchorElement = anchorRequestedEventArgs.AnchorElement;
            anchorCandidates = anchorRequestedEventArgs.AnchorCandidates;
        }

        if (requestedAnchorElement is not null)
        {
            m_anchorElement = (requestedAnchorElement);
            m_anchorElementBounds = GetDescendantBounds(content, requestedAnchorElement);

            if (globalTestHooks is not null && ScrollPresenterTestHooks.AreAnchorNotificationsRaised())
            {
                ScrollPresenterTestHooks.NotifyAnchorEvaluated(this, requestedAnchorElement, viewportAnchorPointHorizontalOffset, viewportAnchorPointVerticalOffset);
            }

            return;
        }

        Rect bestAnchorCandidateBounds = new Rect();
        UIElement bestAnchorCandidate = null;
        double bestAnchorCandidateDistance = float.MaxValue;
        Rect viewportAnchorBounds = new Rect(
            (float)(m_zoomedHorizontalOffset / m_zoomFactor),
        (float)(m_zoomedVerticalOffset / m_zoomFactor),
        (float)(m_viewportWidth / m_zoomFactor),
        (float)(m_viewportHeight / m_zoomFactor)
);

        Debug.Assert(content is not null);

        if (anchorCandidates is not null)
        {
            foreach (UIElement anchorCandidate in anchorCandidates)
            {
                ProcessAnchorCandidate(
                    anchorCandidate,
                    content,
                    viewportAnchorBounds,
                    viewportAnchorPointHorizontalOffset,
                    viewportAnchorPointVerticalOffset,
                    ref bestAnchorCandidateDistance,
                    ref bestAnchorCandidate,
                    ref bestAnchorCandidateBounds);
            }
        }
        else
        {
            foreach (UIElement anchorCandidateTracker in m_anchorCandidates)
            {
                UIElement anchorCandidate = anchorCandidateTracker;

                ProcessAnchorCandidate(
                    anchorCandidate,
                    content,
                    viewportAnchorBounds,
                    viewportAnchorPointHorizontalOffset,
                    viewportAnchorPointVerticalOffset,
                    ref bestAnchorCandidateDistance,
                    ref bestAnchorCandidate,
                    ref bestAnchorCandidateBounds);
            }
        }

        if (bestAnchorCandidate is not null)
        {
            m_anchorElement = (bestAnchorCandidate);
            m_anchorElementBounds = bestAnchorCandidateBounds;
        }

        if (globalTestHooks is not null && ScrollPresenterTestHooks.AreAnchorNotificationsRaised())
        {
            ScrollPresenterTestHooks.NotifyAnchorEvaluated(this, m_anchorElement, viewportAnchorPointHorizontalOffset, viewportAnchorPointVerticalOffset);
        }
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

    private void EnsureScrollControllerExpressionAnimationSources(
        ScrollPresenterDimension dimension)
    {
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);
        Debug.Assert(m_interactionTracker is not null);

        Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        CompositionPropertySet scrollControllerExpressionAnimationSources = null;

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            if (m_horizontalScrollControllerExpressionAnimationSources is not null)
            {
                return;
            }

            m_horizontalScrollControllerExpressionAnimationSources = scrollControllerExpressionAnimationSources = compositor.CreatePropertySet();
        }
        else
        {
            if (m_verticalScrollControllerExpressionAnimationSources is not null)
            {
                return;
            }

            m_verticalScrollControllerExpressionAnimationSources = scrollControllerExpressionAnimationSources = compositor.CreatePropertySet();
        }

        scrollControllerExpressionAnimationSources.InsertScalar(s_minOffsetPropertyName, 0.0f);
        scrollControllerExpressionAnimationSources.InsertScalar(s_maxOffsetPropertyName, 0.0f);
        scrollControllerExpressionAnimationSources.InsertScalar(s_offsetPropertyName, 0.0f);
        scrollControllerExpressionAnimationSources.InsertScalar(s_multiplierPropertyName, 1.0f);

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            Debug.Assert(m_horizontalScrollControllerOffsetExpressionAnimation is null);
            Debug.Assert(m_horizontalScrollControllerMaxOffsetExpressionAnimation is null);

            m_horizontalScrollControllerOffsetExpressionAnimation = compositor.CreateExpressionAnimation("it.Position.X - it.MinPosition.X");
            m_horizontalScrollControllerOffsetExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);
            m_horizontalScrollControllerMaxOffsetExpressionAnimation = compositor.CreateExpressionAnimation("it.MaxPosition.X - it.MinPosition.X");
            m_horizontalScrollControllerMaxOffsetExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);
        }
        else
        {
            Debug.Assert(m_verticalScrollControllerOffsetExpressionAnimation is null);
            Debug.Assert(m_verticalScrollControllerMaxOffsetExpressionAnimation is null);

            m_verticalScrollControllerOffsetExpressionAnimation = compositor.CreateExpressionAnimation("it.Position.Y - it.MinPosition.Y");
            m_verticalScrollControllerOffsetExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);
            m_verticalScrollControllerMaxOffsetExpressionAnimation = compositor.CreateExpressionAnimation("it.MaxPosition.Y - it.MinPosition.Y");
            m_verticalScrollControllerMaxOffsetExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);
        }
    }

    private void EnsureScrollControllerVisualInteractionSource(
        Visual panningElementAncestorVisual,
        ScrollPresenterDimension dimension)
    {
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);
        Debug.Assert(m_interactionTracker is not null);

        VisualInteractionSource scrollControllerVisualInteractionSource = VisualInteractionSource.Create(panningElementAncestorVisual);
        scrollControllerVisualInteractionSource.ManipulationRedirectionMode = (VisualInteractionSourceRedirectionMode.CapableTouchpadOnly);
        scrollControllerVisualInteractionSource.PositionXChainingMode = (InteractionChainingMode.Never);
        scrollControllerVisualInteractionSource.PositionYChainingMode = (InteractionChainingMode.Never);
        scrollControllerVisualInteractionSource.ScaleChainingMode = (InteractionChainingMode.Never);
        scrollControllerVisualInteractionSource.ScaleSourceMode = (InteractionSourceMode.Disabled);
        m_interactionTracker.InteractionSources.Add(scrollControllerVisualInteractionSource);

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            Debug.Assert(m_horizontalScrollController is not null);
            Debug.Assert(m_horizontalScrollControllerPanningInfo is not null);
            Debug.Assert(m_horizontalScrollControllerVisualInteractionSource is null);
            m_horizontalScrollControllerVisualInteractionSource = scrollControllerVisualInteractionSource;

            HookHorizontalScrollControllerInteractionSourceEvents(m_horizontalScrollControllerPanningInfo);
        }
        else
        {
            Debug.Assert(m_verticalScrollController is not null);
            Debug.Assert(m_verticalScrollControllerPanningInfo is not null);
            Debug.Assert(m_verticalScrollControllerVisualInteractionSource is null);
            m_verticalScrollControllerVisualInteractionSource = scrollControllerVisualInteractionSource;

            HookVerticalScrollControllerInteractionSourceEvents(m_verticalScrollControllerPanningInfo);
        }

        RaiseInteractionSourcesChanged();
    }

    private void EnsureScrollPresenterVisualInteractionSource()
    {
        if (m_scrollPresenterVisualInteractionSource is null)
        {
            EnsureInteractionTracker();

            Visual scrollPresenterVisual = ElementCompositionPreview.GetElementVisual(this);
            VisualInteractionSource scrollPresenterVisualInteractionSource = VisualInteractionSource.Create(scrollPresenterVisual);
            m_interactionTracker.InteractionSources.Add(scrollPresenterVisualInteractionSource);
            m_scrollPresenterVisualInteractionSource = scrollPresenterVisualInteractionSource;
            UpdateManipulationRedirectionMode();
            RaiseInteractionSourcesChanged();
        }
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
        Debug.Assert(content is not null);

        Thickness contentMargin = content.Margin;
        double marginWidth = contentMargin.Left + contentMargin.Right;
        double computedMaxWidth = defaultMaxWidth;
        double width = content.Width;
        double minWidth = content.MinWidth;
        double maxWidth = content.MaxWidth;

        if (!double.IsNaN(width))
        {
            width = Math.Max(0.0, width + marginWidth);
            computedMaxWidth = width;
        }
        if (!double.IsNaN(minWidth))
        {
            minWidth = Math.Max(0.0, minWidth + marginWidth);
            computedMaxWidth = Math.Max(computedMaxWidth, minWidth);
        }
        if (!double.IsNaN(maxWidth))
        {
            maxWidth = Math.Max(0.0, maxWidth + marginWidth);
            computedMaxWidth = Math.Min(computedMaxWidth, maxWidth);
        }

        return computedMaxWidth;
    }

    private ScrollingScrollMode GetComputedScrollMode(ScrollPresenterDimension dimension, bool ignoreZoomMode = false)
    {
        ScrollingScrollMode oldComputedScrollMode;
        ScrollingScrollMode newComputedScrollMode;

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            oldComputedScrollMode = ComputedHorizontalScrollMode;
            newComputedScrollMode = HorizontalScrollMode;
        }
        else
        {
            Debug.Assert(dimension == ScrollPresenterDimension.VerticalScroll);
            oldComputedScrollMode = ComputedVerticalScrollMode;
            newComputedScrollMode = VerticalScrollMode;
        }

        if (newComputedScrollMode == ScrollingScrollMode.Auto)
        {
            if (!ignoreZoomMode && ZoomMode == ScrollingZoomMode.Enabled)
            {
                // Allow scrolling when zooming is turned on so that the Content does not get stuck in the given dimension
                // when it becomes smaller than the viewport.
                newComputedScrollMode = ScrollingScrollMode.Enabled;
            }
            else
            {
                if (dimension == ScrollPresenterDimension.HorizontalScroll)
                {
                    // Enable horizontal scrolling only when the Content's width is larger than the ScrollPresenter's width
                    newComputedScrollMode = ScrollableWidth > 0.0 ? ScrollingScrollMode.Enabled : ScrollingScrollMode.Disabled;
                }
                else
                {
                    // Enable vertical scrolling only when the Content's height is larger than the ScrollPresenter's height
                    newComputedScrollMode = ScrollableHeight > 0.0 ? ScrollingScrollMode.Enabled : ScrollingScrollMode.Disabled;
                }
            }
        }

        if (oldComputedScrollMode != newComputedScrollMode)
        {
            if (dimension == ScrollPresenterDimension.HorizontalScroll)
            {
                SetValue(ComputedHorizontalScrollModeProperty, (newComputedScrollMode));
            }
            else
            {
                SetValue(ComputedVerticalScrollModeProperty, (newComputedScrollMode));
            }
        }

        return newComputedScrollMode;
    }

    private InteractionTrackerAsyncOperation GetInteractionTrackerOperationFromKinds(
        bool isOperationTypeForOffsetsChange,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        ScrollPresenterViewKind viewKind,
        ScrollingScrollOptions options)
    {
        // Going through the existing operations from most recent to oldest, trying to find a match for the trigger, kind and options.
        for (var i = m_interactionTrackerAsyncOperations.Count - 1; i >= 0; i--)
        {
            var interactionTrackerAsyncOperation = m_interactionTrackerAsyncOperations[i];

            if (((int)(interactionTrackerAsyncOperation.GetOperationTrigger()) & (int)(operationTrigger)) == 0x00 &&
                !interactionTrackerAsyncOperation.IsCanceled())
            {
                // When a non-canceled operation with a different trigger is encountered, we bail out right away.
                return null;
            }

            ViewChangeBase viewChangeBase = interactionTrackerAsyncOperation.GetViewChangeBase();

            if (((int)(interactionTrackerAsyncOperation.GetOperationTrigger()) & (int)(operationTrigger)) == 0x00 ||
                !interactionTrackerAsyncOperation.IsQueued() ||
                interactionTrackerAsyncOperation.IsUnqueueing() ||
                interactionTrackerAsyncOperation.IsCanceled() ||
                viewChangeBase is null)
            {
                continue;
            }

            switch (interactionTrackerAsyncOperation.GetOperationType())
            {
                case InteractionTrackerAsyncOperationType.TryUpdatePosition:
                case InteractionTrackerAsyncOperationType.TryUpdatePositionBy:
                case InteractionTrackerAsyncOperationType.TryUpdatePositionWithAnimation:
                    {
                        if (!isOperationTypeForOffsetsChange)
                        {
                            continue;
                        }

                        ViewChange viewChange = viewChangeBase as ViewChange;

                        if (viewChange.ViewKind() != viewKind)
                        {
                            continue;
                        }

                        ScrollingScrollOptions optionsClone = viewChange.Options() as ScrollingScrollOptions;
                        ScrollingAnimationMode animationMode = options is not null ? options.AnimationMode : ScrollingAnimationMode.Auto;
                        ScrollingAnimationMode animationModeClone = optionsClone is not null ? optionsClone.AnimationMode : ScrollingAnimationMode.Auto;

                        if (animationModeClone != animationMode)
                        {
                            continue;
                        }

                        ScrollingSnapPointsMode snapPointsMode = options is not null ? options.SnapPointsMode : ScrollingSnapPointsMode.Default;
                        ScrollingSnapPointsMode snapPointsModeClone = optionsClone is not null ? optionsClone.SnapPointsMode : ScrollingSnapPointsMode.Default;

                        if (snapPointsModeClone != snapPointsMode)
                        {
                            continue;
                        }
                        break;
                    }
                case InteractionTrackerAsyncOperationType.TryUpdateScale:
                case InteractionTrackerAsyncOperationType.TryUpdateScaleWithAnimation:
                    {
                        if (isOperationTypeForOffsetsChange)
                        {
                            continue;
                        }

                        ViewChange viewChange = viewChangeBase as ViewChange;

                        if (viewChange.ViewKind() != viewKind)
                        {
                            continue;
                        }

                        ScrollingZoomOptions optionsClone = viewChange.Options() as ScrollingZoomOptions;
                        ScrollingAnimationMode animationMode = options is not null ? options.AnimationMode : ScrollingAnimationMode.Auto;
                        ScrollingAnimationMode animationModeClone = optionsClone is not null ? optionsClone.AnimationMode : ScrollingAnimationMode.Auto;

                        if (animationModeClone != animationMode)
                        {
                            continue;
                        }

                        ScrollingSnapPointsMode snapPointsMode = options is not null ? options.SnapPointsMode : ScrollingSnapPointsMode.Default;
                        ScrollingSnapPointsMode snapPointsModeClone = optionsClone is not null ? optionsClone.SnapPointsMode : ScrollingSnapPointsMode.Default;

                        if (snapPointsModeClone != snapPointsMode)
                        {
                            continue;
                        }
                        break;
                    }
            }

            return interactionTrackerAsyncOperation;
        }

        return null;
    }

    private int GetInteractionTrackerOperationsTicksCountdown()
    {
        int ticksCountdown = 0;

        foreach (var interactionTrackerAsyncOperation in m_interactionTrackerAsyncOperations)
        {
            if (!interactionTrackerAsyncOperation.IsCompleted() &&
                !interactionTrackerAsyncOperation.IsCanceled())
            {
                ticksCountdown = Math.Max(ticksCountdown, interactionTrackerAsyncOperation.GetTicksCountdown());
            }
        }

        return ticksCountdown;
    }

    private InteractionTrackerAsyncOperation GetInteractionTrackerOperationWithAdditionalVelocity(
        bool isOperationTypeForOffsetsChange,
        InteractionTrackerAsyncOperationTrigger operationTrigger)
    {
        foreach (var interactionTrackerAsyncOperation in m_interactionTrackerAsyncOperations)
        {
            ViewChangeBase viewChangeBase = interactionTrackerAsyncOperation.GetViewChangeBase();

            if (((int)(interactionTrackerAsyncOperation.GetOperationTrigger()) & (int)(operationTrigger)) == 0x00 ||
                !interactionTrackerAsyncOperation.IsQueued() ||
                interactionTrackerAsyncOperation.IsUnqueueing() ||
                interactionTrackerAsyncOperation.IsCanceled() ||
                viewChangeBase is null)
            {
                continue;
            }

            switch (interactionTrackerAsyncOperation.GetOperationType())
            {
                case InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity:
                    {
                        if (!isOperationTypeForOffsetsChange)
                        {
                            continue;
                        }
                        return interactionTrackerAsyncOperation;
                    }
                case InteractionTrackerAsyncOperationType.TryUpdateScaleWithAdditionalVelocity:
                    {
                        if (isOperationTypeForOffsetsChange)
                        {
                            continue;
                        }
                        return interactionTrackerAsyncOperation;
                    }
            }
        }

        return null;
    }

    private InteractionTrackerAsyncOperation? GetLastNonAnimatedInteractionTrackerOperation(
        InteractionTrackerAsyncOperation priorToInteractionTrackerOperation)
    {
        bool priorInteractionTrackerOperationSeen = false;

        for (var i = m_interactionTrackerAsyncOperations.Count - 1; i >= 0; i--)
        {
            var interactionTrackerAsyncOperation = m_interactionTrackerAsyncOperations[i];

            if (!priorInteractionTrackerOperationSeen && priorToInteractionTrackerOperation == interactionTrackerAsyncOperation)
            {
                priorInteractionTrackerOperationSeen = true;
            }
            else if (priorInteractionTrackerOperationSeen &&
                !interactionTrackerAsyncOperation.IsAnimated() &&
                !interactionTrackerAsyncOperation.IsCompleted() &&
                !interactionTrackerAsyncOperation.IsCanceled())
            {
                Debug.Assert(interactionTrackerAsyncOperation.IsDelayed() || interactionTrackerAsyncOperation.IsQueued());
                return interactionTrackerAsyncOperation;
            }
        }

        return null;
    }

    private string GetMaxPositionExpression(
        UIElement content)
    {
        return string.Format("Vector3({0}, {1}, 0.0f)", GetMaxPositionXExpression(content), GetMaxPositionYExpression(content));
    }

    private string GetMaxPositionXExpression(
        UIElement content)
    {
        Debug.Assert(content is not null);

        FrameworkElement contentAsFE = content as FrameworkElement;

        if (FlowDirection == FlowDirection.RightToLeft)
        {
            if (contentAsFE is not null)
            {
                string maxOffset = "contentSizeX * it.Scale - scrollPresenterVisual.Size.X";

                if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Center ||
                    contentAsFE.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    return string.Format("-Min(0.0f, ({0}) / 2.0f) - contentLayoutOffsetX", maxOffset);
                }
                else if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    return string.Format("-Min(0.0f, {0}) - contentLayoutOffsetX", maxOffset);
                }
            }

            return ("-contentLayoutOffsetX");
        }

        if (contentAsFE is not null)
        {
            string maxOffset = "(contentSizeX * it.Scale - scrollPresenterVisual.Size.X)";

            if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Center ||
                contentAsFE.HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                return string.Format("{0} >= 0 ? {0} + contentLayoutOffsetX : {0} / 2.0f + contentLayoutOffsetX", maxOffset);
            }
            else if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Right)
            {
                return string.Format("{0} + contentLayoutOffsetX", maxOffset);
            }
        }

        return "Max(0.0f, contentSizeX * it.Scale - scrollPresenterVisual.Size.X) + contentLayoutOffsetX";
    }

    private string GetMaxPositionYExpression(
        UIElement content)
    {
        Debug.Assert(content is not null);

        FrameworkElement contentAsFE = content as FrameworkElement;

        if (contentAsFE is not null)
        {
            string maxOffset = "(contentSizeY * it.Scale - scrollPresenterVisual.Size.Y)";

            if (contentAsFE.VerticalAlignment == VerticalAlignment.Center ||
                contentAsFE.VerticalAlignment == VerticalAlignment.Stretch)
            {
                return string.Format("{0} >= 0 ? {0} + contentLayoutOffsetY : {0} / 2.0f + contentLayoutOffsetY", maxOffset);
            }
            else if (contentAsFE.VerticalAlignment == VerticalAlignment.Bottom)
            {
                return string.Format("{0} + contentLayoutOffsetY", maxOffset);
            }
        }

        return ("Max(0.0f, contentSizeY * it.Scale - scrollPresenterVisual.Size.Y) + contentLayoutOffsetY");
    }

    private string GetMinPositionExpression(UIElement content)
    {
        return string.Format("Vector3({0}, {1}, 0.0f)", GetMinPositionXExpression(content), GetMinPositionYExpression(content));
    }

    private string GetMinPositionXExpression(UIElement content)
    {
        Debug.Assert(content is not null);

        FrameworkElement contentAsFE = content as FrameworkElement;

        if (FlowDirection == FlowDirection.RightToLeft)
        {
            if (contentAsFE is not null)
            {
                string maxOffset = "(contentSizeX * it.Scale - scrollPresenterVisual.Size.X)";

                if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Center ||
                    contentAsFE.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    return string.Format("{0} >= 0 ? -{0} - contentLayoutOffsetX : -{0} / 2.0f - contentLayoutOffsetX", maxOffset);
                }
                else if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    return string.Format("-{0} - contentLayoutOffsetX", maxOffset);
                }
            }

            return ("-Max(0.0f, contentSizeX * it.Scale - scrollPresenterVisual.Size.X) - contentLayoutOffsetX");
        }

        if (contentAsFE is not null)
        {
            string maxOffset = "contentSizeX * it.Scale - scrollPresenterVisual.Size.X";

            if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Center ||
                contentAsFE.HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                return string.Format("Min(0.0f, ({0}) / 2.0f) + contentLayoutOffsetX", maxOffset);
            }
            else if (contentAsFE.HorizontalAlignment == HorizontalAlignment.Right)
            {
                return string.Format("Min(0.0f, {0}) + contentLayoutOffsetX", maxOffset);
            }
        }

        return ("contentLayoutOffsetX");
    }

    private string GetMinPositionYExpression(UIElement content)
    {
        Debug.Assert(content is not null);

        FrameworkElement contentAsFE = content as FrameworkElement;

        if (contentAsFE is not null)
        {
            string maxOffset = "contentSizeY * it.Scale - scrollPresenterVisual.Size.Y";

            if (contentAsFE.VerticalAlignment == VerticalAlignment.Center ||
                contentAsFE.VerticalAlignment == VerticalAlignment.Stretch)
            {
                return string.Format("Min(0.0f, ({0}) / 2.0f) + contentLayoutOffsetY", maxOffset);
            }
            else if (contentAsFE.VerticalAlignment == VerticalAlignment.Bottom)
            {
                return string.Format("Min(0.0f, {0}) + contentLayoutOffsetY", maxOffset);
            }
        }

        return ("contentLayoutOffsetY");
    }

    private int GetNextViewChangeCorrelationId()
    {
        return (m_latestViewChangeCorrelationId == int.MaxValue) ? 0 : m_latestViewChangeCorrelationId + 1;
    }

    private CompositionAnimation GetPositionAnimation(
        double zoomedHorizontalOffset,
        double zoomedVerticalOffset,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        int offsetsChangeCorrelationId)
    {
        Debug.Assert(m_interactionTracker is not null);

        long minDuration = s_offsetsChangeMinMs;
        long maxDuration = s_offsetsChangeMaxMs;
        long unitDuration = s_offsetsChangeMsPerUnit;
        bool isHorizontalScrollControllerRequest = ((char)(operationTrigger) & (char)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest)) > 0;
        bool isVerticalScrollControllerRequest = ((char)(operationTrigger) & (char)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest)) > 0;
        long distance = (long)(Math.Sqrt(Math.Pow(zoomedHorizontalOffset - m_zoomedHorizontalOffset, 2.0) + Math.Pow(zoomedVerticalOffset - m_zoomedVerticalOffset, 2.0)));
        Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        Vector3KeyFrameAnimation positionAnimation = compositor.CreateVector3KeyFrameAnimation();
        ScrollPresenterTestHooks globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();

        if (globalTestHooks is not null)
        {
            int unitDurationTestOverride;
            int minDurationTestOverride;
            int maxDurationTestOverride;

            ScrollPresenterTestHooks.GetOffsetsChangeVelocityParameters(out unitDurationTestOverride, out minDurationTestOverride, out maxDurationTestOverride);

            minDuration = minDurationTestOverride;
            maxDuration = maxDurationTestOverride;
            unitDuration = unitDurationTestOverride;
        }

        Vector2 endPosition = ComputePositionFromOffsets(zoomedHorizontalOffset, zoomedVerticalOffset);

        positionAnimation.InsertKeyFrame(1.0f, new Vector3(endPosition, 0.0f));
        positionAnimation.Duration = (TimeSpan.FromTicks(Math.Clamp(distance * unitDuration, minDuration, maxDuration) * 10000));

        Vector2 startPosition = new Vector2(m_interactionTracker.Position.X, m_interactionTracker.Position.Y);

        if (isHorizontalScrollControllerRequest || isVerticalScrollControllerRequest)
        {
            CompositionAnimation customAnimation = null;

            if (isHorizontalScrollControllerRequest && m_horizontalScrollController is not null)
            {
                customAnimation = m_horizontalScrollController.GetScrollAnimation(
                    offsetsChangeCorrelationId,
                    startPosition,
                    endPosition,
                    positionAnimation);
            }
            if (isVerticalScrollControllerRequest && m_verticalScrollController is not null)
            {
                customAnimation = m_verticalScrollController.GetScrollAnimation(
                    offsetsChangeCorrelationId,
                    startPosition,
                    endPosition,
                    customAnimation is not null ? customAnimation : positionAnimation);
            }
            return customAnimation is not null ? customAnimation : positionAnimation;
        }

        return RaiseScrollAnimationStarting(positionAnimation, startPosition, endPosition, offsetsChangeCorrelationId);
    }

    private CompositionAnimation GetZoomFactorAnimation(
        float zoomFactor,
        Vector2 centerPoint,
        int zoomFactorChangeCorrelationId)
    {
        long minDuration = s_zoomFactorChangeMinMs;
        long maxDuration = s_zoomFactorChangeMaxMs;
        long unitDuration = s_zoomFactorChangeMsPerUnit;
        long distance = (long)(Math.Abs(zoomFactor - m_zoomFactor));
        Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        ScalarKeyFrameAnimation zoomFactorAnimation = compositor.CreateScalarKeyFrameAnimation();
        ScrollPresenterTestHooks globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();

        if (globalTestHooks is not null)
        {
            int unitDurationTestOverride;
            int minDurationTestOverride;
            int maxDurationTestOverride;

            ScrollPresenterTestHooks.GetZoomFactorChangeVelocityParameters(out unitDurationTestOverride, out minDurationTestOverride, out maxDurationTestOverride);

            minDuration = minDurationTestOverride;
            maxDuration = maxDurationTestOverride;
            unitDuration = unitDurationTestOverride;
        }

        zoomFactorAnimation.InsertKeyFrame(1.0f, zoomFactor);
        zoomFactorAnimation.Duration = (TimeSpan.FromTicks(Math.Clamp(distance * unitDuration, minDuration, maxDuration) * 10000));

        return RaiseZoomAnimationStarting(zoomFactorAnimation, zoomFactor, centerPoint, zoomFactorChangeCorrelationId);
    }

    private bool HasBringingIntoViewListener()
    {
        return BringingIntoView != null;
    }

    private void HookCompositionTargetRendering()
    {
        Windows.UI.Xaml.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
    }

    private void HookContentPropertyChanged(UIElement content)
    {
        if (content is not null)
        {
            if (content is FrameworkElement contentAsFE)
            {
                if (m_contentMinWidthChangedRevoker == 0)
                {
                    m_contentMinWidthChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.MinWidthProperty, OnContentPropertyChanged);
                }
                if (m_contentWidthChangedRevoker == 0)
                {
                    m_contentWidthChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.WidthProperty, OnContentPropertyChanged);
                }
                if (m_contentMaxWidthChangedRevoker == 0)
                {
                    m_contentMaxWidthChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.MaxWidthProperty, OnContentPropertyChanged);
                }
                if (m_contentMinHeightChangedRevoker == 0)
                {
                    m_contentMinHeightChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.MinHeightProperty, OnContentPropertyChanged);
                }
                if (m_contentHeightChangedRevoker == 0)
                {
                    m_contentHeightChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.HeightProperty, OnContentPropertyChanged);
                }
                if (m_contentMaxHeightChangedRevoker == 0)
                {
                    m_contentMaxHeightChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.MaxHeightProperty, OnContentPropertyChanged);
                }
                if (m_contentHorizontalAlignmentChangedRevoker == 0)
                {
                    m_contentHorizontalAlignmentChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.HorizontalAlignmentProperty, OnContentPropertyChanged);
                }
                if (m_contentVerticalAlignmentChangedRevoker == 0)
                {
                    m_contentVerticalAlignmentChangedRevoker = contentAsFE.RegisterPropertyChangedCallback(FrameworkElement.VerticalAlignmentProperty, OnContentPropertyChanged);
                }
            }
        }
    }

    private void HookHorizontalScrollControllerEvents(IScrollController horizontalScrollController)
    {
        Debug.Assert(horizontalScrollController is not null);

        horizontalScrollController.ScrollToRequested += OnScrollControllerScrollToRequested;

        horizontalScrollController.ScrollByRequested += OnScrollControllerScrollByRequested;

        horizontalScrollController.AddScrollVelocityRequested += OnScrollControllerAddScrollVelocityRequested;
    }

    private void HookHorizontalScrollControllerInteractionSourceEvents(IScrollControllerPanningInfo horizontalScrollControllerPanningInfo)
    {
        Debug.Assert(horizontalScrollControllerPanningInfo is not null);

        horizontalScrollControllerPanningInfo.PanRequested += OnScrollControllerPanningInfoPanRequested;
    }

    private void HookHorizontalScrollControllerPanningInfoEvents(
        IScrollControllerPanningInfo horizontalScrollControllerPanningInfo,
        bool hasInteractionSource)
    {
        Debug.Assert(horizontalScrollControllerPanningInfo is not null);

        if (hasInteractionSource)
        {
            HookHorizontalScrollControllerInteractionSourceEvents(horizontalScrollControllerPanningInfo);
        }

        horizontalScrollControllerPanningInfo.Changed += OnScrollControllerPanningInfoChanged;
    }

    private void HookScrollPresenterEvents()
    {
        if (m_flowDirectionChangedRevoker == 0)
        {
            m_flowDirectionChangedRevoker = RegisterPropertyChangedCallback(FlowDirectionProperty, OnFlowDirectionChanged);
        }

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        BringIntoViewRequested += OnBringIntoViewRequestedHandler;

        if (m_pointerPressedEventHandler is null)
        {
            m_pointerPressedEventHandler = new PointerEventHandler(OnPointerPressed);
            Debug.Assert(m_pointerPressedEventHandler is not null);
            AddHandler(UIElement.PointerPressedEvent, m_pointerPressedEventHandler, true /*handledEventsToo*/);
        }
    }

    private void HookVerticalScrollControllerEvents(
        IScrollController verticalScrollController)
    {
        Debug.Assert(verticalScrollController is not null);

        verticalScrollController.ScrollToRequested += OnScrollControllerScrollToRequested;

        verticalScrollController.ScrollByRequested += OnScrollControllerScrollByRequested;

        verticalScrollController.AddScrollVelocityRequested += OnScrollControllerAddScrollVelocityRequested;
    }

    private void HookVerticalScrollControllerInteractionSourceEvents(
        IScrollControllerPanningInfo verticalScrollControllerPanningInfo)
    {
        Debug.Assert(verticalScrollControllerPanningInfo is not null);

        verticalScrollControllerPanningInfo.PanRequested += OnScrollControllerPanningInfoPanRequested;
    }

    private void HookVerticalScrollControllerPanningInfoEvents(
        IScrollControllerPanningInfo verticalScrollControllerPanningInfo,
        bool hasInteractionSource)
    {
        Debug.Assert(verticalScrollControllerPanningInfo is not null);

        if (hasInteractionSource)
        {
            HookVerticalScrollControllerInteractionSourceEvents(verticalScrollControllerPanningInfo);
        }

        verticalScrollControllerPanningInfo.Changed += OnScrollControllerPanningInfoChanged;
    }

    private const double c_edgeDetectionTolerance = 0.1;

    private void IsAnchoring(
        out bool isAnchoringElementHorizontally,
        out bool isAnchoringElementVertically,
        out bool isAnchoringFarEdgeHorizontally,
        out bool isAnchoringFarEdgeVertically)
    {
        isAnchoringElementHorizontally = false;
        isAnchoringElementVertically = false;

        {
            isAnchoringFarEdgeHorizontally = false;
        }

        {
            isAnchoringFarEdgeVertically = false;
        }

        // Mouse wheel comes in as a custom animation, and we are currently not 
        // anchoring because of the check below. Unfortunately, I cannot validate that
        // removing the check is the correct fix due to dcomp bug 17523225. I filed a 
        // tracking bug to follow up once the dcomp bug is fixed.
        // Bug 17523266: ScrollPresenter is not anchoring during mouse wheel
        if (m_interactionTracker is null || m_state == ScrollingInteractionState.Animation)
        {
            // Skip calls to SetContentLayoutOffsetX / SetContentLayoutOffsetY when the InteractionTracker has not been set up yet,
            // or when it is performing a custom animation because if would result in a visual flicker.
            return;
        }

        double horizontalAnchorRatio = HorizontalAnchorRatio;
        double verticalAnchorRatio = VerticalAnchorRatio;

        // For edge anchoring, the near edge is considered when HorizontalAnchorRatio or VerticalAnchorRatio is 0.0. 
        // When the property is 1.0, the far edge is considered.
        if (!double.IsNaN(horizontalAnchorRatio))
        {
            Debug.Assert(horizontalAnchorRatio >= 0.0);
            Debug.Assert(horizontalAnchorRatio <= 1.0);

            if (horizontalAnchorRatio == 0.0 || horizontalAnchorRatio == 1.0)
            {
                if (horizontalAnchorRatio == 1.0 && m_zoomedHorizontalOffset + m_viewportWidth - m_unzoomedExtentWidth * m_zoomFactor > -c_edgeDetectionTolerance)
                {
                    if (isAnchoringFarEdgeHorizontally)
                    {
                        isAnchoringFarEdgeHorizontally = true;
                    }
                }
                else if (!(horizontalAnchorRatio == 0.0 && m_zoomedHorizontalOffset < c_edgeDetectionTolerance))
                {
                    isAnchoringElementHorizontally = true;
                }
            }
            else
            {
                isAnchoringElementHorizontally = true;
            }
        }

        if (!double.IsNaN(verticalAnchorRatio))
        {
            Debug.Assert(verticalAnchorRatio >= 0.0);
            Debug.Assert(verticalAnchorRatio <= 1.0);

            if (verticalAnchorRatio == 0.0 || verticalAnchorRatio == 1.0)
            {
                if (verticalAnchorRatio == 1.0 && m_zoomedVerticalOffset + m_viewportHeight - m_unzoomedExtentHeight * m_zoomFactor > -c_edgeDetectionTolerance)
                {
                    if (isAnchoringFarEdgeVertically)
                    {
                        isAnchoringFarEdgeVertically = true;
                    }
                }
                else if (!(verticalAnchorRatio == 0.0 && m_zoomedVerticalOffset < c_edgeDetectionTolerance))
                {
                    isAnchoringElementVertically = true;
                }
            }
            else
            {
                isAnchoringElementVertically = true;
            }
        }
    }

    private bool IsElementValidAnchor(UIElement element)
    {
        return IsElementValidAnchor(element, Content);
    }

    private bool IsInputKindIgnored(ScrollingInputKinds inputKind)
    {
        return (IgnoredInputKinds & inputKind) == inputKind;
    }

    private bool IsLoadedAndSetUp()
    {
        return IsLoaded && m_interactionTracker is not null;
    }

    private void MaximizeInteractionTrackerOperationsTicksCountdown()
    {
        if (m_interactionTrackerAsyncOperations.Count <= 0)
        {
            return;
        }

        foreach (var operationsIter in m_interactionTrackerAsyncOperations)
        {
            var interactionTrackerAsyncOperation = operationsIter;

            if (!interactionTrackerAsyncOperation.IsDelayed() &&
                !interactionTrackerAsyncOperation.IsCanceled() &&
                !interactionTrackerAsyncOperation.IsCompleted() &&
                interactionTrackerAsyncOperation.IsQueued())
            {
                interactionTrackerAsyncOperation.SetMaxTicksCountdown();
            }
        }
    }

    private void OnBringIntoViewRequestedHandler(
        object sender,
        BringIntoViewRequestedEventArgs args)
    {
        UIElement content = Content;

        if (args.Handled ||
            args.TargetElement == (this) ||
            (args.TargetElement == content && content.Visibility == Visibility.Collapsed) ||
            (args.TargetElement != content && !SharedHelpers.IsAncestor(args.TargetElement, content, true /*checkVisibility*/)))
        {
            // Ignore the request when:
            // - There is no InteractionTracker to fulfill it.
            // - It was handled already.
            // - The target element is this ScrollPresenter itself. A parent scrollPresenter may fulfill the request instead then.
            // - The target element is effectively collapsed within the ScrollPresenter.
            return;
        }

        Rect targetRect = default;
        int offsetsChangeCorrelationId = s_noOpCorrelationId;
        double targetZoomedHorizontalOffset = 0.0;
        double targetZoomedVerticalOffset = 0.0;
        double appliedOffsetX = 0.0;
        double appliedOffsetY = 0.0;
        ScrollingSnapPointsMode snapPointsMode = ScrollingSnapPointsMode.Ignore;

        // Compute the target offsets based on the provided BringIntoViewRequestedEventArgs.
        ComputeBringIntoViewTargetOffsetsFromRequestEventArgs(
            content,
            snapPointsMode,
            args,
            out targetZoomedHorizontalOffset,
            out targetZoomedVerticalOffset,
            out appliedOffsetX,
            out appliedOffsetY,
            out targetRect);

        if (HasBringingIntoViewListener())
        {
            // Raise the ScrollPresenter.BringingIntoView event to give the listeners a chance to adjust the operation.

            offsetsChangeCorrelationId = m_latestViewChangeCorrelationId = GetNextViewChangeCorrelationId();

            if (!RaiseBringingIntoView(
                targetZoomedHorizontalOffset,
                targetZoomedVerticalOffset,
                args,
                offsetsChangeCorrelationId,
                ref snapPointsMode))
            {
                // A listener canceled the operation in the ScrollPresenter.BringingIntoView event handler before any scrolling was attempted.
                RaiseViewChangeCompleted(true /*isForScroll*/, ScrollPresenterViewChangeResult.Completed, offsetsChangeCorrelationId);
                return;
            }

            content = Content;

            if (content is null ||
                args.Handled ||
                args.TargetElement == (this) ||
                (args.TargetElement == content && content.Visibility == Visibility.Collapsed) ||
                (args.TargetElement != content && !SharedHelpers.IsAncestor(args.TargetElement, content, true /*checkVisibility*/)))
            {
                // Again, ignore the request when:
                // - There is no Content anymore.
                // - The request was handled already.
                // - The target element is this ScrollPresenter itself. A parent scrollPresenter may fulfill the request instead then.
                // - The target element is effectively collapsed within the ScrollPresenter.
                return;
            }

            // Re-evaluate the target offsets based on the potentially modified BringIntoViewRequestedEventArgs.
            // Take into account potential SnapPointsMode == Default so that parents contribute accordingly.
            ComputeBringIntoViewTargetOffsetsFromRequestEventArgs(
                content,
                snapPointsMode,
                args,
                out targetZoomedHorizontalOffset,
                out targetZoomedVerticalOffset,
                out appliedOffsetX,
                out appliedOffsetY,
                out targetRect);
        }

        // Do not include the applied offsets so that potential parent bring-into-view contributors ignore that shift.
        Rect nextTargetRect = new Rect(
            (float)(targetRect.X * m_zoomFactor - targetZoomedHorizontalOffset - appliedOffsetX),
        (float)(targetRect.Y * m_zoomFactor - targetZoomedVerticalOffset - appliedOffsetY),
        Math.Min(targetRect.Width * m_zoomFactor, (float)(m_viewportWidth)),
        Math.Min(targetRect.Height * m_zoomFactor, (float)(m_viewportHeight))
        );

        Rect viewportRect = new Rect(
            0.0f,
        0.0f,
        (float)(m_viewportWidth),
        (float)(m_viewportHeight)
    );

        if (targetZoomedHorizontalOffset != m_zoomedHorizontalOffset ||
            targetZoomedVerticalOffset != m_zoomedVerticalOffset)
        {
            ScrollingScrollOptions options =
                new ScrollingScrollOptions(
                    args.AnimationDesired ? ScrollingAnimationMode.Auto : ScrollingAnimationMode.Disabled,
                    snapPointsMode);

            ChangeOffsetsPrivate(
                targetZoomedHorizontalOffset /*zoomedHorizontalOffset*/,
                targetZoomedVerticalOffset /*zoomedVerticalOffset*/,
                ScrollPresenterViewKind.Absolute,
                options,
                args /*bringIntoViewRequestedEventArgs*/,
                InteractionTrackerAsyncOperationTrigger.BringIntoViewRequest,
                offsetsChangeCorrelationId /*existingViewChangeCorrelationId*/,
                out _ /*viewChangeCorrelationId*/);
        }
        else
        {
            // No offset change was triggered because the target offsets are the same as the current ones. Mark the operation as completed immediately.
            RaiseViewChangeCompleted(true /*isForScroll*/, ScrollPresenterViewChangeResult.Completed, offsetsChangeCorrelationId);
        }

        if (SharedHelpers.DoRectsIntersect(nextTargetRect, viewportRect))
        {
            // Next bring a portion of this ScrollPresenter into view.
            args.TargetRect = (nextTargetRect);
            args.TargetElement = (this);
            args.HorizontalOffset = (args.HorizontalOffset - appliedOffsetX);
            args.VerticalOffset = (args.VerticalOffset - appliedOffsetY);
        }
        else
        {
            // This ScrollPresenter did not even partially bring the TargetRect into its viewport.
            // Mark the operation as handled since no portion of this ScrollPresenter needs to be brought into view.
            args.Handled = (true);
        }
    }

    private void OnCompositionTargetRendering(object sender, object args)
    {
        bool unhookCompositionTargetRendering = StartTranslationAndZoomFactorExpressionAnimations();

        if (m_interactionTrackerAsyncOperations.Count > 0 && IsLoaded)
        {
            bool delayProcessingViewChanges = false;

            for (var i = 0; i < m_interactionTrackerAsyncOperations.Count; i++)
            {
                var interactionTrackerAsyncOperation = m_interactionTrackerAsyncOperations[i];

                if (interactionTrackerAsyncOperation.IsDelayed())
                {
                    interactionTrackerAsyncOperation.SetIsDelayed(false);
                    unhookCompositionTargetRendering = false;
                    Debug.Assert(interactionTrackerAsyncOperation.IsQueued());
                }
                else if (interactionTrackerAsyncOperation.IsQueued())
                {
                    if (!delayProcessingViewChanges && interactionTrackerAsyncOperation.GetTicksCountdown() == 1)
                    {
                        // Evaluate whether all remaining queued operations need to be delayed until the completion of a prior required operation.
                        InteractionTrackerAsyncOperation requiredInteractionTrackerAsyncOperation = interactionTrackerAsyncOperation.GetRequiredOperation();

                        if (requiredInteractionTrackerAsyncOperation is not null)
                        {
                            if (!requiredInteractionTrackerAsyncOperation.IsCanceled() && !requiredInteractionTrackerAsyncOperation.IsCompleted())
                            {
                                // Prior required operation is not canceled or completed yet. All subsequent operations need to be delayed.
                                delayProcessingViewChanges = true;
                            }
                            else
                            {
                                // Previously set required operation is now canceled or completed. Check if it needs to be replaced with an older one.
                                requiredInteractionTrackerAsyncOperation = GetLastNonAnimatedInteractionTrackerOperation(interactionTrackerAsyncOperation);
                                interactionTrackerAsyncOperation.SetRequiredOperation(requiredInteractionTrackerAsyncOperation);
                                if (requiredInteractionTrackerAsyncOperation is not null)
                                {
                                    // An older operation is now required. All subsequent operations need to be delayed.
                                    delayProcessingViewChanges = true;
                                }
                            }
                        }
                    }

                    if (delayProcessingViewChanges)
                    {
                        if (interactionTrackerAsyncOperation.GetTicksCountdown() > 1)
                        {
                            // Ticking the queued operation without processing it.
                            interactionTrackerAsyncOperation.TickQueuedOperation();
                        }
                        unhookCompositionTargetRendering = false;
                    }
                    else if (interactionTrackerAsyncOperation.TickQueuedOperation())
                    {
                        // InteractionTracker is ready for the operation's processing.
                        ProcessDequeuedViewChange(interactionTrackerAsyncOperation);
                        if (!interactionTrackerAsyncOperation.IsAnimated())
                        {
                            unhookCompositionTargetRendering = false;
                        }
                    }
                    else
                    {
                        unhookCompositionTargetRendering = false;
                    }
                }
                else if (!interactionTrackerAsyncOperation.IsAnimated())
                {
                    if (interactionTrackerAsyncOperation.TickNonAnimatedOperation())
                    {
                        // The non-animated view change request did not result in a status change or ValuesChanged notification. Consider it completed.
                        CompleteViewChange(interactionTrackerAsyncOperation, ScrollPresenterViewChangeResult.Completed);
                        if (m_translationAndZoomFactorAnimationsRestartTicksCountdown > 0)
                        {
                            // Do not unhook the Rendering event when there is a pending restart of the Translation and Scale animations.
                            unhookCompositionTargetRendering = false;
                        }
                        m_interactionTrackerAsyncOperations.Remove(interactionTrackerAsyncOperation);
                        i--;
                    }
                    else
                    {
                        unhookCompositionTargetRendering = false;
                    }
                }
            }
        }

        if (unhookCompositionTargetRendering)
        {
            UnhookCompositionTargetRendering();
        }
    }

    private bool IsScrollPresenterTracingEnabled()
    {
        return false;
    }

    internal void ValuesChanged(
     InteractionTrackerValuesChangedArgs args)
    {
        bool isScrollPresenterTracingEnabled = IsScrollPresenterTracingEnabled();


        int requestId = args.RequestId;

        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = GetInteractionTrackerOperationFromRequestId(requestId);

        bool isRightToLeftDirection = FlowDirection == FlowDirection.RightToLeft;
        double oldZoomedHorizontalOffset = m_zoomedHorizontalOffset;
        double oldZoomedVerticalOffset = m_zoomedVerticalOffset;
        float oldZoomFactor = m_zoomFactor;
        Vector2 minPosition = default;
        Vector2 maxPosition = default;

        m_zoomFactor = args.Scale;

        if (isRightToLeftDirection)
        {
            ComputeMinMaxPositions(m_zoomFactor, out minPosition, out maxPosition);
        }
        else
        {
            ComputeMinMaxPositions(m_zoomFactor, out minPosition, out _);
        }

        if (isRightToLeftDirection)
        {
            UpdateOffset(ScrollPresenterDimension.HorizontalScroll, maxPosition.X - args.Position.X);
        }
        else
        {
            UpdateOffset(ScrollPresenterDimension.HorizontalScroll, args.Position.X - minPosition.X);
        }

        UpdateOffset(ScrollPresenterDimension.VerticalScroll, args.Position.Y - minPosition.Y);

        if (oldZoomFactor != m_zoomFactor || oldZoomedHorizontalOffset != m_zoomedHorizontalOffset || oldZoomedVerticalOffset != m_zoomedVerticalOffset)
        {
            OnViewChanged(oldZoomedHorizontalOffset != m_zoomedHorizontalOffset /*horizontalOffsetChanged*/,
                oldZoomedVerticalOffset != m_zoomedVerticalOffset /*verticalOffsetChanged*/);
        }

        if (requestId != 0 && m_interactionTrackerAsyncOperations.Count > 0)
        {
            CompleteInteractionTrackerOperations(
                requestId,
                ScrollPresenterViewChangeResult.Completed   /*operationResult*/,
                ScrollPresenterViewChangeResult.Completed   /*priorNonAnimatedOperationsResult*/,
                ScrollPresenterViewChangeResult.Interrupted /*priorAnimatedOperationsResult*/,
                true  /*completeNonAnimatedOperation*/,
                false /*completeAnimatedOperation*/,
                true  /*completePriorNonAnimatedOperations*/,
                true  /*completePriorAnimatedOperations*/);
        }
    }


    private void OnContentLayoutOffsetChanged(ScrollPresenterDimension dimension)
    {
        ScrollPresenterTestHooks globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();

        if (globalTestHooks is not null)
        {
            if (dimension == ScrollPresenterDimension.HorizontalScroll)
            {
                ScrollPresenterTestHooks.NotifyContentLayoutOffsetXChanged(this);
            }
            else
            {
                ScrollPresenterTestHooks.NotifyContentLayoutOffsetYChanged(this);
            }
        }

        if (m_minPositionExpressionAnimation is not null && m_maxPositionExpressionAnimation is not null)
        {
            UIElement? content = Content;

            if (content is not null)
            {
                UpdatePositionBoundaries(content);
            }
        }

        if (m_expressionAnimationSources is not null)
        {
            m_expressionAnimationSources.InsertVector2(s_offsetSourcePropertyName, new Vector2(m_contentLayoutOffsetX, m_contentLayoutOffsetY));
        }

        if (m_scrollPresenterVisualInteractionSource is not null)
        {
            SetupVisualInteractionSourceCenterPointModifier(
                m_scrollPresenterVisualInteractionSource,
                dimension,
                false /*flowDirectionChanged*/);
        }
    }

    private void OnContentPropertyChanged(DependencyObject sender, DependencyProperty args)
    {
        UIElement content = Content;

        if (content is not null)
        {
            if (args == FrameworkElement.HorizontalAlignmentProperty ||
                args == FrameworkElement.VerticalAlignmentProperty)
            {
                // The ExtentWidth and ExtentHeight may have to be updated because of this alignment change.
                InvalidateMeasure();

                if (m_interactionTracker is not null)
                {
                    if (m_minPositionExpressionAnimation is not null && m_maxPositionExpressionAnimation is not null)
                    {
                        SetupPositionBoundariesExpressionAnimations(content);
                    }

                    if (m_translationExpressionAnimation is not null && m_zoomFactorExpressionAnimation is not null)
                    {
                        SetupTransformExpressionAnimations(content);
                    }
                }
            }
            else if (args == FrameworkElement.MinWidthProperty ||
                args == FrameworkElement.WidthProperty ||
                args == FrameworkElement.MaxWidthProperty ||
                args == FrameworkElement.MinHeightProperty ||
                args == FrameworkElement.HeightProperty ||
                args == FrameworkElement.MaxHeightProperty)
            {
                InvalidateMeasure();
            }
        }
    }

    private void OnContentSizeChanged(UIElement content)
    {
        if (m_minPositionExpressionAnimation is not null && m_maxPositionExpressionAnimation is not null)
        {
            UpdatePositionBoundaries(content);
        }

        if (m_interactionTracker is not null && m_translationExpressionAnimation is not null && m_zoomFactorExpressionAnimation is not null)
        {
            SetupTransformExpressionAnimations(content);
        }
    }

    private void OnFlowDirectionChanged(DependencyObject sender, DependencyProperty args)
    {
        if (m_interactionTracker is not null)
        {
            if (Content is UIElement content)
            {
                if (m_minPositionExpressionAnimation is not null && m_maxPositionExpressionAnimation is not null)
                {
                    SetupPositionBoundariesExpressionAnimations(content);
                }

                if (m_translationExpressionAnimation is not null && m_zoomFactorExpressionAnimation is not null)
                {
                    SetupTransformExpressionAnimations(content);
                }
            }

            if (m_scrollPresenterVisualInteractionSource is not null)
            {
                // When the direction is RightToLeft, the center point modifier is function of the ScrollPresenter width, so it needs to be updated.
                SetupVisualInteractionSourceCenterPointModifier(
                    m_scrollPresenterVisualInteractionSource,
                    ScrollPresenterDimension.HorizontalScroll,
                    true /*flowDirectionChanged*/);
            }

            // The updates above reset the horizontal HorizontalOffset is 0, so it is brought back to its original value through a non-animated scroll.
            if (m_zoomedHorizontalOffset > 0.0)
            {
                ScrollingScrollOptions options = new ScrollingScrollOptions(ScrollingAnimationMode.Disabled, ScrollingSnapPointsMode.Ignore);

                ScrollTo(m_zoomedHorizontalOffset, m_zoomedVerticalOffset, options);
            }
        }
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
        var dependencyProperty = args.Property;

        if (dependencyProperty == ContentProperty)
        {
            object oldContent = args.OldValue;
            object newContent = args.NewValue;
            UpdateContent(oldContent as UIElement, newContent as UIElement);
        }
        else if (dependencyProperty == BackgroundProperty)
        {
            Panel thisAsPanel = this;

            thisAsPanel.Background = (args.NewValue as Brush);
        }
        else if (dependencyProperty == MinZoomFactorProperty || dependencyProperty == MaxZoomFactorProperty)
        {
            Debug.Assert(IsZoomFactorBoundaryValid((double)(args.OldValue)));

            if (m_interactionTracker is not null)
            {
                SetupInteractionTrackerZoomFactorBoundaries(
                    MinZoomFactor,
                    MaxZoomFactor);
            }
        }
        else if (dependencyProperty == ContentOrientationProperty)
        {
            m_contentOrientation = ContentOrientation;

            InvalidateMeasure();
        }
        else if (dependencyProperty == HorizontalAnchorRatioProperty ||
            dependencyProperty == VerticalAnchorRatioProperty)
        {
            Debug.Assert(IsAnchorRatioValid((double)(args.OldValue)));

            m_isAnchorElementDirty = true;
        }
        else if (m_scrollPresenterVisualInteractionSource is not null)
        {
            if (dependencyProperty == HorizontalScrollChainModeProperty)
            {
                SetupVisualInteractionSourceChainingMode(
                    m_scrollPresenterVisualInteractionSource,
                    ScrollPresenterDimension.HorizontalScroll,
                    HorizontalScrollChainMode);
            }
            else if (dependencyProperty == VerticalScrollChainModeProperty)
            {
                SetupVisualInteractionSourceChainingMode(
                    m_scrollPresenterVisualInteractionSource,
                    ScrollPresenterDimension.VerticalScroll,
                    VerticalScrollChainMode);
            }
            else if (dependencyProperty == ZoomChainModeProperty)
            {
                SetupVisualInteractionSourceChainingMode(
                    m_scrollPresenterVisualInteractionSource,
                    ScrollPresenterDimension.ZoomFactor,
                    ZoomChainMode);
            }
            else if (dependencyProperty == HorizontalScrollRailModeProperty)
            {
                SetupVisualInteractionSourceRailingMode(
                    m_scrollPresenterVisualInteractionSource,
                    ScrollPresenterDimension.HorizontalScroll,
                    HorizontalScrollRailMode);
            }
            else if (dependencyProperty == VerticalScrollRailModeProperty)
            {
                SetupVisualInteractionSourceRailingMode(
                    m_scrollPresenterVisualInteractionSource,
                    ScrollPresenterDimension.VerticalScroll,
                    VerticalScrollRailMode);
            }
            else if (dependencyProperty == HorizontalScrollModeProperty)
            {
                UpdateVisualInteractionSourceMode(
                    ScrollPresenterDimension.HorizontalScroll);
            }
            else if (dependencyProperty == VerticalScrollModeProperty)
            {
                UpdateVisualInteractionSourceMode(
                    ScrollPresenterDimension.VerticalScroll);
            }
            else if (dependencyProperty == ZoomModeProperty)
            {
                // Updating the horizontal and vertical scroll modes because GetComputedScrollMode is function of ZoomMode.
                UpdateVisualInteractionSourceMode(
                    ScrollPresenterDimension.HorizontalScroll);
                UpdateVisualInteractionSourceMode(
                    ScrollPresenterDimension.VerticalScroll);

                SetupVisualInteractionSourceMode(
                    m_scrollPresenterVisualInteractionSource,
                    ZoomMode);
            }
            else if (dependencyProperty == IgnoredInputKindsProperty)
            {
                UpdateManipulationRedirectionMode();
            }
        }
    }

    private void OnScrollControllerAddScrollVelocityRequested(
        IScrollController sender,
        ScrollControllerAddScrollVelocityRequestedEventArgs args)
    {
        Debug.Assert(sender == m_horizontalScrollController || sender == m_verticalScrollController);

        bool isFromHorizontalScrollController = sender == m_horizontalScrollController;
        int viewChangeCorrelationId = s_noOpCorrelationId;
        float? horizontalInertiaDecayRate = null;
        float? verticalInertiaDecayRate = null;

        // Attempt to find an offset change with velocity request from an IScrollController and this same tick.
        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = GetInteractionTrackerOperationWithAdditionalVelocity(
            true /*isOperationTypeForOffsetsChange*/,
            (InteractionTrackerAsyncOperationTrigger)((int)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest) + (int)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest)));

        if (interactionTrackerAsyncOperation is null)
        {
            Vector2? inertiaDecayRate = null;
            Vector2 offsetsVelocity = default;

            if (isFromHorizontalScrollController)
            {
                offsetsVelocity.X = args.OffsetVelocity;
                horizontalInertiaDecayRate = args.InertiaDecayRate;
            }
            else
            {
                offsetsVelocity.Y = args.OffsetVelocity;
                verticalInertiaDecayRate = args.InertiaDecayRate;
            }

            if (horizontalInertiaDecayRate.HasValue || verticalInertiaDecayRate.HasValue)
            {
                object inertiaDecayRateAsInsp = null;

                if (horizontalInertiaDecayRate.HasValue)
                {
                    inertiaDecayRateAsInsp = new Vector2(horizontalInertiaDecayRate.Value, c_scrollPresenterDefaultInertiaDecayRate);
                }
                else
                {
                    inertiaDecayRateAsInsp = new Vector2(c_scrollPresenterDefaultInertiaDecayRate, verticalInertiaDecayRate.Value);
                }

                inertiaDecayRate = inertiaDecayRateAsInsp as Vector2?;
            }

            ChangeOffsetsWithAdditionalVelocityPrivate(
                offsetsVelocity,
                Vector2.Zero /*anticipatedOffsetsChange*/,
                inertiaDecayRate,
                isFromHorizontalScrollController ? InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest : InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest,
                out viewChangeCorrelationId);
        }
        else
        {
            // Coalesce requests
            int existingViewChangeCorrelationId = interactionTrackerAsyncOperation.GetViewChangeCorrelationId();
            ViewChangeBase viewChangeBase = interactionTrackerAsyncOperation.GetViewChangeBase();
            OffsetsChangeWithAdditionalVelocity offsetsChangeWithAdditionalVelocity = (viewChangeBase) as OffsetsChangeWithAdditionalVelocity;

            Vector2 offsetsVelocity = offsetsChangeWithAdditionalVelocity.OffsetsVelocity();
            Vector2? inertiaDecayRate = offsetsChangeWithAdditionalVelocity.InertiaDecayRate();

            interactionTrackerAsyncOperation.SetIsScrollControllerRequest(isFromHorizontalScrollController);

            if (isFromHorizontalScrollController)
            {
                offsetsVelocity.X = args.OffsetVelocity;
                horizontalInertiaDecayRate = args.InertiaDecayRate;

                if (!horizontalInertiaDecayRate.HasValue)
                {
                    if (inertiaDecayRate.HasValue)
                    {
                        if (inertiaDecayRate.Value.Y == c_scrollPresenterDefaultInertiaDecayRate)
                        {
                            offsetsChangeWithAdditionalVelocity.InertiaDecayRate(null);
                        }
                        else
                        {
                            object newInertiaDecayRateAsInsp =
                                new Vector2(c_scrollPresenterDefaultInertiaDecayRate, inertiaDecayRate.Value.Y);
                            Vector2? newInertiaDecayRate =
                                newInertiaDecayRateAsInsp as Vector2?;

                            offsetsChangeWithAdditionalVelocity.InertiaDecayRate(newInertiaDecayRate);
                        }
                    }
                }
                else
                {
                    object newInertiaDecayRateAsInsp = null;

                    if (!inertiaDecayRate.HasValue)
                    {
                        newInertiaDecayRateAsInsp =
                            new Vector2(horizontalInertiaDecayRate.Value, c_scrollPresenterDefaultInertiaDecayRate);
                    }
                    else
                    {
                        newInertiaDecayRateAsInsp =
                            new Vector2(horizontalInertiaDecayRate.Value, inertiaDecayRate.Value.Y);
                    }

                    Vector2? newInertiaDecayRate = newInertiaDecayRateAsInsp as Vector2?;

                    offsetsChangeWithAdditionalVelocity.InertiaDecayRate(newInertiaDecayRate);
                }
            }
            else
            {
                offsetsVelocity.Y = args.OffsetVelocity;
                verticalInertiaDecayRate = args.InertiaDecayRate;

                if (!verticalInertiaDecayRate.HasValue)
                {
                    if (inertiaDecayRate.HasValue)
                    {
                        if (inertiaDecayRate.Value.X == c_scrollPresenterDefaultInertiaDecayRate)
                        {
                            offsetsChangeWithAdditionalVelocity.InertiaDecayRate(null);
                        }
                        else
                        {
                            object newInertiaDecayRateAsInsp =
                                new Vector2(inertiaDecayRate.Value.X, c_scrollPresenterDefaultInertiaDecayRate);
                            Vector2? newInertiaDecayRate =
                                newInertiaDecayRateAsInsp as Vector2?;

                            offsetsChangeWithAdditionalVelocity.InertiaDecayRate(newInertiaDecayRate);
                        }
                    }
                }
                else
                {
                    object newInertiaDecayRateAsInsp = null;

                    if (!inertiaDecayRate.HasValue)
                    {
                        newInertiaDecayRateAsInsp =
                            new Vector2(c_scrollPresenterDefaultInertiaDecayRate, verticalInertiaDecayRate.Value);
                    }
                    else
                    {
                        newInertiaDecayRateAsInsp =
                            new Vector2(inertiaDecayRate.Value.X, verticalInertiaDecayRate.Value);
                    }

                    Vector2? newInertiaDecayRate = newInertiaDecayRateAsInsp as Vector2?;

                    offsetsChangeWithAdditionalVelocity.InertiaDecayRate(newInertiaDecayRate);
                }
            }

            offsetsChangeWithAdditionalVelocity.OffsetsVelocity(offsetsVelocity);

            viewChangeCorrelationId = existingViewChangeCorrelationId;
        }

        if (viewChangeCorrelationId != s_noOpCorrelationId)
        {
            args.CorrelationId = (viewChangeCorrelationId);
        }
    }

    private void OnScrollControllerPanningInfoChanged(
        IScrollControllerPanningInfo sender,
        object args)
    {
        Debug.Assert(sender == m_horizontalScrollControllerPanningInfo || sender == m_verticalScrollControllerPanningInfo);

        if (m_interactionTracker is null)
        {
            return;
        }

        bool isFromHorizontalScrollController = sender == m_horizontalScrollControllerPanningInfo;

        CompositionPropertySet scrollControllerExpressionAnimationSources =
            isFromHorizontalScrollController ? m_horizontalScrollControllerExpressionAnimationSources : m_verticalScrollControllerExpressionAnimationSources;

        SetupScrollControllerVisualInterationSource(isFromHorizontalScrollController ? ScrollPresenterDimension.HorizontalScroll : ScrollPresenterDimension.VerticalScroll);

        if (isFromHorizontalScrollController)
        {
            if (scrollControllerExpressionAnimationSources != m_horizontalScrollControllerExpressionAnimationSources)
            {
                Debug.Assert(m_horizontalScrollControllerPanningInfo is not null);

                m_horizontalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                    m_horizontalScrollControllerExpressionAnimationSources,
                    s_minOffsetPropertyName,
                    s_maxOffsetPropertyName,
                    s_offsetPropertyName,
                    s_multiplierPropertyName);
            }
        }
        else
        {
            if (scrollControllerExpressionAnimationSources != m_verticalScrollControllerExpressionAnimationSources)
            {
                Debug.Assert(m_verticalScrollControllerPanningInfo is not null);

                m_verticalScrollControllerPanningInfo.SetPanningElementExpressionAnimationSources(
                    m_verticalScrollControllerExpressionAnimationSources,
                    s_minOffsetPropertyName,
                    s_maxOffsetPropertyName,
                    s_offsetPropertyName,
                    s_multiplierPropertyName);
            }
        }
    }

    private void OnScrollControllerPanningInfoPanRequested(
        IScrollControllerPanningInfo sender,
        ScrollControllerPanRequestedEventArgs args)
    {
        Debug.Assert(sender == m_horizontalScrollControllerPanningInfo || sender == m_verticalScrollControllerPanningInfo);

        if (args.Handled)
        {
            return;
        }

        VisualInteractionSource scrollControllerVisualInteractionSource = null;

        if (sender == m_horizontalScrollControllerPanningInfo)
        {
            scrollControllerVisualInteractionSource = m_horizontalScrollControllerVisualInteractionSource;
        }
        else
        {
            scrollControllerVisualInteractionSource = m_verticalScrollControllerVisualInteractionSource;
        }

        if (scrollControllerVisualInteractionSource is not null)
        {
            try
            {
                scrollControllerVisualInteractionSource.TryRedirectForManipulation(args.PointerPoint);
            }
            catch (Exception e)
            {
                // Swallowing Access Denied error because of InteractionTracker bug 17434718 which has been
                // causing crashes at least in RS3, RS4 and RS5.
                // TODO - Stop eating the error in future OS versions that include a fix for 17434718 if any.
                if ((uint)e.HResult == E_ACCESSDENIED)
                {
                    // Do not set the Handled flag. The request is simply ignored.
                    return;
                }
                else
                {
                    throw;
                }
            }
            args.Handled = (true);
        }
    }

    private void OnScrollControllerScrollByRequested(
        IScrollController sender,
        ScrollControllerScrollByRequestedEventArgs args)
    {
        Debug.Assert(sender == m_horizontalScrollController || sender == m_verticalScrollController);

        bool isFromHorizontalScrollController = sender == m_horizontalScrollController;
        int viewChangeCorrelationId = s_noOpCorrelationId;

        // Attempt to find an offset change request from an IScrollController with the same ScrollPresenterViewKind,
        // the same ScrollingScrollOptions settings and same tick.
        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = GetInteractionTrackerOperationFromKinds(
            true /*isOperationTypeForOffsetsChange*/,
            (InteractionTrackerAsyncOperationTrigger)((int)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest) + (int)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest)),
            ScrollPresenterViewKind.RelativeToCurrentView,
            args.Options);

        if (interactionTrackerAsyncOperation is null)
        {
            ChangeOffsetsPrivate(
                isFromHorizontalScrollController ? args.OffsetDelta : 0.0 /*zoomedHorizontalOffset*/,
                isFromHorizontalScrollController ? 0.0 : args.OffsetDelta /*zoomedVerticalOffset*/,
                ScrollPresenterViewKind.RelativeToCurrentView,
                args.Options,
                null /*bringIntoViewRequestedEventArgs*/,
                isFromHorizontalScrollController ? InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest : InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest,
                s_noOpCorrelationId /*existingViewChangeCorrelationId*/,
                out viewChangeCorrelationId);
        }
        else
        {
            // Coalesce requests
            int existingViewChangeCorrelationId = interactionTrackerAsyncOperation.GetViewChangeCorrelationId();
            ViewChangeBase viewChangeBase = interactionTrackerAsyncOperation.GetViewChangeBase();
            OffsetsChange offsetsChange = viewChangeBase as OffsetsChange;

            interactionTrackerAsyncOperation.SetIsScrollControllerRequest(isFromHorizontalScrollController);

            if (isFromHorizontalScrollController)
            {
                offsetsChange.ZoomedHorizontalOffset = (offsetsChange.ZoomedHorizontalOffset + args.OffsetDelta);
            }
            else
            {
                offsetsChange.ZoomedVerticalOffset = (offsetsChange.ZoomedVerticalOffset + args.OffsetDelta);
            }

            viewChangeCorrelationId = existingViewChangeCorrelationId;
        }

        if (viewChangeCorrelationId != s_noOpCorrelationId)
        {
            args.CorrelationId = (viewChangeCorrelationId);
        }
    }

    private void OnScrollControllerScrollToRequested(
        IScrollController sender,
        ScrollControllerScrollToRequestedEventArgs args)
    {
        Debug.Assert(sender == m_horizontalScrollController || sender == m_verticalScrollController);

        bool isFromHorizontalScrollController = sender == m_horizontalScrollController;
        int viewChangeCorrelationId = s_noOpCorrelationId;

        // Attempt to find an offset change request from an IScrollController with the same ScrollPresenterViewKind,
        // the same ScrollingScrollOptions settings and same tick.
        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation = GetInteractionTrackerOperationFromKinds(
            true /*isOperationTypeForOffsetsChange*/,
            (InteractionTrackerAsyncOperationTrigger)((int)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest) + (int)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest)),
            ScrollPresenterViewKind.Absolute,
            args.Options);

        if (interactionTrackerAsyncOperation is null)
        {
            ChangeOffsetsPrivate(
                isFromHorizontalScrollController ? args.Offset : m_zoomedHorizontalOffset,
                isFromHorizontalScrollController ? m_zoomedVerticalOffset : args.Offset,
                ScrollPresenterViewKind.Absolute,
                args.Options,
                null /*bringIntoViewRequestedEventArgs*/,
                isFromHorizontalScrollController ? InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest : InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest,
                s_noOpCorrelationId /*existingViewChangeCorrelationId*/,
                out viewChangeCorrelationId);
        }
        else
        {
            // Coalesce requests
            int existingViewChangeCorrelationId = interactionTrackerAsyncOperation.GetViewChangeCorrelationId();
            ViewChangeBase viewChangeBase = interactionTrackerAsyncOperation.GetViewChangeBase();
            OffsetsChange offsetsChange = (viewChangeBase) as OffsetsChange;

            interactionTrackerAsyncOperation.SetIsScrollControllerRequest(isFromHorizontalScrollController);

            if (isFromHorizontalScrollController)
            {
                offsetsChange.ZoomedHorizontalOffset = (args.Offset);
            }
            else
            {
                offsetsChange.ZoomedVerticalOffset = (args.Offset);
            }

            viewChangeCorrelationId = existingViewChangeCorrelationId;
        }

        if (viewChangeCorrelationId != s_noOpCorrelationId)
        {
            args.CorrelationId = (viewChangeCorrelationId);
        }
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

    private void OnViewChanged(bool horizontalOffsetChanged, bool verticalOffsetChanged)
    {
        if (horizontalOffsetChanged)
        {
            UpdateScrollControllerValues(ScrollPresenterDimension.HorizontalScroll);
        }

        if (verticalOffsetChanged)
        {
            UpdateScrollControllerValues(ScrollPresenterDimension.VerticalScroll);
        }

        UpdateScrollAutomationPatternProperties();

        RaiseViewChanged();
    }

    private InteractionTrackerAsyncOperation GetInteractionTrackerOperationFromRequestId(
        int requestId)
    {
        Debug.Assert(requestId >= 0);

        foreach (var interactionTrackerAsyncOperation in m_interactionTrackerAsyncOperations)
        {
            if (interactionTrackerAsyncOperation.GetRequestId() == requestId)
            {
                return interactionTrackerAsyncOperation;
            }
        }

        return null;
    }

    private void PostProcessOffsetsChange(InteractionTrackerAsyncOperation interactionTrackerAsyncOperation)
    {
        Debug.Assert(m_interactionTracker is not null);

        if (interactionTrackerAsyncOperation.GetRequestId() != m_latestInteractionTrackerRequest)
        {
            InteractionTrackerAsyncOperation latestInteractionTrackerAsyncOperation = GetInteractionTrackerOperationFromRequestId(
                m_latestInteractionTrackerRequest);
            if (latestInteractionTrackerAsyncOperation is not null &&
                latestInteractionTrackerAsyncOperation.GetOperationType() == InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity)
            {
                // Do not reset the scroll inertia decay rate when there is a new ongoing offset change with additional velocity
                return;
            }
        }

        ResetOffsetsInertiaDecayRate();
    }

    private void PostProcessZoomFactorChange(InteractionTrackerAsyncOperation interactionTrackerAsyncOperation)
    {
        Debug.Assert(m_interactionTracker is not null);

        if (interactionTrackerAsyncOperation.GetRequestId() != m_latestInteractionTrackerRequest)
        {
            InteractionTrackerAsyncOperation latestInteractionTrackerAsyncOperation = GetInteractionTrackerOperationFromRequestId(
                m_latestInteractionTrackerRequest);
            if (latestInteractionTrackerAsyncOperation is not null &&
                latestInteractionTrackerAsyncOperation.GetOperationType() == InteractionTrackerAsyncOperationType.TryUpdateScaleWithAdditionalVelocity)
            {
                // Do not reset the zoomFactor inertia decay rate when there is a new ongoing zoomFactor change with additional velocity
                return;
            }
        }

        ResetZoomFactorInertiaDecayRate();
    }

    private void ProcessAnchorCandidate(
        UIElement anchorCandidate,
        UIElement content,
        Rect viewportAnchorBounds,
        double viewportAnchorPointHorizontalOffset,
        double viewportAnchorPointVerticalOffset,
        ref double bestAnchorCandidateDistance,
        ref UIElement bestAnchorCandidate,
        ref Rect bestAnchorCandidateBounds)
    {
        throw new NotImplementedException();
    }

    private void ProcessDequeuedViewChange(
        InteractionTrackerAsyncOperation interactionTrackerAsyncOperation)
    {
        Debug.Assert(IsLoadedAndSetUp());
        Debug.Assert(!interactionTrackerAsyncOperation.IsQueued());

        ViewChangeBase viewChangeBase = interactionTrackerAsyncOperation.GetViewChangeBase();

        Debug.Assert(viewChangeBase is not null);

        switch (interactionTrackerAsyncOperation.GetOperationType())
        {
            case InteractionTrackerAsyncOperationType.TryUpdatePosition:
            case InteractionTrackerAsyncOperationType.TryUpdatePositionBy:
            case InteractionTrackerAsyncOperationType.TryUpdatePositionWithAnimation:
                {
                    OffsetsChange offsetsChange = viewChangeBase as OffsetsChange;

                    ProcessOffsetsChange(
                        interactionTrackerAsyncOperation.GetOperationTrigger() /*operationTrigger*/,
                        offsetsChange,
                        interactionTrackerAsyncOperation.GetViewChangeCorrelationId() /*offsetsChangeId*/,
                        true /*isForAsyncOperation*/);
                    break;
                }
            case InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity:
                {
                    OffsetsChangeWithAdditionalVelocity offsetsChangeWithAdditionalVelocity = viewChangeBase as OffsetsChangeWithAdditionalVelocity;

                    ProcessOffsetsChange(
                        interactionTrackerAsyncOperation.GetOperationTrigger() /*operationTrigger*/,
                        offsetsChangeWithAdditionalVelocity);
                    break;
                }
            case InteractionTrackerAsyncOperationType.TryUpdateScale:
            case InteractionTrackerAsyncOperationType.TryUpdateScaleWithAnimation:
                {
                    ZoomFactorChange zoomFactorChange = viewChangeBase as ZoomFactorChange;

                    ProcessZoomFactorChange(
                        zoomFactorChange,
                        interactionTrackerAsyncOperation.GetViewChangeCorrelationId() /*zoomFactorChangeId*/);
                    break;
                }
            case InteractionTrackerAsyncOperationType.TryUpdateScaleWithAdditionalVelocity:
                {
                    ZoomFactorChangeWithAdditionalVelocity zoomFactorChangeWithAdditionalVelocity = viewChangeBase as ZoomFactorChangeWithAdditionalVelocity;

                    ProcessZoomFactorChange(
                        interactionTrackerAsyncOperation.GetOperationTrigger() /*operationTrigger*/,
                        zoomFactorChangeWithAdditionalVelocity);
                    break;
                }
            default:
                {
                    Debug.Assert(false);
                    break;
                }
        }
        interactionTrackerAsyncOperation.SetRequestId(m_latestInteractionTrackerRequest);
    }

    private void ProcessOffsetsChange(
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        OffsetsChange offsetsChange,
        int offsetsChangeCorrelationId,
        bool isForAsyncOperation)
    {
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(offsetsChange is not null);

        double zoomedHorizontalOffset = offsetsChange.ZoomedHorizontalOffset;
        double zoomedVerticalOffset = offsetsChange.ZoomedVerticalOffset;
        ScrollingScrollOptions options = offsetsChange.Options() as ScrollingScrollOptions;

        ScrollingAnimationMode animationMode = options is not null ? options.AnimationMode : ScrollingAnimationMode.Auto;
        ScrollingSnapPointsMode snapPointsMode = options is not null ? options.SnapPointsMode : ScrollingSnapPointsMode.Default;

        animationMode = GetComputedAnimationMode(animationMode);

        if (((char)(operationTrigger) & (char)(InteractionTrackerAsyncOperationTrigger.BringIntoViewRequest)) > 0)
        {
            if (Content is UIElement content)
            {
                BringIntoViewOffsetsChange bringIntoViewOffsetsChange = (offsetsChange) as BringIntoViewOffsetsChange;

                if (bringIntoViewOffsetsChange is not null)
                {
                    // The target Element may have moved within the Content since the bring-into-view operation was
                    // initiated one or more ticks ago in ScrollPresenter::OnBringIntoViewRequestedHandler.
                    // The target offsets are therefore re-evaluated according to the latest Element position and size.
                    ComputeBringIntoViewUpdatedTargetOffsets(
                        content,
                        bringIntoViewOffsetsChange.Element(),
                        bringIntoViewOffsetsChange.ElementRect(),
                        snapPointsMode,
                        bringIntoViewOffsetsChange.HorizontalAlignmentRatio(),
                        bringIntoViewOffsetsChange.VerticalAlignmentRatio(),
                        bringIntoViewOffsetsChange.HorizontalOffset(),
                        bringIntoViewOffsetsChange.VerticalOffset(),
                        out zoomedHorizontalOffset,
                        out zoomedVerticalOffset);
                }
            }
        }

        switch (offsetsChange.ViewKind())
        {
            case ScrollPresenterViewKind.RelativeToCurrentView:
                {
                    if (snapPointsMode == ScrollingSnapPointsMode.Default || animationMode == ScrollingAnimationMode.Enabled)
                    {
                        zoomedHorizontalOffset += m_zoomedHorizontalOffset;
                        zoomedVerticalOffset += m_zoomedVerticalOffset;
                    }
                    break;
                }
        }

        if (snapPointsMode == ScrollingSnapPointsMode.Default)
        {
            zoomedHorizontalOffset = ComputeValueAfterSnapPoints<ScrollSnapPointBase>(zoomedHorizontalOffset, m_sortedConsolidatedHorizontalSnapPoints);
            zoomedVerticalOffset = ComputeValueAfterSnapPoints<ScrollSnapPointBase>(zoomedVerticalOffset, m_sortedConsolidatedVerticalSnapPoints);
        }

        switch (animationMode)
        {
            case ScrollingAnimationMode.Disabled:
                {
                    if (offsetsChange.ViewKind() == ScrollPresenterViewKind.RelativeToCurrentView && snapPointsMode == ScrollingSnapPointsMode.Ignore)
                    {
                        m_latestInteractionTrackerRequest = m_interactionTracker.TryUpdatePositionBy(
                            new Vector3((float)(zoomedHorizontalOffset), (float)(zoomedVerticalOffset), 0.0f));
                        m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.TryUpdatePositionBy;
                    }
                    else
                    {
                        Vector2 targetPosition = ComputePositionFromOffsets(zoomedHorizontalOffset, zoomedVerticalOffset);

                        m_latestInteractionTrackerRequest = m_interactionTracker.TryUpdatePosition(
                            new Vector3(targetPosition, 0.0f));
                        m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.TryUpdatePosition;
                    }

                    if (isForAsyncOperation)
                    {
                        HookCompositionTargetRendering();
                    }
                    break;
                }
            case ScrollingAnimationMode.Enabled:
                {
                    m_latestInteractionTrackerRequest = m_interactionTracker.TryUpdatePositionWithAnimation(
                        GetPositionAnimation(
                            zoomedHorizontalOffset,
                            zoomedVerticalOffset,
                            operationTrigger,
                            offsetsChangeCorrelationId));
                    m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.TryUpdatePositionWithAnimation;
                    break;
                }
        }
    }

    private void ProcessOffsetsChange(
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        OffsetsChangeWithAdditionalVelocity offsetsChangeWithAdditionalVelocity)
    {
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(offsetsChangeWithAdditionalVelocity is not null);

        Vector2 offsetsVelocity = offsetsChangeWithAdditionalVelocity.OffsetsVelocity();
        Vector2? inertiaDecayRate = offsetsChangeWithAdditionalVelocity.InertiaDecayRate();

        if (operationTrigger == InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest ||
            operationTrigger == InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest)
        {
            // Requests coming from an IScrollController implementation do not include the 'minimum inertia velocity' value of 30.0f, because that
            // concept is InteractionTracker-specific (the IScrollController interface is meant to be InteractionTracker-agnostic).
            if (m_state != ScrollingInteractionState.Inertia)
            {
                // When there is no current inertia, include that minimum velocity automatically. So the IScrollController-provided velocity is always
                // proportional to the resulting offset change.
                const float s_minimumVelocity = 30.0f;

                if (offsetsVelocity.X < 0.0f)
                {
                    offsetsVelocity = new Vector2(offsetsVelocity.X - s_minimumVelocity, offsetsVelocity.Y);
                }
                else if (offsetsVelocity.X > 0.0f)
                {
                    offsetsVelocity = new Vector2(offsetsVelocity.X + s_minimumVelocity, offsetsVelocity.Y);
                }

                if (offsetsVelocity.Y < 0.0f)
                {
                    offsetsVelocity = new Vector2(offsetsVelocity.X, offsetsVelocity.Y - s_minimumVelocity);
                }
                else if (offsetsVelocity.Y > 0.0f)
                {
                    offsetsVelocity = new Vector2(offsetsVelocity.X, offsetsVelocity.Y + s_minimumVelocity);
                }
            }
        }

        if (inertiaDecayRate.HasValue)
        {
            float horizontalInertiaDecayRate = Math.Clamp(inertiaDecayRate.Value.X, 0.0f, 1.0f);
            float verticalInertiaDecayRate = Math.Clamp(inertiaDecayRate.Value.Y, 0.0f, 1.0f);

            m_interactionTracker.PositionInertiaDecayRate = (
                new Vector3(horizontalInertiaDecayRate, verticalInertiaDecayRate, 0.0f));
        }
        else
        {
            // Restore the default 0.95 position inertia decay rate since it may have been overridden by a prior offset change with additional velocity.
            ResetOffsetsInertiaDecayRate();
        }

        m_latestInteractionTrackerRequest = m_interactionTracker.TryUpdatePositionWithAdditionalVelocity(
            new Vector3(offsetsVelocity, 0.0f));
        m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.TryUpdatePositionWithAdditionalVelocity;
    }

    private void ProcessZoomFactorChange(
        ZoomFactorChange zoomFactorChange,
        int zoomFactorChangeCorrelationId)
    {
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(zoomFactorChange is not null);

        float zoomFactor = zoomFactorChange.ZoomFactor;
        Vector2? nullableCenterPoint = zoomFactorChange.CenterPoint;
        ScrollPresenterViewKind viewKind = zoomFactorChange.ViewKind();
        ScrollingZoomOptions options = zoomFactorChange.Options() as ScrollingZoomOptions;

        Vector2 centerPoint2D = nullableCenterPoint == null ?
           new Vector2((float)(m_viewportWidth / 2.0), (float)(m_viewportHeight / 2.0)) : nullableCenterPoint.Value;
        Vector3 centerPoint = new Vector3(centerPoint2D.X - m_contentLayoutOffsetX, centerPoint2D.Y - m_contentLayoutOffsetY, 0.0f);

        switch (viewKind)
        {
            case ScrollPresenterViewKind.RelativeToCurrentView:
                {
                    zoomFactor += m_zoomFactor;
                    break;
                }
        }

        ScrollingAnimationMode animationMode = options is not null ? options.AnimationMode : ScrollingAnimationMode.Auto;
        ScrollingSnapPointsMode snapPointsMode = options is not null ? options.SnapPointsMode : ScrollingSnapPointsMode.Default;

        animationMode = GetComputedAnimationMode(animationMode);

        if (snapPointsMode == ScrollingSnapPointsMode.Default)
        {
            zoomFactor = (float)(ComputeValueAfterSnapPoints<ZoomSnapPointBase>(zoomFactor, m_sortedConsolidatedZoomSnapPoints));
        }

        switch (animationMode)
        {
            case ScrollingAnimationMode.Disabled:
                {
                    m_latestInteractionTrackerRequest = m_interactionTracker.TryUpdateScale(zoomFactor, centerPoint);
                    m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.TryUpdateScale;

                    HookCompositionTargetRendering();
                    break;
                }
            case ScrollingAnimationMode.Enabled:
                {
                    m_latestInteractionTrackerRequest = m_interactionTracker.TryUpdateScaleWithAnimation(
                        GetZoomFactorAnimation(zoomFactor, centerPoint2D, zoomFactorChangeCorrelationId),
                        centerPoint);
                    m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.TryUpdateScaleWithAnimation;
                    break;
                }
        }
    }

    private void ProcessZoomFactorChange(
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        ZoomFactorChangeWithAdditionalVelocity zoomFactorChangeWithAdditionalVelocity)
    {
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(zoomFactorChangeWithAdditionalVelocity is not null);

        float zoomFactorVelocity = zoomFactorChangeWithAdditionalVelocity.ZoomFactorVelocity();
        float? inertiaDecayRate = zoomFactorChangeWithAdditionalVelocity.InertiaDecayRate();
        Vector2? nullableCenterPoint = zoomFactorChangeWithAdditionalVelocity.CenterPoint();

        if (inertiaDecayRate.HasValue)
        {
            float scaleInertiaDecayRate = Math.Clamp(inertiaDecayRate.Value, 0.0f, 1.0f);

            m_interactionTracker.ScaleInertiaDecayRate = (scaleInertiaDecayRate);
        }
        else
        {
            // Restore the default 0.985 zoomFactor inertia decay rate since it may have been overridden by a prior zoomFactor change with additional velocity.
            ResetZoomFactorInertiaDecayRate();
        }

        Vector2 centerPoint2D = nullableCenterPoint == null ?
            new Vector2((float)(m_viewportWidth / 2.0), (float)(m_viewportHeight / 2.0)) : nullableCenterPoint.Value;
        Vector3 centerPoint = new Vector3(centerPoint2D.X - m_contentLayoutOffsetX, centerPoint2D.Y - m_contentLayoutOffsetY, 0.0f);

        m_latestInteractionTrackerRequest = m_interactionTracker.TryUpdateScaleWithAdditionalVelocity(
            zoomFactorVelocity,
            centerPoint);
        m_lastInteractionTrackerAsyncOperationType = InteractionTrackerAsyncOperationType.TryUpdateScaleWithAdditionalVelocity;
    }

    private void RaiseAnchorRequested()
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
        if (BringingIntoView is not null)
        {
            var bringingIntoViewEventArgs = new ScrollingBringingIntoViewEventArgs();

            bringingIntoViewEventArgs.SnapPointsMode = (snapPointsMode);
            bringingIntoViewEventArgs.OffsetsChangeCorrelationId(offsetsChangeCorrelationId);
            bringingIntoViewEventArgs.RequestEventArgs = (requestEventArgs);
            bringingIntoViewEventArgs.TargetOffsets(targetZoomedHorizontalOffset, targetZoomedVerticalOffset);

            BringingIntoView(this, bringingIntoViewEventArgs);
            snapPointsMode = bringingIntoViewEventArgs.SnapPointsMode;
            return !bringingIntoViewEventArgs.Cancel;
        }
        return true;
    }

    private void RaiseExpressionAnimationStatusChanged(
        bool isExpressionAnimationStarted,
        string propertyName)
    {
        ScrollPresenterTestHooks globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();

        if (globalTestHooks is not null && ScrollPresenterTestHooks.AreExpressionAnimationStatusNotificationsRaised())
        {
            ScrollPresenterTestHooks.NotifyExpressionAnimationStatusChanged(this, isExpressionAnimationStarted, propertyName);
        }
    }

    private void RaiseExtentChanged()
    {
        ExtentChanged?.Invoke(this, null);
    }

    private void RaiseInteractionSourcesChanged()
    {
        ScrollPresenterTestHooks globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();

        if (globalTestHooks is not null && ScrollPresenterTestHooks.AreInteractionSourcesNotificationsRaised())
        {
            ScrollPresenterTestHooks.NotifyInteractionSourcesChanged(this, m_interactionTracker.InteractionSources);
        }
    }

    private void RaisePostArrange()
    {
        if (m_postArrange is not null)
        {
            m_postArrange(this);
        }
    }

    private CompositionAnimation RaiseScrollAnimationStarting(
        Vector3KeyFrameAnimation positionAnimation,
        Vector2 startPosition,
        Vector2 endPosition,
        int offsetsChangeCorrelationId)
    {
        if (ScrollAnimationStarting is not null)
        {
            var scrollAnimationStartingEventArgs = new ScrollingScrollAnimationStartingEventArgs();

            if (offsetsChangeCorrelationId != s_noOpCorrelationId)
            {
                scrollAnimationStartingEventArgs.SetOffsetsChangeCorrelationId(offsetsChangeCorrelationId);
            }

            scrollAnimationStartingEventArgs.Animation = (positionAnimation);
            scrollAnimationStartingEventArgs.SetStartPosition(startPosition);
            scrollAnimationStartingEventArgs.SetEndPosition(endPosition);
            ScrollAnimationStarting(this, scrollAnimationStartingEventArgs);
            return scrollAnimationStartingEventArgs.Animation;
        }
        else
        {
            return positionAnimation;
        }
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this, null);
    }

    private void RaiseViewChangeCompleted(
        bool isForScroll,
        ScrollPresenterViewChangeResult result,
        int viewChangeCorrelationId)
    {
        if (viewChangeCorrelationId != 0)
        {
            if (isForScroll && ScrollCompleted is not null)
            {
                var scrollCompletedEventArgs = new ScrollingScrollCompletedEventArgs();

                scrollCompletedEventArgs.Result(result);
                scrollCompletedEventArgs.OffsetsChangeCorrelationId(viewChangeCorrelationId);
                ScrollCompleted(this, scrollCompletedEventArgs);
            }
            else if (!isForScroll && ZoomCompleted is not null)
            {
                var zoomCompletedEventArgs = new ScrollingZoomCompletedEventArgs();

                zoomCompletedEventArgs.Result(result);
                zoomCompletedEventArgs.ZoomFactorChangeCorrelationId(viewChangeCorrelationId);
                ZoomCompleted(this, zoomCompletedEventArgs);
            }
        }

        InvalidateViewport();
    }

    private void RaiseViewChanged()
    {
        ViewChanged?.Invoke(this, null);

        InvalidateViewport();
    }

    private CompositionAnimation RaiseZoomAnimationStarting(
        ScalarKeyFrameAnimation zoomFactorAnimation,
        float endZoomFactor,
        Vector2 centerPoint,
        int zoomFactorChangeCorrelationId)
    {
        if (ZoomAnimationStarting is not null)
        {
            var zoomAnimationStartingEventArgs = new ScrollingZoomAnimationStartingEventArgs();

            if (zoomFactorChangeCorrelationId != s_noOpCorrelationId)
            {
                zoomAnimationStartingEventArgs.SetZoomFactorChangeCorrelationId(zoomFactorChangeCorrelationId);
            }

            zoomAnimationStartingEventArgs.Animation = (zoomFactorAnimation);
            zoomAnimationStartingEventArgs.SetCenterPoint(centerPoint);
            zoomAnimationStartingEventArgs.SetStartZoomFactor(m_zoomFactor);
            zoomAnimationStartingEventArgs.SetEndZoomFactor(endZoomFactor);
            ZoomAnimationStarting(this, zoomAnimationStartingEventArgs);
            return zoomAnimationStartingEventArgs.Animation;
        }
        else
        {
            return zoomFactorAnimation;
        }
    }

    private void RegenerateSnapPointsSet<T>(IObservableVector<T> userVector, HashSet<SnapPointWrapper<T>> internalSet)
    {
        Debug.Assert(internalSet is not null);

        internalSet.Clear();
        foreach (T snapPoint in userVector)
        {
            SnapPointWrapper<T> snapPointWrapper =
                new SnapPointWrapper<T>(snapPoint);

            SnapPointsVectorItemInsertedHelper(snapPointWrapper, internalSet);
        }
    }

    private void ResetAnchorElement()
    {
        if (m_anchorElement is not null)
        {
            m_anchorElement = (null);
            m_anchorElementBounds = new Rect();
            m_isAnchorElementDirty = false;

            ScrollPresenterTestHooks globalTestHooks = ScrollPresenterTestHooks.GetGlobalTestHooks();

            if (globalTestHooks is not null && ScrollPresenterTestHooks.AreAnchorNotificationsRaised())
            {
                ScrollPresenterTestHooks.NotifyAnchorEvaluated(this, null /*anchorElement*/, double.NaN /*viewportAnchorPointHorizontalOffset*/, double.NaN /*viewportAnchorPointVerticalOffset*/);
            }
        }
    }

    private void ResetOffsetsInertiaDecayRate()
    {
        Debug.Assert(m_interactionTracker is not null);

        m_interactionTracker.PositionInertiaDecayRate = (null);
    }

    private void ResetZoomFactorInertiaDecayRate()
    {
        Debug.Assert(m_interactionTracker is not null);

        m_interactionTracker.ScaleInertiaDecayRate = (null);
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

        UIElement? content = Content;

        if (content is not null && (m_minPositionExpressionAnimation is null || m_maxPositionExpressionAnimation is null))
        {
            EnsurePositionBoundariesExpressionAnimations();
            SetupPositionBoundariesExpressionAnimations(content);
        }
    }

    private void SetupInteractionTrackerZoomFactorBoundaries(double minZoomFactor, double maxZoomFactor)
    {
        Debug.Assert(m_interactionTracker is not null);

        float oldMaxZoomFactor = m_interactionTracker.MaxScale;

        minZoomFactor = Math.Max(0.0, minZoomFactor);
        maxZoomFactor = Math.Max(minZoomFactor, maxZoomFactor);

        float newMinZoomFactor = (float)(minZoomFactor);
        float newMaxZoomFactor = (float)(maxZoomFactor);

        if (newMinZoomFactor > oldMaxZoomFactor)
        {
            m_interactionTracker.MaxScale = (newMaxZoomFactor);
            m_interactionTracker.MinScale = (newMinZoomFactor);
        }
        else
        {
            m_interactionTracker.MinScale = (newMinZoomFactor);
            m_interactionTracker.MaxScale = (newMaxZoomFactor);
        }
    }

    private void SetupPositionBoundariesExpressionAnimations(UIElement content)
    {
        Debug.Assert(content is not null);
        Debug.Assert(m_minPositionExpressionAnimation is not null);
        Debug.Assert(m_maxPositionExpressionAnimation is not null);
        Debug.Assert(m_interactionTracker is not null);

        Visual scrollPresenterVisual = ElementCompositionPreview.GetElementVisual(this);

        string s = m_minPositionExpressionAnimation.Expression;

        if (s.Length == 0)
        {
            m_minPositionExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);
            m_minPositionExpressionAnimation.SetReferenceParameter("scrollPresenterVisual", scrollPresenterVisual);
        }

        m_minPositionExpressionAnimation.Expression = GetMinPositionExpression(content);

        s = m_maxPositionExpressionAnimation.Expression;

        if (s.Length == 0)
        {
            m_maxPositionExpressionAnimation.SetReferenceParameter("it", m_interactionTracker);
            m_maxPositionExpressionAnimation.SetReferenceParameter("scrollPresenterVisual", scrollPresenterVisual);
        }

        m_maxPositionExpressionAnimation.Expression = GetMaxPositionExpression(content);

        UpdatePositionBoundaries(content);
    }

    private void SetupScrollControllerVisualInterationSource(ScrollPresenterDimension dimension)
    {
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);

        VisualInteractionSource scrollControllerVisualInteractionSource = null;
        Visual panningElementAncestorVisual = null;

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            scrollControllerVisualInteractionSource = m_horizontalScrollControllerVisualInteractionSource;
            if (m_horizontalScrollControllerPanningInfo is not null)
            {
                UIElement panningElementAncestor = m_horizontalScrollControllerPanningInfo.PanningElementAncestor;

                if (panningElementAncestor is not null)
                {
                    panningElementAncestorVisual = ElementCompositionPreview.GetElementVisual(panningElementAncestor);
                }
            }
        }
        else
        {
            scrollControllerVisualInteractionSource = m_verticalScrollControllerVisualInteractionSource;
            if (m_verticalScrollControllerPanningInfo is not null)
            {
                UIElement panningElementAncestor = m_verticalScrollControllerPanningInfo.PanningElementAncestor;

                if (panningElementAncestor is not null)
                {
                    panningElementAncestorVisual = ElementCompositionPreview.GetElementVisual(panningElementAncestor);
                }
            }
        }

        if (panningElementAncestorVisual is null && scrollControllerVisualInteractionSource is not null)
        {
            // The IScrollController no longer uses a Visual.
            VisualInteractionSource otherScrollControllerVisualInteractionSource =
                dimension == ScrollPresenterDimension.HorizontalScroll ? m_verticalScrollControllerVisualInteractionSource : m_horizontalScrollControllerVisualInteractionSource;

            if (otherScrollControllerVisualInteractionSource != scrollControllerVisualInteractionSource)
            {
                // The horizontal and vertical IScrollController implementations are not using the same Visual,
                // so the old VisualInteractionSource can be discarded.
                m_interactionTracker.InteractionSources.Remove(scrollControllerVisualInteractionSource);
                StopScrollControllerExpressionAnimationSourcesAnimations(dimension);
                if (dimension == ScrollPresenterDimension.HorizontalScroll)
                {
                    m_horizontalScrollControllerVisualInteractionSource = null;
                    m_horizontalScrollControllerExpressionAnimationSources = null;
                    m_horizontalScrollControllerOffsetExpressionAnimation = null;
                    m_horizontalScrollControllerMaxOffsetExpressionAnimation = null;
                }
                else
                {
                    m_verticalScrollControllerVisualInteractionSource = null;
                    m_verticalScrollControllerExpressionAnimationSources = null;
                    m_verticalScrollControllerOffsetExpressionAnimation = null;
                    m_verticalScrollControllerMaxOffsetExpressionAnimation = null;
                }

                RaiseInteractionSourcesChanged();
            }
            else
            {
                // The horizontal and vertical IScrollController implementations were using the same Visual,
                // so the old VisualInteractionSource cannot be discarded.
                if (dimension == ScrollPresenterDimension.HorizontalScroll)
                {
                    scrollControllerVisualInteractionSource.PositionXSourceMode = (InteractionSourceMode.Disabled);
                    scrollControllerVisualInteractionSource.IsPositionXRailsEnabled = (false);
                }
                else
                {
                    scrollControllerVisualInteractionSource.PositionYSourceMode = (InteractionSourceMode.Disabled);
                    scrollControllerVisualInteractionSource.IsPositionYRailsEnabled = (false);
                }
            }
            return;
        }
        else if (panningElementAncestorVisual is not null)
        {
            if (scrollControllerVisualInteractionSource is null)
            {
                // The IScrollController now uses a Visual.
                VisualInteractionSource otherScrollControllerVisualInteractionSource =
                    dimension == ScrollPresenterDimension.HorizontalScroll ? m_verticalScrollControllerVisualInteractionSource : m_horizontalScrollControllerVisualInteractionSource;

                if (otherScrollControllerVisualInteractionSource is null || otherScrollControllerVisualInteractionSource.Source != panningElementAncestorVisual)
                {
                    // That Visual is not shared with the other dimension, so create a new VisualInteractionSource for it.
                    EnsureScrollControllerVisualInteractionSource(panningElementAncestorVisual, dimension);
                }
                else
                {
                    // That Visual is shared with the other dimension, so share the existing VisualInteractionSource as well.
                    if (dimension == ScrollPresenterDimension.HorizontalScroll)
                    {
                        m_horizontalScrollControllerVisualInteractionSource = otherScrollControllerVisualInteractionSource;
                    }
                    else
                    {
                        m_verticalScrollControllerVisualInteractionSource = otherScrollControllerVisualInteractionSource;
                    }
                }
                EnsureScrollControllerExpressionAnimationSources(dimension);
                StartScrollControllerExpressionAnimationSourcesAnimations(dimension);
            }

            Orientation orientation;
            bool isRailEnabled;

            // Setup the VisualInteractionSource instance.
            if (dimension == ScrollPresenterDimension.HorizontalScroll)
            {
                orientation = m_horizontalScrollControllerPanningInfo.PanOrientation;
                isRailEnabled = m_horizontalScrollControllerPanningInfo.IsRailEnabled;

                if (orientation == Orientation.Horizontal)
                {
                    m_horizontalScrollControllerVisualInteractionSource.PositionXSourceMode = (InteractionSourceMode.EnabledWithoutInertia);
                    m_horizontalScrollControllerVisualInteractionSource.IsPositionXRailsEnabled = (isRailEnabled);
                }
                else
                {
                    m_horizontalScrollControllerVisualInteractionSource.PositionYSourceMode = (InteractionSourceMode.EnabledWithoutInertia);
                    m_horizontalScrollControllerVisualInteractionSource.IsPositionYRailsEnabled = (isRailEnabled);
                }
            }
            else
            {
                orientation = m_verticalScrollControllerPanningInfo.PanOrientation;
                isRailEnabled = m_verticalScrollControllerPanningInfo.IsRailEnabled;

                if (orientation == Orientation.Horizontal)
                {
                    m_verticalScrollControllerVisualInteractionSource.PositionXSourceMode = (InteractionSourceMode.EnabledWithoutInertia);
                    m_verticalScrollControllerVisualInteractionSource.IsPositionXRailsEnabled = (isRailEnabled);
                }
                else
                {
                    m_verticalScrollControllerVisualInteractionSource.PositionYSourceMode = (InteractionSourceMode.EnabledWithoutInertia);
                    m_verticalScrollControllerVisualInteractionSource.IsPositionYRailsEnabled = (isRailEnabled);
                }
            }

            if (scrollControllerVisualInteractionSource is null)
            {
                SetupScrollControllerVisualInterationSourcePositionModifiers(
                    dimension,
                    orientation);
            }
        }
    }

    private void SetupScrollControllerVisualInterationSourcePositionModifiers(
        ScrollPresenterDimension dimension,
        Orientation orientation)
    {
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);
        Debug.Assert(m_interactionTracker is not null);

        VisualInteractionSource scrollControllerVisualInteractionSource = dimension == ScrollPresenterDimension.HorizontalScroll ?
                m_horizontalScrollControllerVisualInteractionSource : m_verticalScrollControllerVisualInteractionSource;
        CompositionPropertySet scrollControllerExpressionAnimationSources = dimension == ScrollPresenterDimension.HorizontalScroll ?
                  m_horizontalScrollControllerExpressionAnimationSources : m_verticalScrollControllerExpressionAnimationSources;

        Debug.Assert(scrollControllerVisualInteractionSource is not null);
        Debug.Assert(scrollControllerExpressionAnimationSources is not null);

        Compositor compositor = scrollControllerVisualInteractionSource.Compositor;
        CompositionConditionalValue[] ccvs = new CompositionConditionalValue[] { CompositionConditionalValue.Create(compositor), CompositionConditionalValue.Create(compositor), CompositionConditionalValue.Create(compositor), CompositionConditionalValue.Create(compositor) };
        ExpressionAnimation[] conditions = new ExpressionAnimation[] { compositor.CreateExpressionAnimation(), compositor.CreateExpressionAnimation(), compositor.CreateExpressionAnimation(), compositor.CreateExpressionAnimation() };
        ExpressionAnimation[] values = new ExpressionAnimation[] { compositor.CreateExpressionAnimation(), compositor.CreateExpressionAnimation(), compositor.CreateExpressionAnimation(), compositor.CreateExpressionAnimation() };
        for (int index = 0; index < 4; index++)
        {
            ccvs[index].Condition = (conditions[index]);
            ccvs[index].Value = (values[index]);

            values[index].SetReferenceParameter("sceas", scrollControllerExpressionAnimationSources);
            values[index].SetReferenceParameter("scvis", scrollControllerVisualInteractionSource);
            values[index].SetReferenceParameter("it", m_interactionTracker);
        }

        for (int index = 0; index < 3; index++)
        {
            conditions[index].SetReferenceParameter("scvis", scrollControllerVisualInteractionSource);
            conditions[index].SetReferenceParameter("sceas", scrollControllerExpressionAnimationSources);
        }
        conditions[3].Expression = ("true");

        var modifiersVector = new List<CompositionConditionalValue>();

        for (int index = 0; index < 4; index++)
        {
            modifiersVector.Add(ccvs[index]);
        }

        if (orientation == Orientation.Horizontal)
        {
            conditions[0].Expression = ("scvis.DeltaPosition.X < 0.0f && sceas.Multiplier < 0.0f");
            conditions[1].Expression = ("scvis.DeltaPosition.X < 0.0f && sceas.Multiplier >= 0.0f");
            conditions[2].Expression = ("scvis.DeltaPosition.X >= 0.0f && sceas.Multiplier < 0.0f");
            // Case #4 <==> scvis.DeltaPosition.X >= 0.0f && sceas.Multiplier > 0.0f, uses conditions[3].Expression(L"true").
            if (dimension == ScrollPresenterDimension.HorizontalScroll)
            {
                var expressionClampToMinPosition = "min(sceas.Multiplier * scvis.DeltaPosition.X, it.Position.X - it.MinPosition.X)";
                var expressionClampToMaxPosition = "max(sceas.Multiplier * scvis.DeltaPosition.X, it.Position.X - it.MaxPosition.X)";

                values[0].Expression = (expressionClampToMinPosition);
                values[1].Expression = (expressionClampToMaxPosition);
                values[2].Expression = (expressionClampToMaxPosition);
                values[3].Expression = (expressionClampToMinPosition);
                scrollControllerVisualInteractionSource.ConfigureDeltaPositionXModifiers(modifiersVector);
            }
            else
            {
                var expressionClampToMinPosition = "min(sceas.Multiplier * scvis.DeltaPosition.X, it.Position.Y - it.MinPosition.Y)";
                var expressionClampToMaxPosition = "max(sceas.Multiplier * scvis.DeltaPosition.X, it.Position.Y - it.MaxPosition.Y)";

                values[0].Expression = (expressionClampToMinPosition);
                values[1].Expression = (expressionClampToMaxPosition);
                values[2].Expression = (expressionClampToMaxPosition);
                values[3].Expression = (expressionClampToMinPosition);
                scrollControllerVisualInteractionSource.ConfigureDeltaPositionYModifiers(modifiersVector);

                // When the IScrollController's Visual moves horizontally and controls the vertical ScrollPresenter.Content movement, make sure that the
                // vertical finger movements do not affect the ScrollPresenter.Content vertically. The vertical component of the finger movement is filtered out.
                CompositionConditionalValue ccvOrtho = CompositionConditionalValue.Create(compositor);
                ExpressionAnimation conditionOrtho = compositor.CreateExpressionAnimation("true");
                ExpressionAnimation valueOrtho = compositor.CreateExpressionAnimation("0");
                ccvOrtho.Condition = (conditionOrtho);
                ccvOrtho.Value = (valueOrtho);

                var modifiersVectorOrtho = new List<CompositionConditionalValue>();
                modifiersVectorOrtho.Add(ccvOrtho);

                scrollControllerVisualInteractionSource.ConfigureDeltaPositionXModifiers(modifiersVectorOrtho);
            }
        }
        else
        {
            conditions[0].Expression = ("scvis.DeltaPosition.Y < 0.0f && sceas.Multiplier < 0.0f");
            conditions[1].Expression = ("scvis.DeltaPosition.Y < 0.0f && sceas.Multiplier >= 0.0f");
            conditions[2].Expression = ("scvis.DeltaPosition.Y >= 0.0f && sceas.Multiplier < 0.0f");
            // Case #4 <==> scvis.DeltaPosition.Y >= 0.0f && sceas.Multiplier > 0.0f, uses conditions[3].Expression(L"true").
            if (dimension == ScrollPresenterDimension.HorizontalScroll)
            {
                var expressionClampToMinPosition = "min(sceas.Multiplier * scvis.DeltaPosition.Y, it.Position.X - it.MinPosition.X)";
                var expressionClampToMaxPosition = "max(sceas.Multiplier * scvis.DeltaPosition.Y, it.Position.X - it.MaxPosition.X)";

                values[0].Expression = (expressionClampToMinPosition);
                values[1].Expression = (expressionClampToMaxPosition);
                values[2].Expression = (expressionClampToMaxPosition);
                values[3].Expression = (expressionClampToMinPosition);
                scrollControllerVisualInteractionSource.ConfigureDeltaPositionXModifiers(modifiersVector);

                // When the IScrollController's Visual moves vertically and controls the horizontal ScrollPresenter.Content movement, make sure that the
                // horizontal finger movements do not affect the ScrollPresenter.Content horizontally. The horizontal component of the finger movement is filtered out.
                CompositionConditionalValue ccvOrtho = CompositionConditionalValue.Create(compositor);
                ExpressionAnimation conditionOrtho = compositor.CreateExpressionAnimation("true");
                ExpressionAnimation valueOrtho = compositor.CreateExpressionAnimation("0");
                ccvOrtho.Condition = (conditionOrtho);
                ccvOrtho.Value = (valueOrtho);

                var modifiersVectorOrtho = new List<CompositionConditionalValue>();
                modifiersVectorOrtho.Add(ccvOrtho);

                scrollControllerVisualInteractionSource.ConfigureDeltaPositionYModifiers(modifiersVectorOrtho);
            }
            else
            {
                var expressionClampToMinPosition = "min(sceas.Multiplier * scvis.DeltaPosition.Y, it.Position.Y - it.MinPosition.Y)";
                var expressionClampToMaxPosition = "max(sceas.Multiplier * scvis.DeltaPosition.Y, it.Position.Y - it.MaxPosition.Y)";

                values[0].Expression = (expressionClampToMinPosition);
                values[1].Expression = (expressionClampToMaxPosition);
                values[2].Expression = (expressionClampToMaxPosition);
                values[3].Expression = (expressionClampToMinPosition);
                scrollControllerVisualInteractionSource.ConfigureDeltaPositionYModifiers(modifiersVector);
            }
        }
    }

    private void SetupScrollPresenterVisualInteractionSource()
    {
        Debug.Assert(m_scrollPresenterVisualInteractionSource is not null);

        SetupVisualInteractionSourceRailingMode(
            m_scrollPresenterVisualInteractionSource,
            ScrollPresenterDimension.HorizontalScroll,
            HorizontalScrollRailMode);

        SetupVisualInteractionSourceRailingMode(
            m_scrollPresenterVisualInteractionSource,
            ScrollPresenterDimension.VerticalScroll,
            VerticalScrollRailMode);

        SetupVisualInteractionSourceChainingMode(
            m_scrollPresenterVisualInteractionSource,
            ScrollPresenterDimension.HorizontalScroll,
            HorizontalScrollChainMode);

        SetupVisualInteractionSourceChainingMode(
            m_scrollPresenterVisualInteractionSource,
            ScrollPresenterDimension.VerticalScroll,
            VerticalScrollChainMode);

        SetupVisualInteractionSourceChainingMode(
            m_scrollPresenterVisualInteractionSource,
            ScrollPresenterDimension.ZoomFactor,
            ZoomChainMode);

        UpdateVisualInteractionSourceMode(
            ScrollPresenterDimension.HorizontalScroll);

        UpdateVisualInteractionSourceMode(
            ScrollPresenterDimension.VerticalScroll);

        SetupVisualInteractionSourceMode(
            m_scrollPresenterVisualInteractionSource,
            ZoomMode);
    }

    private void SetupSnapPoints<T>(
        HashSet<SnapPointWrapper<T>> snapPointsSet,
        ScrollPresenterDimension dimension)
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
        Debug.Assert(visualInteractionSource is not null);
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);
        Debug.Assert(m_interactionTracker is not null);

        bool isHorizontalDimension = dimension == ScrollPresenterDimension.HorizontalScroll;
        bool isRightToLeftDirection = FlowDirection == FlowDirection.RightToLeft;
        float xamlLayoutOffset = isHorizontalDimension ? m_contentLayoutOffsetX : m_contentLayoutOffsetY;

        // Note that resetting to 'nullptr' when xamlLayoutOffset is 0 and isRightToLeftDirection is False is not working, so the branch below
        // is used instead when flowDirectionChanged is True.
        if (xamlLayoutOffset == 0.0f && !(isHorizontalDimension && (isRightToLeftDirection || flowDirectionChanged)))
        {
            if (isHorizontalDimension)
            {
                visualInteractionSource.ConfigureCenterPointXModifiers(null);
                m_interactionTracker.ConfigureCenterPointXInertiaModifiers(null);
            }
            else
            {
                visualInteractionSource.ConfigureCenterPointYModifiers(null);
                m_interactionTracker.ConfigureCenterPointYInertiaModifiers(null);
            }
        }
        else
        {
            Compositor compositor = visualInteractionSource.Compositor;
            ExpressionAnimation conditionCenterPointModifier = compositor.CreateExpressionAnimation("true");
            CompositionConditionalValue conditionValueCenterPointModifier = CompositionConditionalValue.Create(compositor);
            string valueCenterPointModifierExpression;

            if (isHorizontalDimension)
            {
                valueCenterPointModifierExpression = isRightToLeftDirection ? "-visualInteractionSource.CenterPoint.X + xamlLayoutOffset" : "visualInteractionSource.CenterPoint.X - xamlLayoutOffset";
            }
            else
            {
                valueCenterPointModifierExpression = "visualInteractionSource.CenterPoint.Y - xamlLayoutOffset";
            }

            ExpressionAnimation valueCenterPointModifier = compositor.CreateExpressionAnimation(valueCenterPointModifierExpression);

            valueCenterPointModifier.SetReferenceParameter("visualInteractionSource", visualInteractionSource);
            valueCenterPointModifier.SetScalarParameter("xamlLayoutOffset", xamlLayoutOffset);

            conditionValueCenterPointModifier.Condition = (conditionCenterPointModifier);
            conditionValueCenterPointModifier.Value = (valueCenterPointModifier);

            var centerPointModifiers = new List<CompositionConditionalValue>();
            centerPointModifiers.Add(conditionValueCenterPointModifier);

            if (isHorizontalDimension)
            {
                visualInteractionSource.ConfigureCenterPointXModifiers(centerPointModifiers);
                m_interactionTracker.ConfigureCenterPointXInertiaModifiers(centerPointModifiers);
            }
            else
            {
                visualInteractionSource.ConfigureCenterPointYModifiers(centerPointModifiers);
                m_interactionTracker.ConfigureCenterPointYInertiaModifiers(centerPointModifiers);
            }
        }
    }

    private void SetupVisualInteractionSourceChainingMode(
        VisualInteractionSource visualInteractionSource,
        ScrollPresenterDimension dimension,
        ScrollingChainMode chainingMode)
    {
        Debug.Assert(visualInteractionSource is not null);

        InteractionChainingMode interactionChainingMode = InteractionChainingModeFromChainingMode(chainingMode);

        switch (dimension)
        {
            case ScrollPresenterDimension.HorizontalScroll:
                visualInteractionSource.PositionXChainingMode = (interactionChainingMode);
                break;

            case ScrollPresenterDimension.VerticalScroll:
                visualInteractionSource.PositionYChainingMode = (interactionChainingMode);
                break;

            case ScrollPresenterDimension.ZoomFactor:
                visualInteractionSource.ScaleChainingMode = (interactionChainingMode);
                break;

            default:
                Debug.Assert(false);
                break;
        }
    }

    private void SetupVisualInteractionSourceMode(
        VisualInteractionSource visualInteractionSource,
        ScrollPresenterDimension dimension,
        ScrollingScrollMode scrollMode)
    {
        Debug.Assert(visualInteractionSource is not null);
        Debug.Assert(scrollMode == ScrollingScrollMode.Enabled || scrollMode == ScrollingScrollMode.Disabled);

        InteractionSourceMode interactionSourceMode = InteractionSourceModeFromScrollMode(scrollMode);

        switch (dimension)
        {
            case ScrollPresenterDimension.HorizontalScroll:
                visualInteractionSource.PositionXSourceMode = (interactionSourceMode);
                break;

            case ScrollPresenterDimension.VerticalScroll:
                visualInteractionSource.PositionYSourceMode = (interactionSourceMode);
                break;

            default:
                Debug.Assert(false);
                break;
        }
    }

    private void SetupVisualInteractionSourceMode(
        VisualInteractionSource visualInteractionSource,
        ScrollingZoomMode zoomMode)
    {
        Debug.Assert(visualInteractionSource is not null);

        visualInteractionSource.ScaleSourceMode = (InteractionSourceModeFromZoomMode(zoomMode));
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

    private void SetupVisualInteractionSourceRedirectionMode(
        VisualInteractionSource visualInteractionSource)
    {
        Debug.Assert(visualInteractionSource is not null);

        VisualInteractionSourceRedirectionMode redirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadOnly;

        if (!IsInputKindIgnored(ScrollingInputKinds.MouseWheel))
        {
            redirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadAndPointerWheel;
        }

        visualInteractionSource.ManipulationRedirectionMode = (redirectionMode);
    }

    private void SnapPointsVectorItemInsertedHelper<T>(
        SnapPointWrapper<T> insertedItem,
        HashSet<SnapPointWrapper<T>> snapPointsSet)
    {
        throw new NotImplementedException();
    }

    private bool SnapPointsViewportChangedHelper<T>(
        IObservableVector<T> snapPoints, double viewport)
    {
        bool snapPointsNeedViewportUpdates = false;

        foreach (T snapPoint in snapPoints)
        {
            SnapPointBase winrtSnapPointBase = snapPoint as SnapPointBase;
            SnapPointBase snapPointBase = (winrtSnapPointBase);

            snapPointsNeedViewportUpdates |= snapPointBase.OnUpdateViewport(viewport);
        }

        return snapPointsNeedViewportUpdates;
    }

    private void StartExpressionAnimationSourcesAnimations()
    {
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(m_expressionAnimationSources is not null);
        Debug.Assert(m_positionSourceExpressionAnimation is not null);
        Debug.Assert(m_minPositionSourceExpressionAnimation is not null);
        Debug.Assert(m_maxPositionSourceExpressionAnimation is not null);
        Debug.Assert(m_zoomFactorSourceExpressionAnimation is not null);

        m_expressionAnimationSources.StartAnimation(s_positionSourcePropertyName, m_positionSourceExpressionAnimation);
        RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_positionSourcePropertyName /*propertyName*/);

        m_expressionAnimationSources.StartAnimation(s_minPositionSourcePropertyName, m_minPositionSourceExpressionAnimation);
        RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_minPositionSourcePropertyName /*propertyName*/);

        m_expressionAnimationSources.StartAnimation(s_maxPositionSourcePropertyName, m_maxPositionSourceExpressionAnimation);
        RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_maxPositionSourcePropertyName /*propertyName*/);

        m_expressionAnimationSources.StartAnimation(s_zoomFactorSourcePropertyName, m_zoomFactorSourceExpressionAnimation);
        RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_zoomFactorSourcePropertyName /*propertyName*/);
    }

    private void StartScrollControllerExpressionAnimationSourcesAnimations(
        ScrollPresenterDimension dimension)
    {
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            Debug.Assert(m_horizontalScrollControllerExpressionAnimationSources is not null);
            Debug.Assert(m_horizontalScrollControllerOffsetExpressionAnimation is not null);
            Debug.Assert(m_horizontalScrollControllerMaxOffsetExpressionAnimation is not null);

            m_horizontalScrollControllerExpressionAnimationSources.StartAnimation(s_offsetPropertyName, m_horizontalScrollControllerOffsetExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_offsetPropertyName /*propertyName*/);

            m_horizontalScrollControllerExpressionAnimationSources.StartAnimation(s_maxOffsetPropertyName, m_horizontalScrollControllerMaxOffsetExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_maxOffsetPropertyName /*propertyName*/);
        }
        else
        {
            Debug.Assert(m_verticalScrollControllerExpressionAnimationSources is not null);
            Debug.Assert(m_verticalScrollControllerOffsetExpressionAnimation is not null);
            Debug.Assert(m_verticalScrollControllerMaxOffsetExpressionAnimation is not null);

            m_verticalScrollControllerExpressionAnimationSources.StartAnimation(s_offsetPropertyName, m_verticalScrollControllerOffsetExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_offsetPropertyName /*propertyName*/);

            m_verticalScrollControllerExpressionAnimationSources.StartAnimation(s_maxOffsetPropertyName, m_verticalScrollControllerMaxOffsetExpressionAnimation);
            RaiseExpressionAnimationStatusChanged(true /*isExpressionAnimationStarted*/, s_maxOffsetPropertyName /*propertyName*/);
        }
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

    private bool StartTranslationAndZoomFactorExpressionAnimations(bool interruptCountdown = false)
    {
        if (m_translationAndZoomFactorAnimationsRestartTicksCountdown > 0)
        {
            // A Translation and Scale animations restart is pending after the Idle State was reached or a zoom factor change operation completed.
            m_translationAndZoomFactorAnimationsRestartTicksCountdown--;

            if (m_translationAndZoomFactorAnimationsRestartTicksCountdown == 0 || interruptCountdown)
            {
                // Countdown is over or state is no longer Idle, restart the Translation and Scale animations.
                Debug.Assert(m_interactionTracker is not null);

                if (m_translationAndZoomFactorAnimationsRestartTicksCountdown > 0)
                {
                    Debug.Assert(interruptCountdown);

                    m_translationAndZoomFactorAnimationsRestartTicksCountdown = 0;
                }

                StartTransformExpressionAnimations(Content);
            }
            else
            {
                // Countdown needs to continue.
                return false;
            }
        }

        return true;
    }

    private void StopScrollControllerExpressionAnimationSourcesAnimations(
        ScrollPresenterDimension dimension)
    {
        Debug.Assert(dimension == ScrollPresenterDimension.HorizontalScroll || dimension == ScrollPresenterDimension.VerticalScroll);

        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            Debug.Assert(m_horizontalScrollControllerExpressionAnimationSources is not null);

            m_horizontalScrollControllerExpressionAnimationSources.StopAnimation(s_offsetPropertyName);
            RaiseExpressionAnimationStatusChanged(false /*isExpressionAnimationStarted*/, s_offsetPropertyName /*propertyName*/);

            m_horizontalScrollControllerExpressionAnimationSources.StopAnimation(s_maxOffsetPropertyName);
            RaiseExpressionAnimationStatusChanged(false /*isExpressionAnimationStarted*/, s_maxOffsetPropertyName /*propertyName*/);
        }
        else
        {
            Debug.Assert(m_verticalScrollControllerExpressionAnimationSources is not null);

            m_verticalScrollControllerExpressionAnimationSources.StopAnimation(s_offsetPropertyName);
            RaiseExpressionAnimationStatusChanged(false /*isExpressionAnimationStarted*/, s_offsetPropertyName /*propertyName*/);

            m_verticalScrollControllerExpressionAnimationSources.StopAnimation(s_maxOffsetPropertyName);
            RaiseExpressionAnimationStatusChanged(false /*isExpressionAnimationStarted*/, s_maxOffsetPropertyName /*propertyName*/);
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

    private void StopTranslationAndZoomFactorExpressionAnimations()
    {
        if (m_zoomFactorExpressionAnimation is not null && m_animationRestartZoomFactor != m_zoomFactor)
        {
            // The zoom factor has changed since the last restart of the Translation and Scale animations.
            UIElement content = Content;

            if (m_translationAndZoomFactorAnimationsRestartTicksCountdown == 0)
            {
                // Stop Translation and Scale animations to trigger rasterization of Content, to avoid fuzzy text rendering for instance.
                StopTransformExpressionAnimations(content);

                // Trigger ScrollPresenter::OnCompositionTargetRendering calls in order to re-establish the Translation and Scale animations
                // after the Content rasterization was triggered within a few ticks.
                HookCompositionTargetRendering();
            }

            m_animationRestartZoomFactor = m_zoomFactor;
            m_translationAndZoomFactorAnimationsRestartTicksCountdown = s_translationAndZoomFactorAnimationsRestartTicks;
        }
    }

    private void UnhookCompositionTargetRendering()
    {
        Windows.UI.Xaml.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;
    }

    private void UnhookContentPropertyChanged(UIElement content)
    {
        if (content is not null)
        {
            FrameworkElement contentAsFE = content as FrameworkElement;

            if (contentAsFE is not null)
            {
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.MinWidthProperty, m_contentMinWidthChangedRevoker);
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.WidthProperty, m_contentWidthChangedRevoker);
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.MaxWidthProperty, m_contentMaxWidthChangedRevoker);
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.MinHeightProperty, m_contentMinHeightChangedRevoker);
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.HeightProperty, m_contentHeightChangedRevoker);
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.MaxHeightProperty, m_contentMaxHeightChangedRevoker);
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.HorizontalAlignmentProperty, m_contentHorizontalAlignmentChangedRevoker);
                contentAsFE.UnregisterPropertyChangedCallback(FrameworkElement.VerticalAlignmentProperty, m_contentVerticalAlignmentChangedRevoker);
            }
        }
    }

    private void UnhookHorizontalScrollControllerEvents()
    {
        throw new NotImplementedException();
    }

    private void UnhookHorizontalScrollControllerPanningInfoEvents()
    {
        throw new NotImplementedException();
    }

    private void UnhookScrollPresenterEvents()
    {
        UnregisterPropertyChangedCallback(FlowDirectionProperty, m_flowDirectionChangedRevoker);
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        BringIntoViewRequested -= OnBringIntoViewRequestedHandler;

        if (m_pointerPressedEventHandler is not null)
        {
            RemoveHandler(UIElement.PointerPressedEvent, m_pointerPressedEventHandler);
            m_pointerPressedEventHandler = null;
        }
    }

    private void UnhookVerticalScrollControllerEvents()
    {
        throw new NotImplementedException();
    }

    private void UnhookVerticalScrollControllerPanningInfoEvents()
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
        Debug.Assert(m_interactionTracker is not null);
        Debug.Assert(m_expressionAnimationSources is not null);

        m_expressionAnimationSources.InsertVector2(s_extentSourcePropertyName, new Vector2((float)(m_unzoomedExtentWidth), (float)(m_unzoomedExtentHeight)));
        m_expressionAnimationSources.InsertVector2(s_viewportSourcePropertyName, new Vector2((float)(m_viewportWidth), (float)(m_viewportHeight)));
    }

    private void UpdateManipulationRedirectionMode()
    {
        if (m_scrollPresenterVisualInteractionSource is not null)
        {
            SetupVisualInteractionSourceRedirectionMode(m_scrollPresenterVisualInteractionSource);
        }
    }

    private void UpdateOffset(ScrollPresenterDimension dimension, double zoomedOffset)
    {
        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            if (m_zoomedHorizontalOffset != zoomedOffset)
            {
                m_zoomedHorizontalOffset = zoomedOffset;
            }
        }
        else
        {
            Debug.Assert(dimension == ScrollPresenterDimension.VerticalScroll);
            if (m_zoomedVerticalOffset != zoomedOffset)
            {
                m_zoomedVerticalOffset = zoomedOffset;
            }
        }
    }

    private void UpdatePositionBoundaries(UIElement? content)
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
        if (dimension == ScrollPresenterDimension.HorizontalScroll)
        {
            if (m_horizontalScrollController is not null)
            {
                m_horizontalScrollController.SetIsScrollable(ComputedHorizontalScrollMode == ScrollingScrollMode.Enabled);
            }
        }
        else
        {
            Debug.Assert(dimension == ScrollPresenterDimension.VerticalScroll);

            if (m_verticalScrollController is not null)
            {
                m_verticalScrollController.SetIsScrollable(ComputedVerticalScrollMode == ScrollingScrollMode.Enabled);
            }
        }
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
        if (state != ScrollingInteractionState.Idle)
        {
            // Restart the interrupted expression animations sooner than planned to visualize the new view change immediately.
            StartTranslationAndZoomFactorExpressionAnimations(true /*interruptCountdown*/);
        }

        if (state != m_state)
        {
            m_state = state;
            RaiseStateChanged();
        }
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
        UIElement content = Content;
        UIElement thisAsUIE = this;
        double oldUnzoomedExtentWidth = m_unzoomedExtentWidth;
        double oldUnzoomedExtentHeight = m_unzoomedExtentHeight;
        double oldViewportWidth = m_viewportWidth;
        double oldViewportHeight = m_viewportHeight;

        Debug.Assert(!double.IsInfinity(unzoomedExtentWidth));
        Debug.Assert(!double.IsNaN(unzoomedExtentWidth));
        Debug.Assert(!double.IsInfinity(unzoomedExtentHeight));
        Debug.Assert(!double.IsNaN(unzoomedExtentHeight));

        Debug.Assert(!double.IsInfinity(viewportWidth));
        Debug.Assert(!double.IsNaN(viewportWidth));
        Debug.Assert(!double.IsInfinity(viewportHeight));
        Debug.Assert(!double.IsNaN(viewportHeight));

        Debug.Assert(unzoomedExtentWidth >= 0.0);
        Debug.Assert(unzoomedExtentHeight >= 0.0);
        Debug.Assert(!(content is null && unzoomedExtentWidth != 0.0));
        Debug.Assert(!(content is null && unzoomedExtentHeight != 0.0));

        bool horizontalExtentChanged = oldUnzoomedExtentWidth != unzoomedExtentWidth;
        bool verticalExtentChanged = oldUnzoomedExtentHeight != unzoomedExtentHeight;
        bool extentChanged = horizontalExtentChanged || verticalExtentChanged;

        bool horizontalViewportChanged = oldViewportWidth != viewportWidth;
        bool verticalViewportChanged = oldViewportHeight != viewportHeight;
        bool viewportChanged = horizontalViewportChanged || verticalViewportChanged;

        m_unzoomedExtentWidth = unzoomedExtentWidth;
        m_unzoomedExtentHeight = unzoomedExtentHeight;

        m_viewportWidth = viewportWidth;
        m_viewportHeight = viewportHeight;

        if (m_expressionAnimationSources is not null)
        {
            UpdateExpressionAnimationSources();
        }

        if ((extentChanged || renderSizeChanged) && (content is not null))
        {
            OnContentSizeChanged(content);
        }

        if (extentChanged || viewportChanged)
        {
            MaximizeInteractionTrackerOperationsTicksCountdown();
            UpdateScrollAutomationPatternProperties();
        }

        if (horizontalExtentChanged || horizontalViewportChanged)
        {
            // Updating the horizontal scroll mode because GetComputedScrollMode is function of the scrollable width.
            UpdateVisualInteractionSourceMode(ScrollPresenterDimension.HorizontalScroll);

            UpdateScrollControllerValues(ScrollPresenterDimension.HorizontalScroll);
        }

        if (verticalExtentChanged || verticalViewportChanged)
        {
            // Updating the vertical scroll mode because GetComputedScrollMode is function of the scrollable height.
            UpdateVisualInteractionSourceMode(ScrollPresenterDimension.VerticalScroll);

            UpdateScrollControllerValues(ScrollPresenterDimension.VerticalScroll);
        }

        if (horizontalViewportChanged && m_horizontalSnapPoints is not null && m_horizontalSnapPointsNeedViewportUpdates)
        {
            // At least one horizontal scroll snap point is not near-aligned and is thus sensitive to the
            // viewport width. Regenerate and set up all horizontal scroll snap points.
            var horizontalSnapPoints = m_horizontalSnapPoints as IObservableVector<ScrollSnapPointBase>;
            bool horizontalSnapPointsNeedViewportUpdates = SnapPointsViewportChangedHelper(
                horizontalSnapPoints,
                m_viewportWidth);
            Debug.Assert(horizontalSnapPointsNeedViewportUpdates);

            RegenerateSnapPointsSet(horizontalSnapPoints, m_sortedConsolidatedHorizontalSnapPoints);
            SetupSnapPoints(m_sortedConsolidatedHorizontalSnapPoints, ScrollPresenterDimension.HorizontalScroll);
        }

        if (verticalViewportChanged && m_verticalSnapPoints is not null && m_verticalSnapPointsNeedViewportUpdates)
        {
            // At least one vertical scroll snap point is not near-aligned and is thus sensitive to the
            // viewport height. Regenerate and set up all vertical scroll snap points.
            var verticalSnapPoints = m_verticalSnapPoints as IObservableVector<ScrollSnapPointBase>;
            bool verticalSnapPointsNeedViewportUpdates = SnapPointsViewportChangedHelper(
                verticalSnapPoints,
                m_viewportHeight);
            Debug.Assert(verticalSnapPointsNeedViewportUpdates);

            RegenerateSnapPointsSet(verticalSnapPoints, m_sortedConsolidatedVerticalSnapPoints);
            SetupSnapPoints(m_sortedConsolidatedVerticalSnapPoints, ScrollPresenterDimension.VerticalScroll);
        }

        if (extentChanged)
        {
            RaiseExtentChanged();
        }
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