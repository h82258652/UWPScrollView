using System;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnScrollViewUnloaded(object sender, RoutedEventArgs args)
    {
        m_showingMouseIndicators = false;
        m_keepIndicatorsShowing = false;
        m_bringIntoViewOperations.Clear();

        UnhookCompositionTargetRendering();
        ResetHideIndicatorsTimer();
    }
}