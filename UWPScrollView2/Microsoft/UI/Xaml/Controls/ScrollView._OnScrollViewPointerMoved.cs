using Windows.Devices.Input;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewPointerMoved(object sender, PointerRoutedEventArgs args)
    {
        // Don't process if this is a generated replay of the event.
        if (args.IsGenerated)
        {
            return;
        }

        if (args.Pointer.PointerDeviceType != PointerDeviceType.Touch)
        {
            // Mouse/Pen inputs dominate. If touch panning indicators are shown, switch to mouse indicators.
            m_preferMouseIndicators = true;

            UpdateVisualStates(
                true  /*useTransitions*/,
                true  /*showIndicators*/,
                false /*hideIndicators*/,
                false /*scrollControllersAutoHidingChanged*/,
                false /*updateScrollControllersAutoHiding*/,
                true  /*onlyForAutoHidingScrollControllers*/);

            if (AreScrollControllersAutoHiding() &&
                !SharedHelpers.IsAnimationsEnabled() &&
                m_hideIndicatorsTimer is not null &&
                (_isPointerOverHorizontalScrollController || _isPointerOverVerticalScrollController))
            {
                ResetHideIndicatorsTimer();
            }
        }
    }
}