using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        UpdateVisualStates(
            true  /*useTransitions*/,
            false /*showIndicators*/,
            false /*hideIndicators*/,
            false /*scrollControllersAutoHidingChanged*/,
            true  /*updateScrollControllersAutoHiding*/);
    }
}