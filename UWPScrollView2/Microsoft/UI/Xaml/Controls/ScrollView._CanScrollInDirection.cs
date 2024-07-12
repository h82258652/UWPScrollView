using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool CanScrollInDirection(FocusNavigationDirection direction)
    {
        bool result = false;
        switch (direction)
        {
            case FocusNavigationDirection.Down:
                result = CanScrollDown();
                break;

            case FocusNavigationDirection.Up:
                result = CanScrollUp();
                break;

            case FocusNavigationDirection.Left:
                result = CanScrollLeft();
                break;

            case FocusNavigationDirection.Right:
                result = CanScrollRight();
                break;
        }

        return result;
    }
}