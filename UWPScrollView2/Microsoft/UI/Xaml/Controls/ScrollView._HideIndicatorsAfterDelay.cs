using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void HideIndicatorsAfterDelay()
    {
        Debug.Assert(AreScrollControllersAutoHiding());

        if (!m_keepIndicatorsShowing && IsLoaded)
        {
            DispatcherTimer? hideIndicatorsTimer = null;

            if (m_hideIndicatorsTimer is not null)
            {
                hideIndicatorsTimer = m_hideIndicatorsTimer;
                if (hideIndicatorsTimer.IsEnabled)
                {
                    hideIndicatorsTimer.Stop();
                }
            }
            else
            {
                hideIndicatorsTimer = new DispatcherTimer();
                hideIndicatorsTimer.Interval = TimeSpan.FromTicks(s_noIndicatorCountdown);
                hideIndicatorsTimer.Tick += OnHideIndicatorsTimerTick;
                m_hideIndicatorsTimer = hideIndicatorsTimer;
            }

            hideIndicatorsTimer.Start();
        }
    }
}