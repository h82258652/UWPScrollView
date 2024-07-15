using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

namespace Microsoft.UI.Xaml.Automation.Peers;

internal class ScrollPresenterAutomationPeer : FrameworkElementAutomationPeer, IScrollProvider
{
    private bool m_verticallyScrollable;

    public ScrollPresenterAutomationPeer(ScrollPresenter owner) : base(owner)
    {
        throw new NotImplementedException();
    }

    public bool HorizontallyScrollable => throw new NotImplementedException();

    public double HorizontalScrollPercent => throw new NotImplementedException();

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
        throw new NotImplementedException();
    }

    public void UpdateScrollPatternProperties()
    {
        throw new NotImplementedException();
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Pane;
    }

    protected override object GetPatternCore(PatternInterface patternInterface)
    {
        return GetPatternCoreImpl(patternInterface);
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