namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void OnIndicatorStateStoryboardCompleted(object sender, object args)
    {
        if (AreScrollControllersAutoHiding() &&
            !_keepIndicatorsShowing &&
            !_isPointerOverVerticalScrollController &&
            !_isPointerOverHorizontalScrollController)
        {
            UpdateScrollControllersVisualState(true /*useTransitions*/, false /*showIndicators*/, true /*hideIndicators*/);
        }
    }
}