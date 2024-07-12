namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    private double GetZoomedExtentHeight()
    {
        return _unzoomedExtentHeight * _zoomFactor;
    }
}