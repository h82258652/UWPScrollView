namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void ResetHideIndicatorsTimer(bool isForDestructor = false, bool restart = false)
    {
        var hideIndicatorsTimer = m_hideIndicatorsTimer;

        if (hideIndicatorsTimer is not null && hideIndicatorsTimer.IsEnabled)
        {
            hideIndicatorsTimer.Stop();
            if (restart)
            {
                hideIndicatorsTimer.Start();
            }
        }
    }
}