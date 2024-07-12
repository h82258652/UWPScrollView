using System;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnHideIndicatorsTimerTick(object sender, object args)
    {
        ResetHideIndicatorsTimer();

        if (AreScrollControllersAutoHiding())
        {
            HideIndicators();
        }
    }
}