using System;
using Windows.Devices.Input;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewPointerCanceled(object sender, PointerRoutedEventArgs args)
    {
        if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
        {
            m_isLeftMouseButtonPressedForFocus = false;
        }
    }
}