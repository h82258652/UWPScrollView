namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void UpdateScrollControllersAutoHiding(bool forceUpdate = false)
    {
        if ((forceUpdate || m_autoHideScrollBarsState.m_uiSettings5 is null) && m_autoHideScrollControllersValid)
        {
            m_autoHideScrollControllersValid = false;

            bool oldAutoHideScrollControllers = m_autoHideScrollControllers;
            bool newAutoHideScrollControllers = AreScrollControllersAutoHiding();

            if (oldAutoHideScrollControllers != newAutoHideScrollControllers)
            {
                UpdateVisualStates(
                    true  /*useTransitions*/,
                    false /*showIndicators*/,
                    false /*hideIndicators*/,
                    true  /*scrollControllersAutoHidingChanged*/);
            }
        }
    }
}