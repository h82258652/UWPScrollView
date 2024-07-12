namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool CanScrollUp()
    {
        return CanScrollVerticallyInDirection(false);
    }
}