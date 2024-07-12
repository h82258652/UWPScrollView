using System;
using Windows.System;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI;

internal static class FocusHelper
{
    internal static FocusNavigationDirection GetOppositeDirection(FocusNavigationDirection direction)
    {
        FocusNavigationDirection oppositeDirection = FocusNavigationDirection.None;
        switch (direction)
        {
            case FocusNavigationDirection.Down:
                oppositeDirection = FocusNavigationDirection.Up;
                break;
            case FocusNavigationDirection.Up:
                oppositeDirection = FocusNavigationDirection.Down;
                break;
            case FocusNavigationDirection.Left:
                oppositeDirection = FocusNavigationDirection.Right;
                break;
            case FocusNavigationDirection.Right:
                oppositeDirection = FocusNavigationDirection.Left;
                break;
            case FocusNavigationDirection.Next:
                oppositeDirection = FocusNavigationDirection.Previous;
                break;
            case FocusNavigationDirection.Previous:
                oppositeDirection = FocusNavigationDirection.Next;
                break;
        }
        return oppositeDirection;
    }

    internal static bool IsGamepadNavigationDirection(VirtualKey key)
    {
        throw new NotImplementedException();
    }

    internal static bool IsGamepadPageNavigationDirection(VirtualKey key)
    {
        throw new NotImplementedException();
    }
}