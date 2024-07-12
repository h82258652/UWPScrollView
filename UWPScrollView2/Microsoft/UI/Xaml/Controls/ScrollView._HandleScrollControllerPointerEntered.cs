namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void HandleScrollControllerPointerEntered(bool isForHorizontalScrollController)
    {
        if (isForHorizontalScrollController)
        {
            _isPointerOverHorizontalScrollController = true;
        }
        else
        {
            _isPointerOverVerticalScrollController = true;
        }

        UpdateScrollControllersAutoHiding();
        if (AreScrollControllersAutoHiding() && !SharedHelpers.IsAnimationsEnabled())
        {
            HideIndicatorsAfterDelay();
        }
    }
}