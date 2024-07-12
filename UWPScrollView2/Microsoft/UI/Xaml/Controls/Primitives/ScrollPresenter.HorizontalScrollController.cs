using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public IScrollController? HorizontalScrollController
    {
        get
        {
            if (m_horizontalScrollController is not null)
            {
                return m_horizontalScrollController;
            }

            return null;
        }
        set
        {
            throw new NotImplementedException();
        }
    }
}