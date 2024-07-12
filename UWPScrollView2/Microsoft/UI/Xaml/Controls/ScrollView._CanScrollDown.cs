namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private bool CanScrollDown()
    {
        return CanScrollVerticallyInDirection(true);
    }
}