using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnHorizontalScrollControllerPointerExited(object sender, PointerRoutedEventArgs args)
    {
        HandleScrollControllerPointerExited(true /*isForHorizontalScrollController*/);
    }
}