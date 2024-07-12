using System;

namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    public void SetValues(double minOffset, double maxOffset, double offset, double viewportLength)
    {
        if (maxOffset < minOffset)
        {
            throw new ArgumentException("maxOffset cannot be smaller than minOffset.");
        }

        if (viewportLength < 0.0)
        {
            throw new ArgumentException("viewportLength cannot be negative.");
        }

        offset = Math.Max(minOffset, offset);
        offset = Math.Min(maxOffset, offset);
        _lastOffset = offset;

        if (minOffset < _scrollBar.Minimum)
        {
            _scrollBar.Minimum = minOffset;
        }

        if (maxOffset > _scrollBar.Maximum)
        {
            _scrollBar.Maximum = maxOffset;
        }

        if (minOffset != _scrollBar.Minimum)
        {
            _scrollBar.Minimum = minOffset;
        }

        if (maxOffset != _scrollBar.Maximum)
        {
            _scrollBar.Maximum = maxOffset;
        }

        _scrollBar.ViewportSize = viewportLength;
        _scrollBar.LargeChange = viewportLength;
        _scrollBar.SmallChange = Math.Max(1.0, viewportLength / _defaultViewportToSmallChangeRatio);

        // The ScrollBar Value is only updated when there is no operation in progress otherwise the Scroll
        // event handler, ScrollBarScroll, may initiate a new request impeding the on-going operation.
        if (_operationsCount == 0 || _scrollBar.Value < minOffset || _scrollBar.Value > maxOffset)
        {
            _scrollBar.Value = offset;
            _lastScrollBarValue = offset;
        }

        // Potentially changed ScrollBar.Minimum / ScrollBar.Maximum value(s) may have an effect
        // on the read-only IScrollController.CanScroll property.
        UpdateCanScroll();
    }
}