using Windows.System;
using Windows.UI.ViewManagement;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnAutoHideScrollBarsChanged(UISettings uiSettings, UISettingsAutoHideScrollBarsChangedEventArgs args)
    {
        // OnAutoHideScrollBarsChanged is called on a non-UI thread, process notification on the UI thread using a dispatcher.
        m_dispatcherQueue.TryEnqueue(new DispatcherQueueHandler(() =>
        {
            m_autoHideScrollControllersValid = false;
            UpdateVisualStates(
                true  /*useTransitions*/,
                false /*showIndicators*/,
                false /*hideIndicators*/,
                true  /*scrollControllersAutoHidingChanged*/);
        }));
    }
}