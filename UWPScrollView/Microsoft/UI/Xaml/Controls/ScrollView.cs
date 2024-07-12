using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Numerics;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Represents a container that provides scroll, pan, and zoom support for its content.
/// </summary>
public class ScrollView : Control
{
    private const string s_horizontalScrollBarPartName = "PART_HorizontalScrollBar";
    private const string s_IScrollAnchorProviderNotImpl = "Template part named PART_ScrollPresenter does not implement IScrollAnchorProvider.";
    private const string s_noScrollPresenterPart = "No template part named PART_ScrollPresenter was loaded.";
    private const string s_rootPartName = "PART_Root";
    private const string s_scrollBarsSeparatorPartName = "PART_ScrollBarsSeparator";
    private const string s_scrollPresenterPartName = "PART_ScrollPresenter";
    private const string s_verticalScrollBarPartName = "PART_VerticalScrollBar";
    private ScrollPresenter? _scrollPresenter;
    private FocusInputDeviceKind m_focusInputDeviceKind = FocusInputDeviceKind.None;
    private DispatcherTimer m_hideIndicatorsTimer;
    private IScrollController m_horizontalScrollController;
    private UIElement m_horizontalScrollControllerElement;
    private bool m_isLeftMouseButtonPressedForFocus = false;
    private bool m_preferMouseIndicators = false;
    private UIElement m_scrollControllersSeparatorElement;
    private IScrollController m_verticalScrollController;
    private UIElement m_verticalScrollControllerElement;

    /// <summary>
    /// Gets a value that indicates the effective visibility of the horizontal scrollbar.
    /// </summary>
    public Visibility ComputedHorizontalScrollBarVisibility
    {
        get
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
    /// Gets a value that indicates the effective visibility of the vertical scrollbar.
    /// </summary>
    public Visibility ComputedVerticalScrollBarVisibility
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
    /// Gets or sets a value that indicates whether the content prefers to scroll horizontally or vertically.
    /// </summary>
    public ScrollingContentOrientation ContentOrientation
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
    /// Gets a <see cref="CompositionPropertySet"/> of scrolling related property values.
    /// </summary>
    public CompositionPropertySet ExpressionAnimationSources
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
    public double ExtentWidth => _scrollPresenter?.ExtentWidth ?? 0;

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
    public float ZoomFactor => _scrollPresenter?.ZoomFactor ?? 0;

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

    /// <inheritdoc/>
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        throw new NotImplementedException();
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

    private void HandleScrollControllerPointerEntered(bool isForHorizontalScrollController)
    {
        throw new NotImplementedException();
    }

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
        throw new NotImplementedException();
    }

    private void OnZoomAnimationStarting(object sender, ScrollingZoomAnimationStartingEventArgs args)
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