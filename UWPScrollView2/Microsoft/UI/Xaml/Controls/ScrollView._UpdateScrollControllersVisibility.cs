using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void UpdateScrollControllersVisibility(bool horizontalChange, bool verticalChange)
    {
        Debug.Assert(horizontalChange || verticalChange);

        bool isHorizontalScrollControllerVisible = false;

        if (horizontalChange)
        {
            var scrollBarVisibility = HorizontalScrollBarVisibility;

            if (scrollBarVisibility == ScrollingScrollBarVisibility.Auto &&
                m_horizontalScrollController is not null &&
                m_horizontalScrollController.CanScroll)
            {
                isHorizontalScrollControllerVisible = true;
            }
            else
            {
                isHorizontalScrollControllerVisible = (scrollBarVisibility == ScrollingScrollBarVisibility.Visible);
            }

            SetValue(ComputedHorizontalScrollBarVisibilityProperty, (isHorizontalScrollControllerVisible ? Visibility.Visible : Visibility.Collapsed));
        }
        else
        {
            isHorizontalScrollControllerVisible = ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        }

        bool isVerticalScrollControllerVisible = false;

        if (verticalChange)
        {
            var scrollBarVisibility = VerticalScrollBarVisibility;

            if (scrollBarVisibility == ScrollingScrollBarVisibility.Auto &&
                m_verticalScrollController is not null &&
                m_verticalScrollController.CanScroll)
            {
                isVerticalScrollControllerVisible = true;
            }
            else
            {
                isVerticalScrollControllerVisible = (scrollBarVisibility == ScrollingScrollBarVisibility.Visible);
            }

            SetValue(ComputedVerticalScrollBarVisibilityProperty, (isVerticalScrollControllerVisible ? Visibility.Visible : Visibility.Collapsed));
        }
        else
        {
            isVerticalScrollControllerVisible = ComputedVerticalScrollBarVisibility == Visibility.Visible;
        }

        if (m_scrollControllersSeparatorElement is not null)
        {
            m_scrollControllersSeparatorElement.Visibility = (isHorizontalScrollControllerVisible && isVerticalScrollControllerVisible ?
                Visibility.Visible : Visibility.Collapsed);
        }

        throw new NotImplementedException();
    }
}