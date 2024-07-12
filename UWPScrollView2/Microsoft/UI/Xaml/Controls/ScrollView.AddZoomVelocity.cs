using System.Numerics;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public int AddZoomVelocity(float zoomFactorVelocity, Vector2? centerPoint, float? inertiaDecayRate)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.AddZoomVelocity(zoomFactorVelocity, centerPoint, inertiaDecayRate);
        }

        return -1;
    }
}