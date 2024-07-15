using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

internal class ScrollViewBringIntoViewOperation
{
    /// <summary>
    /// Number of UI thread ticks allowed before this expected bring-into-view operation is no
    /// longer expected and removed from the ScrollView's m_bringIntoViewOperations list.
    /// </summary>
    private const short s_maxTicksCount = 3;

    private bool m_cancelBringIntoView;

    private WeakReference<UIElement> m_targetElement;

    private short m_ticksCount;

    public ScrollViewBringIntoViewOperation(UIElement targetElement, bool cancelBringIntoView)
    {
        m_targetElement = new WeakReference<UIElement>(targetElement);
        m_cancelBringIntoView = cancelBringIntoView;
    }

    public bool HasMaxTicksCount
    {
        get
        {
            Debug.Assert(m_ticksCount <= s_maxTicksCount);

            return m_ticksCount == s_maxTicksCount;
        }
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

    public short TickOperation()
    {
        Debug.Assert(m_ticksCount < s_maxTicksCount);

        return ++m_ticksCount;
    }
}