using System.Numerics;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class ZoomFactorChange : ViewChange
{
    public ZoomFactorChange(
        float zoomFactor,
        Vector2? centerPoint,
        ScrollPresenterViewKind zoomFactorKind,
        object options) : base(zoomFactorKind, options)
    {
        ZoomFactor = zoomFactor;
        CenterPoint = centerPoint;
    }

    public Vector2? CenterPoint { get; }

    public float ZoomFactor { get; }
}