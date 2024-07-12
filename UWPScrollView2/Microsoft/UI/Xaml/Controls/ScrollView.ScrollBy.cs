using System;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ScrollBy(horizontalOffsetDelta, verticalOffsetDelta);
        }

        return -1;
    }

    public int ScrollBy(double horizontalOffsetDelta, double verticalOffsetDelta, ScrollingScrollOptions? options)
    {
        if (_scrollPresenter is not null)
        {
            return _scrollPresenter.ScrollBy(horizontalOffsetDelta, verticalOffsetDelta, options);
        }

        return -1;
    }
}