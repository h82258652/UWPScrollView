namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool AreAllScrollControllersCollapsed()
    {
        return !SharedHelpers.IsAncestor(m_horizontalScrollControllerElement /*child*/, this /*parent*/, true /*checkVisibility*/) &&
            !SharedHelpers.IsAncestor(m_verticalScrollControllerElement /*child*/, this /*parent*/, true /*checkVisibility*/);
    }
}