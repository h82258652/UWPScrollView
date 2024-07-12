namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool CanScrollRight()
    {
        return CanScrollHorizontallyInDirection(true);
    }
}