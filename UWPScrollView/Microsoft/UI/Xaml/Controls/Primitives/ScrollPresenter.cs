using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    /// Identifies the <see cref="ComputedHorizontalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedHorizontalScrollModeProperty;

    /// <summary>
    /// Identifies the <see cref="ComputedVerticalScrollMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ComputedVerticalScrollModeProperty;

    /// <summary>
    /// Identifies the <see cref="ContentOrientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentOrientationProperty;

    /// <summary>
    /// Identifies the <see cref="Content"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentProperty;

    public static readonly DependencyProperty HorizontalAnchorRatioProperty;

    public static readonly DependencyProperty HorizontalScrollChainModeProperty;

    public static readonly DependencyProperty VerticalAnchorRatioProperty;

    public static readonly DependencyProperty VerticalScrollChainModeProperty;

    /// <summary>
    /// Identifies the <see cref="VerticalScrollRailMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalScrollRailModeProperty;

    /// <summary>
    /// Identifies the <see cref="ZoomMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ZoomModeProperty;

    /// <summary>
    /// Identifies the <see cref="ZoomChainMode"/> dependency property.
    /// </summary>
    public static DependencyProperty ZoomChainModeProperty;

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
    private InteractionTracker m_interactionTracker;

    /// <summary>
    /// False when m_anchorElement is up-to-date, True otherwise.
    /// </summary>
    private bool m_isAnchorElementDirty = true;

    private ExpressionAnimation m_maxPositionExpressionAnimation;
    private ExpressionAnimation m_minPositionExpressionAnimation;
    private ScrollingInteractionState m_state = ScrollingInteractionState.Idle;
    private double m_unzoomedExtentHeight;

    private double m_unzoomedExtentWidth;

    private double m_viewportHeight;

    private double m_viewportWidth;

    private double m_zoomedHorizontalOffset;

    private double m_zoomedVerticalOffset;

    private float m_zoomFactor = 1;

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
    public event TypedEventHandler<ScrollPresenter, object> ExtentChanged;

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
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets a value that indicates the effective ability to scroll vertically by means of user input.
    /// </summary>
    public ScrollingScrollMode ComputedVerticalScrollMode
    {
        get
        {
            throw new NotImplementedException();
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
    public double HorizontalOffset
    {
        get
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        set
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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

    private static bool IsAnchorRatioValid(double value)
    {
        return double.IsNaN(value) || (!double.IsInfinity(value) && value >= 0 && value <= 1);
    }

    private void CustomAnimationStateEntered(InteractionTrackerCustomAnimationStateEnteredArgs args)
    {
        throw new NotImplementedException();
    }

    private void EnsureExpressionAnimationSources()
    {
        throw new NotImplementedException();
    }

    private void EnsureInteractionTracker()
    { throw new NotImplementedException(); }

    private void EnsurePositionBoundariesExpressionAnimations()
    {
        throw new NotImplementedException();
    }

    private double GetComputedMaxHeight(
        double defaultMaxHeight,
        FrameworkElement content)
    {
        throw new NotImplementedException();
    }

    private double GetComputedMaxWidth(
                               double defaultMaxWidth,
            FrameworkElement content)
    {
        throw new NotImplementedException();
    }

    private void OnFlowDirectionChanged(DependencyObject sender, DependencyProperty args)
    {
        throw new NotImplementedException();
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void RaisePostArrange()
    {
        throw new NotImplementedException();
    }

    private void ResetAnchorElement()
    {
        throw new NotImplementedException();
    }

    private void UpdateContent(UIElement oldContent, UIElement newContent)
    {
        throw new NotImplementedException();
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

    private void SetupInteractionTrackerZoomFactorBoundaries(
            double minZoomFactor, double maxZoomFactor)
    { throw new NotImplementedException(); }

    private void SetupPositionBoundariesExpressionAnimations(
            UIElement content)
    {
        throw new NotImplementedException();
    }
}