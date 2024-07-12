using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public double ScrollableWidth
    {
        get
        {
            return Math.Max(0.0, GetZoomedExtentWidth() - ViewportWidth);
        }
    }
}