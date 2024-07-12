using Windows.Foundation;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public event TypedEventHandler<ScrollPresenter, ScrollingZoomAnimationStartingEventArgs>? ZoomAnimationStarting;
}