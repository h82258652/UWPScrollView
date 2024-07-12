using Microsoft.UI.Xaml.Controls.Primitives;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController : IScrollController
{
    private const double _defaultViewportToSmallChangeRatio = 8;
    private const double _minMaxEpsilon = 0.001;
    private bool _canScroll = false;

    private bool _isScrollable = false;

    private bool _isScrollingWithMouse = false;
    private double _lastOffset;

    private double _lastScrollBarValue = 0;


    private int _operationsCount;
    private ScrollBar? _scrollBar;
    private long _scrollBarIsEnabledChangedToken;

    private event TypedEventHandler<IScrollController, object?>? _isScrollingWithMouseChanged;

    private event TypedEventHandler<IScrollController, ScrollControllerScrollToRequestedEventArgs>? m_scrollToRequested;


    private event TypedEventHandler<IScrollController, object?>? m_canScrollChanged;
}