using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

internal class ScrollBarController : IScrollController
{
    /// <summary>
    /// Default amount to scroll when hitting the SmallIncrement/SmallDecrement buttons: 1/8 of the viewport size.
    /// This amount can be overridden by setting the ScrollBar.SmallChange property to something else than double.NaN.
    /// </summary>
    private const double DefaultViewportToSmallChangeRatio = 8;

    /// <summary>
    /// Inertia decay rate for SmallChange / LargeChange animated Value changes.
    /// </summary>
    private const float InertiaDecayRate = 0.9995f;

    /// <summary>
    /// Additional velocity at Minimum and Maximum positions to ensure hitting the extreme Value.
    /// </summary>
    private const double MinMaxEpsilon = 0.001;

    /// <summary>
    /// Additional velocity required with decay s_inertiaDecayRate to move Position by one pixel.
    /// </summary>
    private const double VelocityNeededPerPixel = 7.600855902349023;

    private bool _canScroll;
    private bool _isScrollable;
    private bool _isScrollingWithMouse;
    private double _lastOffset;
    private int _lastOffsetChangeCorrelationIdForAddScrollVelocity = -1;
    private int _lastOffsetChangeCorrelationIdForScrollBy = -1;
    private int _lastOffsetChangeCorrelationIdForScrollTo = -1;
    private double _lastScrollBarValue;
    private int _operationsCount;
    private ScrollBar? _scrollBar;
    private long _scrollBarIsEnabledChangedToken;

    /// <inheritdoc/>
    public event TypedEventHandler<IScrollController, ScrollControllerAddScrollVelocityRequestedEventArgs>? AddScrollVelocityRequested;

    /// <inheritdoc/>
    public event TypedEventHandler<IScrollController, object?>? CanScrollChanged;

    /// <inheritdoc/>
    public event TypedEventHandler<IScrollController, object?>? IsScrollingWithMouseChanged;

    /// <inheritdoc/>
    public event TypedEventHandler<IScrollController, ScrollControllerScrollByRequestedEventArgs>? ScrollByRequested;

    /// <inheritdoc/>
    public event TypedEventHandler<IScrollController, ScrollControllerScrollToRequestedEventArgs>? ScrollToRequested;

    /// <inheritdoc/>
    public bool CanScroll => _canScroll;

    /// <inheritdoc/>
    public bool IsScrollingWithMouse => _isScrollingWithMouse;

    /// <inheritdoc/>
    public IScrollControllerPanningInfo? PanningInfo => null;

    /// <inheritdoc/>
    public CompositionAnimation? GetScrollAnimation(int correlationId, Vector2 startPosition, Vector2 endPosition, CompositionAnimation defaultAnimation)
    {
        // Using the consumer's default animation.
        return null;
    }

    /// <inheritdoc/>
    public void NotifyRequestedScrollCompleted(int correlationId)
    {
        Debug.Assert(_operationsCount > 0);
        _operationsCount--;

        if (_operationsCount == 0 && _scrollBar is not null && _scrollBar.Value != _lastOffset)
        {
            _scrollBar.Value = _lastOffset;
            _lastScrollBarValue = _lastOffset;
        }
    }

    /// <inheritdoc/>
    public void SetIsScrollable(bool isScrollable)
    {
        _isScrollable = isScrollable;

        UpdateCanScroll();
    }

    /// <inheritdoc/>
    public void SetValues(double minOffset, double maxOffset, double offset, double viewportLength)
    {
        if (maxOffset < minOffset)
        {
            throw new ArgumentException("maxOffset cannot be smaller than minOffset.");
        }

        if (viewportLength < 0.0)
        {
            throw new ArgumentException("viewportLength cannot be negative.");
        }

        offset = Math.Max(minOffset, offset);
        offset = Math.Min(maxOffset, offset);
        _lastOffset = offset;

        Debug.Assert(_scrollBar is not null);

        if (minOffset < _scrollBar.Minimum)
        {
            _scrollBar.Minimum = minOffset;
        }

        if (maxOffset > _scrollBar.Maximum)
        {
            _scrollBar.Maximum = maxOffset;
        }

        if (minOffset != _scrollBar.Minimum)
        {
            _scrollBar.Minimum = minOffset;
        }

        if (maxOffset != _scrollBar.Maximum)
        {
            _scrollBar.Maximum = maxOffset;
        }

        _scrollBar.ViewportSize = viewportLength;
        _scrollBar.LargeChange = viewportLength;
        _scrollBar.SmallChange = Math.Max(1.0, viewportLength / DefaultViewportToSmallChangeRatio);

        // The ScrollBar Value is only updated when there is no operation in progress otherwise the Scroll
        // event handler, ScrollBarScroll, may initiate a new request impeding the on-going operation.
        if (_operationsCount == 0 || _scrollBar.Value < minOffset || _scrollBar.Value > maxOffset)
        {
            _scrollBar.Value = offset;
            _lastScrollBarValue = offset;
        }

        // Potentially changed ScrollBar.Minimum / ScrollBar.Maximum value(s) may have an effect
        // on the read-only IScrollController.CanScroll property.
        UpdateCanScroll();
    }

