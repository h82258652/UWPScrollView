using System;
using System.Numerics;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ZoomBy(zoomFactorDelta, centerPoint);
        }

        return -1;
    }

    public int ZoomBy(float zoomFactorDelta, Vector2? centerPoint, ScrollingZoomOptions? options)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ZoomBy(zoomFactorDelta, centerPoint, options);
        }

        return -1;
    }
}