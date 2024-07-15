using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;

namespace Microsoft.UI;

internal static class RegUtil
{
    /// <summary>
    /// Used on RS4 and RS5 to indicate whether ScrollBars must auto-hide or not.
    /// </summary>
    /// <returns></returns>
    public static bool UseDynamicScrollbars()
    {
        if (ApiInformation.IsReadOnlyPropertyPresent("Windows.UI.ViewManagement.UISettings", "AutoHideScrollBars"))
        {
            UISettings uiSettings = new UISettings();
            return uiSettings.AutoHideScrollBars;
        }

        return true;
    }
}