namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    private double GetZoomedExtentWidth()
    {
        return _unzoomedExtentWidth * _zoomFactor;
    }
}