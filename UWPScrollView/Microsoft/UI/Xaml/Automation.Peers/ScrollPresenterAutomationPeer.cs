using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

namespace Microsoft.UI.Xaml.Automation.Peers;

internal class ScrollPresenterAutomationPeer : FrameworkElementAutomationPeer, IScrollProvider
{
    private static double s_maximumPercent = 100;
    private static double s_minimumPercent = 0;
    private static double s_noScroll = -1;
    private bool m_horizontallyScrollable;
    private double m_horizontalScrollPercent = ScrollPatternIdentifiers.NoScroll;
    private double m_horizontalViewSize = s_maximumPercent;
    private bool m_verticallyScrollable;

    private double m_verticalScrollPercent = ScrollPatternIdentifiers.NoScroll;

    private double m_verticalViewSize = s_maximumPercent;

    public ScrollPresenterAutomationPeer(ScrollPresenter owner) : base(owner)
    {
    }

    public bool HorizontallyScrollable => throw new NotImplementedException();

    public double HorizontalScrollPercent
    {
        get
        {
            Debug.Assert(m_horizontalScrollPercent == get_HorizontalScrollPercentImpl());

            return m_horizontalScrollPercent;
        }
    }

    public double HorizontalViewSize => throw new NotImplementedException();

    public bool VerticallyScrollable => throw new NotImplementedException();

    public double VerticalScrollPercent => throw new NotImplementedException();

    public double VerticalViewSize => throw new NotImplementedException();

    public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
    {
        if (!IsEnabled())
        {
            throw new ElementNotEnabledException();
        }

        bool scrollHorizontally = horizontalAmount != ScrollAmount.NoAmount;
        bool scrollVertically = verticalAmount != ScrollAmount.NoAmount;

        bool isHorizontallyScrollable = HorizontallyScrollable;
        bool isVerticallyScrollable = VerticallyScrollable;

        if (!(scrollHorizontally && !isHorizontallyScrollable) && !(scrollVertically && !isVerticallyScrollable))
        {
            var scrollPresenter = (GetScrollPresenter());
            bool isInvalidOperation = false;

            switch (horizontalAmount)
            {
                case ScrollAmount.LargeDecrement:
                    scrollPresenter.PageLeft();
                    break;

                case ScrollAmount.LargeIncrement:
                    scrollPresenter.PageRight();
                    break;

                case ScrollAmount.SmallDecrement:
                    scrollPresenter.LineLeft();
                    break;

                case ScrollAmount.SmallIncrement:
                    scrollPresenter.LineRight();
                    break;

                case ScrollAmount.NoAmount:
                    break;

                default:
                    isInvalidOperation = true;
                    break;
            }

            if (!isInvalidOperation)
            {
                switch (verticalAmount)
                {
                    case ScrollAmount.LargeDecrement:
                        scrollPresenter.PageUp();
                        return;

                    case ScrollAmount.SmallDecrement:
                        scrollPresenter.LineUp();
                        return;

                    case ScrollAmount.SmallIncrement:
                        scrollPresenter.LineDown();
                        return;

                    case ScrollAmount.LargeIncrement:
                        scrollPresenter.PageDown();
                        return;

                    case ScrollAmount.NoAmount:
                        return;
                }
            }
        }

        throw new InvalidOperationException("Cannot perform the operation.");
    }

    public void SetScrollPercent(double horizontalPercent, double verticalPercent)
    {
        if (!IsEnabled())
        {
            throw new ElementNotEnabledException();
        }

        bool scrollHorizontally = horizontalPercent != s_noScroll;
        bool scrollVertically = verticalPercent != s_noScroll;

        if (!scrollHorizontally && !scrollVertically)
        {
            return;
        }

        bool isHorizontallyScrollable = HorizontallyScrollable;
        bool isVerticallyScrollable = VerticallyScrollable;

        if ((scrollHorizontally && !isHorizontallyScrollable) || (scrollVertically && !isVerticallyScrollable))
        {
            throw new InvalidOperationException("Cannot perform the operation.");
        }

        if ((scrollHorizontally && (horizontalPercent < s_minimumPercent || horizontalPercent > s_maximumPercent)) ||
            (scrollVertically && (verticalPercent < s_minimumPercent || verticalPercent > s_maximumPercent)))
        {
            throw new ArgumentException();
        }

        var scrollPresenter = (GetScrollPresenter());

        if (scrollHorizontally && !scrollVertically)
        {
            double maxOffset = scrollPresenter.ScrollableWidth;

            scrollPresenter.ScrollToHorizontalOffset(maxOffset * horizontalPercent / s_maximumPercent);
        }
        else if (scrollVertically && !scrollHorizontally)
        {
            double maxOffset = scrollPresenter.ScrollableHeight;

            scrollPresenter.ScrollToVerticalOffset(maxOffset * verticalPercent / s_maximumPercent);
        }
        else
        {
            double maxHorizontalOffset = scrollPresenter.ScrollableWidth;
            double maxVerticalOffset = scrollPresenter.ScrollableHeight;

            scrollPresenter.ScrollToOffsets(
                maxHorizontalOffset * horizontalPercent / s_maximumPercent, maxVerticalOffset * verticalPercent / s_maximumPercent);
        }
    }

