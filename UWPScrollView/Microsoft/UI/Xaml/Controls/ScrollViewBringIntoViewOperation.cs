using System;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

internal class ScrollViewBringIntoViewOperation
{
    private bool m_cancelBringIntoView;

    private WeakReference<UIElement> m_targetElement;
}