using Windows.UI.Xaml.Input;

namespace Microsoft.UI
{
    internal static class FocusHelper
    {
        public static FocusNavigationDirection GetOppositeDirection(FocusNavigationDirection direction)
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
    }
}