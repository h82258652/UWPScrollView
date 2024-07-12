using System;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

internal class ScrollViewBringIntoViewOperation
{
    private bool m_cancelBringIntoView;

    private WeakReference<UIElement> m_targetElement;

    private short m_ticksCount;

    public ScrollViewBringIntoViewOperation(UIElement targetElement, bool cancelBringIntoView)
    {
        m_targetElement = new WeakReference<UIElement>(targetElement);
        m_cancelBringIntoView = cancelBringIntoView;
    }

    public UIElement? TargetElement
    {
        get
        {
            m_targetElement.TryGetTarget(out var target);
            return target;
        }
    }

    public short TicksCount
    {
        get
        {
            return m_ticksCount;
        }
    }

    public bool ShouldCancelBringIntoView()
    {
        return m_cancelBringIntoView;
    }
}