using System.Numerics;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public int AddScrollVelocity(Vector2 offsetsVelocity, Vector2? inertiaDecayRate)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.AddScrollVelocity(offsetsVelocity, inertiaDecayRate);
        }

        return -1;
    }
}