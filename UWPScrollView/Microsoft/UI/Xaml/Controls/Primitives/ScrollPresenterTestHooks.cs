using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class ScrollPresenterTestHooks
{
    public static void GetOffsetsChangeVelocityParameters(out Int32 millisecondsPerUnit, out Int32 minMilliseconds, out Int32 maxMilliseconds)
    {
        throw new NotImplementedException();
    }

    private static ScrollPresenterTestHooks s_testHooks;

    private bool m_areAnchorNotificationsRaised;

    private bool m_areExpressionAnimationStatusNotificationsRaised;
    public static ScrollPresenterTestHooks GetGlobalTestHooks()
    {
        return s_testHooks;
    }

    public static void GetZoomFactorChangeVelocityParameters(out Int32 millisecondsPerUnit, out Int32 minMilliseconds, out Int32 maxMilliseconds)
    {
        throw new NotImplementedException();
    }

    public static void NotifyAnchorEvaluated(ScrollPresenter sender, UIElement anchorElement, double viewportAnchorPointHorizontalOffset, double viewportAnchorPointVerticalOffset){
        throw new NotImplementedException();
        }

    public static bool AreAnchorNotificationsRaised()
    {
        throw new NotImplementedException();
    }

    public static bool? IsAnimationsEnabledOverride()
    {
        throw new NotImplementedException();
    }


    public static bool AreExpressionAnimationStatusNotificationsRaised()
    {
        throw new NotImplementedException();
    }

    public static void NotifyContentLayoutOffsetXChanged(ScrollPresenter sender){
        throw new NotImplementedException();
    }


    public static void NotifyExpressionAnimationStatusChanged(ScrollPresenter sender, bool isExpressionAnimationStarted, string propertyName)
    {
        throw new NotImplementedException();
    }

    public static void NotifyContentLayoutOffsetYChanged(ScrollPresenter sender) {
        throw new NotImplementedException();
    }

    public static void NotifyInteractionSourcesChanged(ScrollPresenter sender, Windows.UI.Composition.Interactions.CompositionInteractionSourceCollection interactionSources)
    {
        throw new NotImplementedException();
    }

    public static bool AreInteractionSourcesNotificationsRaised()
    {
        throw new NotImplementedException();
    }
}
