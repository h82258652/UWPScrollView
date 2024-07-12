using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewPointerPressed(object sender, PointerRoutedEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
        {
            PointerPoint pointerPoint = args.GetCurrentPoint(null);
            PointerPointProperties pointerPointProperties = pointerPoint.Properties;

            m_isLeftMouseButtonPressedForFocus = pointerPointProperties.IsLeftButtonPressed;
        }

        // Show the scroll controller indicators as soon as a pointer is pressed on the ScrollView.
        UpdateVisualStates(
            true  /*useTransitions*/,
            true  /*showIndicators*/,
            false /*hideIndicators*/,
            false /*scrollControllersAutoHidingChanged*/,
            true  /*updateScrollControllersAutoHiding*/,
            true  /*onlyForAutoHidingScrollControllers*/);
    }
}