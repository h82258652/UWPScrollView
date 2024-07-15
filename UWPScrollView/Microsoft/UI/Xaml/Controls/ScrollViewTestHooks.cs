using System;
using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Controls;

internal class ScrollViewTestHooks
{
    private static ScrollViewTestHooks s_testHooks;

    private Dictionary<ScrollView, bool?> m_autoHideScrollControllersMap;

    public static ScrollViewTestHooks EnsureGlobalTestHooks()
    {
        throw new NotImplementedException();
    }

    public static bool? GetAutoHideScrollControllers(ScrollView scrollView)
    {
        if (scrollView is not null && s_testHooks is not null)
        {
            var hooks = EnsureGlobalTestHooks();
            if (hooks.m_autoHideScrollControllersMap.ContainsKey(scrollView))
            {
                return hooks.m_autoHideScrollControllersMap[scrollView];
            }
        }

        return null;
    }

    public static ScrollViewTestHooks GetGlobalTestHooks()
    {
        return s_testHooks;
    }
}