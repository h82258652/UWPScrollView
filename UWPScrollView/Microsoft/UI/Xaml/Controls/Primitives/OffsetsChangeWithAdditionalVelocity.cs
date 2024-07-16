using System.Numerics;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class OffsetsChangeWithAdditionalVelocity : ViewChangeBase
{
    private Vector2 m_anticipatedOffsetsChange;
    private Vector2? m_inertiaDecayRate;
    private Vector2 m_offsetsVelocity;

    public OffsetsChangeWithAdditionalVelocity(
        Vector2 offsetsVelocity,
        Vector2 anticipatedOffsetsChange,
        Vector2? inertiaDecayRate)
    {
        m_offsetsVelocity = offsetsVelocity;
        m_anticipatedOffsetsChange = anticipatedOffsetsChange;
        m_inertiaDecayRate = inertiaDecayRate;
    }

    public void AnticipatedOffsetsChange(Vector2 anticipatedOffsetsChange)
    {
        m_anticipatedOffsetsChange = anticipatedOffsetsChange;
    }

    public Vector2 AnticipatedOffsetsChange()
    {
        return m_anticipatedOffsetsChange;
    }

    public Vector2? InertiaDecayRate()
    {
        return m_inertiaDecayRate;
    }

    public void InertiaDecayRate(Vector2? inertiaDecayRate)
    {
        m_inertiaDecayRate = inertiaDecayRate;
    }

    public Vector2 OffsetsVelocity()
    {
        return m_offsetsVelocity;
    }

    public void OffsetsVelocity(Vector2 offsetsVelocity)
    {
        m_offsetsVelocity = offsetsVelocity;
    }
}