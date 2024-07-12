using Windows.UI.Composition;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public CompositionPropertySet? ExpressionAnimationSources
    {
        get
        {
            if (_scrollPresenter is not null)
            {
                return _scrollPresenter.ExpressionAnimationSources;
            }

            return null;
        }
    }
}