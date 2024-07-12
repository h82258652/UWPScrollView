using System;
using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void OnScroll(object sender, ScrollEventArgs args)
    {
        ScrollEventType scrollEventType = args.ScrollEventType;

        if (_scrollBar is null)
        {
            return;
        }

        if (!_isScrollable && scrollEventType != ScrollEventType.ThumbPosition)
        {
            _scrollBar.Value = _lastScrollBarValue;
            return;
        }

        switch (scrollEventType)
        {
            case ScrollEventType.First: 
            case ScrollEventType.Last:
                break;
            case ScrollEventType.EndScroll:
                if (_isScrollingWithMouse)
                {
                    _isScrollingWithMouse = false;
                    RaiseIsScrollingWithMouseChanged();
                }
                break;
            case ScrollEventType.LargeDecrement:
            case ScrollEventType.LargeIncrement:
            case ScrollEventType.SmallDecrement:
            case ScrollEventType.SmallIncrement:
            case ScrollEventType.ThumbPosition:
            case ScrollEventType.ThumbTrack:
                if (scrollEventType == ScrollEventType.ThumbTrack)
                {
                    if (!_isScrollingWithMouse)
                    {
                        _isScrollingWithMouse = true;
                        RaiseIsScrollingWithMouseChanged();
                    }
                }

                bool offsetChangeRequested = false;

                if (scrollEventType == ScrollEventType.ThumbPosition ||
                    scrollEventType == ScrollEventType.ThumbTrack)
                {
                    offsetChangeRequested = RaiseScrollToRequested(args.NewValue);
                }
                else
                {
                    double offsetChange = 0.0;

                    switch (scrollEventType)
                    {
                        case ScrollEventType.LargeDecrement:
                            offsetChange = - Math.Min(_lastScrollBarValue - _scrollBar.Minimum, _scrollBar.LargeChange);
                            break;
                        case ScrollEventType.LargeIncrement:
                            offsetChange = Math.Min(_scrollBar.Maximum - _lastScrollBarValue, _scrollBar.LargeChange);
                            break;
                        case ScrollEventType.SmallDecrement:
                            offsetChange = -Math.Min(_lastScrollBarValue - _scrollBar.Minimum, _scrollBar.SmallChange);
                            break;
                        case ScrollEventType.SmallIncrement:
                            offsetChange = Math.Min(_scrollBar.Maximum - _lastScrollBarValue, _scrollBar.SmallChange);
                            break;
                    }

                    if (args.NewValue - _scrollBar.Minimum < _minMaxEpsilon)
                    {
                        offsetChange -= _minMaxEpsilon;
                    }
                    else if (_scrollBar.Maximum - args.NewValue < _minMaxEpsilon)
                    {
                        offsetChange += _minMaxEpsilon;
                    }

                    if (SharedHelpers.IsAnimationsEnabled())
                    {
                        offsetChangeRequested = RaiseAddScrollVelocityRequested(offsetChange);
                    }
                    else
                    {
                        offsetChangeRequested = RaiseScrollByRequested(offsetChange);
                    }
                }

                if (!offsetChangeRequested)
                {
                    _scrollBar.Value = _lastScrollBarValue;
                }

                break; 
        }

       _lastScrollBarValue = _scrollBar.Value;
    }
}