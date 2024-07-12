using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewGettingFocus(object sender, GettingFocusEventArgs args)
    {
        m_focusInputDeviceKind = args.InputDevice;
    }
}