    private void HookScrollBarEvent()
    {
        if (_scrollBar is not null)
        {
            _scrollBar.Scroll += OnScroll;
        }
    }

    private void HookScrollBarPropertyChanged()
    {
        Debug.Assert(_scrollBarIsEnabledChangedToken == 0);

        if (_scrollBar is not null)
        {
            _scrollBarIsEnabledChangedToken = _scrollBar.RegisterPropertyChangedCallback(Control.IsEnabledProperty, OnScrollBarPropertyChanged);
        }
    }

    private void OnScroll(object sender, ScrollEventArgs args)
    {
        var scrollEventType = args.ScrollEventType;

        if (_scrollBar is null)
        {
            return;
        }

        if (!_isScrollable && scrollEventType != ScrollEventType.ThumbPosition)
        {
            // This ScrollBar is not interactive. Restore its previous Value.
            _scrollBar.Value = _lastScrollBarValue;
            return;
        }

        switch (scrollEventType)
        {
            case ScrollEventType.First:
            case ScrollEventType.Last:
                {
                    break;
                }
            case ScrollEventType.EndScroll:
                {
                    if (_isScrollingWithMouse)
                    {
                        _isScrollingWithMouse = false;
                        RaiseIsScrollingWithMouseChanged();
                    }
                    break;
                }
            case ScrollEventType.LargeDecrement:
            case ScrollEventType.LargeIncrement:
            case ScrollEventType.SmallDecrement:
            case ScrollEventType.SmallIncrement:
            case ScrollEventType.ThumbPosition:
            case ScrollEventType.ThumbTrack:
                {
                    if (scrollEventType == ScrollEventType.ThumbTrack)
                    {
                        if (!_isScrollingWithMouse)
                        {
                            _isScrollingWithMouse = true;
                            RaiseIsScrollingWithMouseChanged();
                        }
                    }

                    bool offsetChangeRequested = false;

                    if (scrollEventType == ScrollEventType.ThumbPosition ||
                        scrollEventType == ScrollEventType.ThumbTrack)
                    {
                        offsetChangeRequested = RaiseScrollToRequested(args.NewValue);
                    }
                    else
                    {
                        double offsetChange = 0.0;

                        switch (scrollEventType)
                        {
                            case ScrollEventType.LargeDecrement:
                                offsetChange = -Math.Min(_lastScrollBarValue - _scrollBar.Minimum, _scrollBar.LargeChange);
                                break;

                            case ScrollEventType.LargeIncrement:
                                offsetChange = Math.Min(_scrollBar.Maximum - _lastScrollBarValue, _scrollBar.LargeChange);
                                break;

                            case ScrollEventType.SmallDecrement:
                                offsetChange = -Math.Min(_lastScrollBarValue - _scrollBar.Minimum, _scrollBar.SmallChange);
                                break;

                            case ScrollEventType.SmallIncrement:
                                offsetChange = Math.Min(_scrollBar.Maximum - _lastScrollBarValue, _scrollBar.SmallChange);
                                break;
                        }

                        // When the requested Value is near the Mininum or Maximum, include a little additional velocity
                        // to ensure the extreme value is reached.
                        if (args.NewValue - _scrollBar.Minimum < MinMaxEpsilon)
                        {
                            Debug.Assert(offsetChange < 0.0);
                            offsetChange -= MinMaxEpsilon;
                        }
                        else if (_scrollBar.Maximum - args.NewValue < MinMaxEpsilon)
                        {
                            Debug.Assert(offsetChange > 0.0);
                            offsetChange += MinMaxEpsilon;
                        }

                        if (SharedHelpers.IsAnimationsEnabled())
                        {
                            offsetChangeRequested = RaiseAddScrollVelocityRequested(offsetChange);
                        }
                        else
                        {
                            offsetChangeRequested = RaiseScrollByRequested(offsetChange);
                        }
                    }

                    if (!offsetChangeRequested)
                    {
                        // This request could not be requested, restore the previous Value.
                        _scrollBar.Value = _lastScrollBarValue;
                    }
                    break;
                }
        }

        _lastScrollBarValue = _scrollBar.Value;
    }

    private void OnScrollBarPropertyChanged(DependencyObject sender, DependencyProperty args)
    {
        Debug.Assert(_scrollBar is not null);

        if (args == Control.IsEnabledProperty)
        {
            UpdateCanScroll();
        }
    }

