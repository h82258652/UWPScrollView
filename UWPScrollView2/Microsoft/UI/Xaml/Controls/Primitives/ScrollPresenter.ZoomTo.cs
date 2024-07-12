using System.Numerics;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public int ZoomTo(float zoomFactor, Vector2? centerPoint)
    {
        return ZoomTo(zoomFactor, centerPoint, null);
    }

    public int ZoomTo(float zoomFactor, Vector2? centerPoint, ScrollingZoomOptions? options)
    {
        int viewChangeCorrelationId;
        ChangeZoomFactorPrivate(
            zoomFactor,
            centerPoint,
            ScrollPresenterViewKind.Absolute,
            options,
            out viewChangeCorrelationId);

        return viewChangeCorrelationId;
    }
}