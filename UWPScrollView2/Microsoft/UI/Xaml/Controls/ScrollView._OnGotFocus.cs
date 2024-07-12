using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);

        m_preferMouseIndicators =
            m_focusInputDeviceKind == FocusInputDeviceKind.Mouse ||
            m_focusInputDeviceKind == FocusInputDeviceKind.Pen;

        UpdateVisualStates(
            true  /*useTransitions*/,
            true  /*showIndicators*/,
            false /*hideIndicators*/,
            false /*scrollControllersAutoHidingChanged*/,
            true  /*updateScrollControllersAutoHiding*/,
            true  /*onlyForAutoHidingScrollControllers*/);
    }
}