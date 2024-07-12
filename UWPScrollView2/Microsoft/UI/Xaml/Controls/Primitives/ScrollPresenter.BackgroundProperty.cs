using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
    public partial class ScrollPresenter
    {
        public static DependencyProperty BackgroundProperty = DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(ScrollPresenter), new PropertyMetadata(null, (sender, args) =>
        {
            ((ScrollPresenter)sender).OnPropertyChanged(args);
        }));
    }
}