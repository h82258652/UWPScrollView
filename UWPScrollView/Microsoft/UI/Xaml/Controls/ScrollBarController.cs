using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

internal class ScrollBarController : IScrollController
{
    private bool m_canScroll;

    private bool m_isScrollable;

    private bool m_isScrollingWithMouse;

    private ScrollBar m_scrollBar;

    public bool CanScroll
    {
        get
        {
            return m_canScroll;
        }
    }

    public bool IsScrollingWithMouse
    {
        get
        {
            return m_isScrollingWithMouse;
        }
    }

    public IScrollControllerPanningInfo? PanningInfo
    {
        get
        {
            return null;
        }
    }

    public CompositionAnimation? GetScrollAnimation(int correlationId, Vector2 startPosition, Vector2 endPosition, CompositionAnimation defaultAnimation)
    {
        throw new NotImplementedException();
    }

    public void SetValues(double minOffset, double maxOffset, double offset, double viewportLength)
    {
        throw new NotImplementedException();
    }

    private void HookScrollBarEvent()
    {
        throw new NotImplementedException();
    }

    private void HookScrollBarPropertyChanged()
    {
        throw new NotImplementedException();
    }

    private void OnScroll(
           object sender,
           ScrollEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnScrollBarPropertyChanged(
        DependencyObject sender,
        DependencyProperty args)
    {
        throw new NotImplementedException();
    }

    private bool RaiseAddScrollVelocityRequested(
            double offsetChange)
    {
        throw new NotImplementedException();
    }

    private void RaiseCanScrollChanged()
    {
        throw new NotImplementedException();
    }

    private void RaiseIsScrollingWithMouseChanged()
    {
        throw new NotImplementedException();
    }

    private bool RaiseScrollByRequested(
        double offsetChange)
    {
        throw new NotImplementedException();
    }

    private bool RaiseScrollToRequested(
           double offset)
    {
        throw new NotImplementedException();
    }

    private void SetScrollBar(ScrollBar scrollBar)
    {
        UnhookScrollBarEvent();

        m_scrollBar = scrollBar;

        HookScrollBarEvent();
        HookScrollBarPropertyChanged();
    }

    private void UnhookScrollBarEvent()
    {
        throw new NotImplementedException();
    }

    private void UnhookScrollBarPropertyChanged()
    {
        throw new NotImplementedException();
    }

    private void UpdateCanScroll()
    {
        throw new NotImplementedException();
    }
}