    private bool RaiseAddScrollVelocityRequested(double offsetChange)
    {
        if (AddScrollVelocityRequested is null)
        {
            return false;
        }

        ScrollControllerAddScrollVelocityRequestedEventArgs addScrollVelocityRequestedEventArgs = new ScrollControllerAddScrollVelocityRequestedEventArgs(
            (float)(offsetChange * VelocityNeededPerPixel),
            InertiaDecayRate);

        AddScrollVelocityRequested(this, addScrollVelocityRequestedEventArgs);

        // The CorrelationId property was set by the AddScrollVelocityRequested event handler.
        // Typically it is set to a new unique value, but it may also be set to the ID
        // from the prior request. This occurs when a request is quickly raised before
        // the prior one was handed off to the Composition layer. The back-to-back requests
        // are then coalesced into a single operation handed off to the Composition layer.
        int offsetChangeCorrelationId = addScrollVelocityRequestedEventArgs.CorrelationId;

        if (offsetChangeCorrelationId != -1)
        {
            // Only increment m_operationsCount when the returned CorrelationId represents a new request that was not coalesced with a pending request.
            if (offsetChangeCorrelationId != _lastOffsetChangeCorrelationIdForAddScrollVelocity)
            {
                _lastOffsetChangeCorrelationIdForAddScrollVelocity = offsetChangeCorrelationId;
                _operationsCount++;
            }

            return true;
        }

        return false;
    }

    private void RaiseCanScrollChanged()
    {
        CanScrollChanged?.Invoke(this, null);
    }

    private void RaiseIsScrollingWithMouseChanged()
    {
        IsScrollingWithMouseChanged?.Invoke(this, null);
    }

    private bool RaiseScrollByRequested(double offsetChange)
    {
        if (ScrollByRequested is null)
        {
            return false;
        }

        var options = new ScrollingScrollOptions(
            ScrollingAnimationMode.Disabled,
            ScrollingSnapPointsMode.Ignore);

        var scrollByRequestedEventArgs = new ScrollControllerScrollByRequestedEventArgs(
            offsetChange,
            options);

        ScrollByRequested(this, scrollByRequestedEventArgs);

        // The CorrelationId property was set by the ScrollByRequested event handler.
        // Typically it is set to a new unique value, but it may also be set to the ID
        // from the prior request. This occurs when a request is quickly raised before
        // the prior one was handed off to the Composition layer. The back-to-back requests
        // are then coalesced into a single operation handed off to the Composition layer.
        int offsetChangeCorrelationId = scrollByRequestedEventArgs.CorrelationId;

        if (offsetChangeCorrelationId != -1)
        {
            // Only increment m_operationsCount when the returned CorrelationId represents a new request that was not coalesced with a pending request.
            if (offsetChangeCorrelationId != _lastOffsetChangeCorrelationIdForScrollBy)
            {
                _lastOffsetChangeCorrelationIdForScrollBy = offsetChangeCorrelationId;
                _operationsCount++;
            }

            return true;
        }

        return false;
    }

    private bool RaiseScrollToRequested(double offset)
    {
        if (ScrollToRequested is null)
        {
            return false;
        }

        var options = new ScrollingScrollOptions(
            ScrollingAnimationMode.Disabled,
            ScrollingSnapPointsMode.Ignore);

        var scrollToRequestedEventArgs = new ScrollControllerScrollToRequestedEventArgs(
            offset,
            options);

        ScrollToRequested(this, scrollToRequestedEventArgs);

        // The CorrelationId property was set by the ScrollToRequested event handler.
        // Typically it is set to a new unique value, but it may also be set to the ID
        // from the prior request. This occurs when a request is quickly raised before
        // the prior one was handed off to the Composition layer. The back-to-back requests
        // are then coalesced into a single operation handed off to the Composition layer.
        int offsetChangeCorrelationId = scrollToRequestedEventArgs.CorrelationId;

        if (offsetChangeCorrelationId != -1)
        {
            // Only increment m_operationsCount when the returned CorrelationId represents a new request that was not coalesced with a pending request.
            if (offsetChangeCorrelationId != _lastOffsetChangeCorrelationIdForScrollTo)
            {
                _lastOffsetChangeCorrelationIdForScrollTo = offsetChangeCorrelationId;
                _operationsCount++;
            }

            return true;
        }

        return false;
    }

    private void SetScrollBar(ScrollBar scrollBar)
    {
        UnhookScrollBarEvent();

        _scrollBar = scrollBar;

        HookScrollBarEvent();
        HookScrollBarPropertyChanged();
    }

    private void UnhookScrollBarEvent()
    {
        if (_scrollBar is not null)
        {
            _scrollBar.Scroll -= OnScroll;
        }
    }

    private void UnhookScrollBarPropertyChanged()
    {
        if (_scrollBar is not null)
        {
            if (_scrollBarIsEnabledChangedToken != 0)
            {
                _scrollBar.UnregisterPropertyChangedCallback(Control.IsEnabledProperty, _scrollBarIsEnabledChangedToken);
                _scrollBarIsEnabledChangedToken = 0;
            }
        }
    }

    private void UpdateCanScroll()
    {
        bool oldCanScroll = _canScroll;

        _canScroll =
            _scrollBar is not null &&
            _scrollBar.Parent is not null &&
            _scrollBar.IsEnabled &&
            _scrollBar.Maximum > _scrollBar.Minimum &&
            _isScrollable;

        if (oldCanScroll != _canScroll)
        {
            RaiseCanScrollChanged();
        }
    }
}