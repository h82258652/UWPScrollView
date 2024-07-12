using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private DependencyObject GetNextFocusCandidate(FocusNavigationDirection navigationDirection, bool isPageNavigation)
    {
        Debug.Assert(_scrollPresenter != null);
        Debug.Assert(navigationDirection != FocusNavigationDirection.None);
        var scrollPresenter = _scrollPresenter as ScrollPresenter;

        FocusNavigationDirection focusDirection = navigationDirection;

        FindNextElementOptions findNextElementOptions = new FindNextElementOptions();
        findNextElementOptions.SearchRoot = scrollPresenter.Content;

        if (isPageNavigation)
        {
            var localBounds = new Windows.Foundation.Rect(0, 0, scrollPresenter.ActualWidth, scrollPresenter.ActualHeight);
            var globalBounds = scrollPresenter.TransformToVisual(null).TransformBounds(localBounds);
            const int numPagesLookAhead = 2;

            var hintRect = globalBounds;
            switch (navigationDirection)
            {
                case FocusNavigationDirection.Down:
                    hintRect.Y += globalBounds.Height * numPagesLookAhead;
                    break;

                case FocusNavigationDirection.Up:
                    hintRect.Y -= globalBounds.Height * numPagesLookAhead;
                    break;

                case FocusNavigationDirection.Left:
                    hintRect.X -= globalBounds.Width * numPagesLookAhead;
                    break;

                case FocusNavigationDirection.Right:
                    hintRect.X += globalBounds.Width * numPagesLookAhead;
                    break;

                default:
                    break;
            }

            findNextElementOptions.HintRect = hintRect;
            findNextElementOptions.ExclusionRect = hintRect;
            focusDirection = FocusHelper.GetOppositeDirection(navigationDirection);
        }

        return FocusManager.FindNextElement(focusDirection, findNextElementOptions);

        throw new NotImplementedException();
    }
}