using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    private string GetMaxPositionYExpression(UIElement content)
    {
        FrameworkElement? contentAsFE = content as FrameworkElement;

        if (contentAsFE is not null)
        {
            string maxOffset = "(contentSizeY * it.Scale - scrollPresenterVisual.Size.Y)";

            if (contentAsFE.VerticalAlignment == VerticalAlignment.Center ||
                contentAsFE.VerticalAlignment == VerticalAlignment.Stretch)
            {
                return string.Format("{0} >= 0 ? {0} + contentLayoutOffsetY : {0} / 2.0f + contentLayoutOffsetY", maxOffset);
            }
            else if (contentAsFE.VerticalAlignment == VerticalAlignment.Bottom)
            {
                return string.Format("{0} + contentLayoutOffsetY", maxOffset);
            }
        }

        return "Max(0.0f, contentSizeY * it.Scale - scrollPresenterVisual.Size.Y) + contentLayoutOffsetY";
    }
}