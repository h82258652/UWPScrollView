using System.Numerics;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class ZoomFactorChangeWithAdditionalVelocity : ViewChangeBase
{
    private float m_anticipatedZoomFactorChange;
    private Vector2? m_centerPoint;
    private float? m_inertiaDecayRate;
    private float m_zoomFactorVelocity;

    public ZoomFactorChangeWithAdditionalVelocity(
        float zoomFactorVelocity,
        float anticipatedZoomFactorChange,
        Vector2? centerPoint,
        float? inertiaDecayRate)
    {
        m_zoomFactorVelocity = zoomFactorVelocity;
        m_anticipatedZoomFactorChange = anticipatedZoomFactorChange;
        m_centerPoint = centerPoint;
        m_inertiaDecayRate = inertiaDecayRate;
    }

    public float AnticipatedZoomFactorChange()
    {
        return m_anticipatedZoomFactorChange;
    }

    public void AnticipatedZoomFactorChange(float anticipatedZoomFactorChange)
    {
        m_anticipatedZoomFactorChange = anticipatedZoomFactorChange;
    }

    public Vector2? CenterPoint()
    {
        return m_centerPoint;
    }

    public float? InertiaDecayRate()
    {
        return m_inertiaDecayRate;
    }

    public float ZoomFactorVelocity()
    {
        return m_zoomFactorVelocity;
    }

    public void ZoomFactorVelocity(float zoomFactorVelocity)
    {
        m_zoomFactorVelocity = zoomFactorVelocity;
    }
}