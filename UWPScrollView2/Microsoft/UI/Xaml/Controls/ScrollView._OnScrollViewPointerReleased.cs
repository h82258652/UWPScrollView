using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewPointerReleased(object sender, PointerRoutedEventArgs args)
    {
        bool takeFocus = false;

        if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse && m_isLeftMouseButtonPressedForFocus)
        {
            m_isLeftMouseButtonPressedForFocus = false;
            takeFocus = true;
        }

        if (args.Handled)
        {
            return;
        }

        if (takeFocus)
        {
            bool tookFocus = Focus(FocusState.Pointer);
            args.Handled = tookFocus;
        }
    }
}