    public void UpdateScrollPatternProperties()
    {
        double newHorizontalScrollPercent = get_HorizontalScrollPercentImpl();
        double newVerticalScrollPercent = get_VerticalScrollPercentImpl();
        double newHorizontalViewSize = get_HorizontalViewSizeImpl();
        double newVerticalViewSize = get_VerticalViewSizeImpl();
        bool newHorizontallyScrollable = get_HorizontallyScrollableImpl();
        bool newVerticallyScrollable = get_VerticallyScrollableImpl();

        if (newHorizontallyScrollable != m_horizontallyScrollable)
        {
            bool oldHorizontallyScrollable = m_horizontallyScrollable;
            m_horizontallyScrollable = newHorizontallyScrollable;
            RaisePropertyChangedEvent(
                ScrollPatternIdentifiers.HorizontallyScrollableProperty,
                oldHorizontallyScrollable,
                newHorizontallyScrollable);
        }

        if (newVerticallyScrollable != m_verticallyScrollable)
        {
            bool oldVerticallyScrollable = m_verticallyScrollable;
            m_verticallyScrollable = newVerticallyScrollable;
            RaisePropertyChangedEvent(
                ScrollPatternIdentifiers.HorizontallyScrollableProperty,
                oldVerticallyScrollable,
                newVerticallyScrollable);
        }

        if (newHorizontalViewSize != m_horizontalViewSize)
        {
            double oldHorizontalViewSize = m_horizontalViewSize;
            m_horizontalViewSize = newHorizontalViewSize;
            RaisePropertyChangedEvent(
                ScrollPatternIdentifiers.HorizontalViewSizeProperty,
                oldHorizontalViewSize,
                newHorizontalViewSize);
        }

        if (newVerticalViewSize != m_verticalViewSize)
        {
            double oldVerticalViewSize = m_verticalViewSize;
            m_verticalViewSize = newVerticalViewSize;
            RaisePropertyChangedEvent(
                ScrollPatternIdentifiers.VerticalViewSizeProperty,
                oldVerticalViewSize,
                newVerticalViewSize);
        }

        if (newHorizontalScrollPercent != m_horizontalScrollPercent)
        {
            double oldHorizontalScrollPercent = m_horizontalScrollPercent;
            m_horizontalScrollPercent = newHorizontalScrollPercent;
            RaisePropertyChangedEvent(
                ScrollPatternIdentifiers.HorizontalScrollPercentProperty,
                oldHorizontalScrollPercent,
                newHorizontalScrollPercent);
        }

        if (newVerticalScrollPercent != m_verticalScrollPercent)
        {
            double oldVerticalScrollPercent = m_verticalScrollPercent;
            m_verticalScrollPercent = newVerticalScrollPercent;
            RaisePropertyChangedEvent(
                ScrollPatternIdentifiers.VerticalScrollPercentProperty,
                oldVerticalScrollPercent,
                (newVerticalScrollPercent));
        }
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Pane;
    }

    protected override object GetPatternCore(PatternInterface patternInterface)
    {
        return GetPatternCoreImpl(patternInterface);
    }

    private bool get_HorizontallyScrollableImpl()
    {
        throw new NotImplementedException();
    }

    private double get_HorizontalScrollPercentImpl()
    {
        ScrollPresenter scrollPresenter = GetScrollPresenter();

        return GetScrollPercent(
            scrollPresenter.GetZoomedExtentWidth(),
            scrollPresenter.ViewportWidth,
            GetScrollPresenter().HorizontalOffset);
    }

    private static double GetScrollPercent(double zoomedExtent, double viewport, double offset)
    {
        Debug.Assert(zoomedExtent >= 0.0);
        Debug.Assert(viewport >= 0.0);

        if (viewport >= zoomedExtent)
        {
            return ScrollPatternIdentifiers.NoScroll;
        }

        double scrollPercent = offset / (zoomedExtent - viewport) * s_maximumPercent;

        scrollPercent = Math.Max(scrollPercent, s_minimumPercent);
        scrollPercent = Math.Min(scrollPercent, s_maximumPercent);

        return scrollPercent;
    }

    private double get_HorizontalViewSizeImpl()
    {
        ScrollPresenter scrollPresenter = (GetScrollPresenter());

        return GetViewPercent(
            scrollPresenter.GetZoomedExtentWidth(),
            scrollPresenter.ViewportWidth);
    }

    private static double GetViewPercent(double zoomedExtent, double viewport)
    {
        throw new NotImplementedException();
    }

    private bool get_VerticallyScrollableImpl()
    {
        ScrollPresenter scrollPresenter = (GetScrollPresenter());

        return scrollPresenter.ScrollableHeight > 0.0;
    }

    private double get_VerticalScrollPercentImpl()
    {
        ScrollPresenter scrollPresenter = (GetScrollPresenter());

        return GetScrollPercent(
            scrollPresenter.GetZoomedExtentHeight(),
            scrollPresenter.ViewportHeight,
            GetScrollPresenter().VerticalOffset);
    }

    private double get_VerticalViewSizeImpl()
    {
        throw new NotImplementedException();
    }

    private object GetPatternCoreImpl(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.Scroll)
        {
            return this;
        }

        return base.GetPatternCore(patternInterface);
    }

    private ScrollPresenter GetScrollPresenter()
    {
        UIElement owner = Owner;
        return owner as ScrollPresenter;
    }
}