using Windows.Devices.Input;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewPointerEntered(object sender, PointerRoutedEventArgs args)
    {
        if (args.Pointer.PointerDeviceType != PointerDeviceType.Touch)
        {
            // Mouse/Pen inputs dominate. If touch panning indicators are shown, switch to mouse indicators.
            m_preferMouseIndicators = true;

            UpdateVisualStates(
                true  /*useTransitions*/,
                true  /*showIndicators*/,
                false /*hideIndicators*/,
                false /*scrollControllersAutoHidingChanged*/,
                true  /*updateScrollControllersAutoHiding*/,
                true  /*onlyForAutoHidingScrollControllers*/);
        }
    }
}