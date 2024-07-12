using System.Diagnostics;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void HideIndicators(bool useTransitions = true)
    {
        Debug.Assert(AreScrollControllersAutoHiding());

        if (!AreAllScrollControllersCollapsed() && !m_keepIndicatorsShowing)
        {
            GoToState(_noIndicatorStateName, useTransitions);

            if (!m_hasNoIndicatorStateStoryboardCompletedHandler)
            {
                m_showingMouseIndicators = false;
            }
        }
    }
}