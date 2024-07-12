namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool CanScrollLeft()
    {
        return CanScrollHorizontallyInDirection(false);
    }
}