using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public class ScrollPresenter : FrameworkElement, IScrollAnchorProvider
{
    public void RegisterAnchorCandidate(UIElement element)
    {
        throw new NotImplementedException();
    }

    public void UnregisterAnchorCandidate(UIElement element)
    {
        throw new NotImplementedException();
    }

    public UIElement CurrentAnchor => throw new NotImplementedException();
}