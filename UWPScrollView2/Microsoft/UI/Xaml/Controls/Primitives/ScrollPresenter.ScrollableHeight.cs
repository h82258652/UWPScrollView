using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public double ScrollableHeight
    {
        get
        {
            return Math.Max(0.0, GetZoomedExtentHeight() - ViewportHeight);
        }
    }
}