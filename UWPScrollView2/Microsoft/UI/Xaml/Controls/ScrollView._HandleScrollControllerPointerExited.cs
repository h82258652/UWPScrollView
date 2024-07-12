namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void HandleScrollControllerPointerExited(bool isForHorizontalScrollController)
    {
        if (isForHorizontalScrollController)
        {
            _isPointerOverHorizontalScrollController = false;
        }
        else
        {
            _isPointerOverVerticalScrollController = false;
        }

        UpdateScrollControllersAutoHiding();
        if (AreScrollControllersAutoHiding())
        {
            HideIndicatorsAfterDelay();
        }
    }
}