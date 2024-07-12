using System.Numerics;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint)
    {
        return ZoomBy(zoomFactorDelta, centerPoint, null);
    }

    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint, ScrollingZoomOptions? options)
    {
        int viewChangeCorrelationId;
        ChangeZoomFactorPrivate(
            zoomFactorDelta,
            centerPoint,
            ScrollPresenterViewKind.RelativeToCurrentView,
            options,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }
}