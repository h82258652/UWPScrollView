using System.Numerics;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public int ZoomTo(float zoomFactor, Vector2? centerPoint)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ZoomTo(zoomFactor, centerPoint);
        }

        return -1;
    }

    public int ZoomTo(float zoomFactor, Vector2? centerPoint, ScrollingZoomOptions? options)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ZoomTo(zoomFactor, centerPoint, options);
        }

        return -1;
    }
}