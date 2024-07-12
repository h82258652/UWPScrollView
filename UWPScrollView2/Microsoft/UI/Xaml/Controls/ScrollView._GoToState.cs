using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void GoToState(string stateName, bool useTransitions = true)
    {
        VisualStateManager.GoToState(this, stateName, useTransitions);
    }
}