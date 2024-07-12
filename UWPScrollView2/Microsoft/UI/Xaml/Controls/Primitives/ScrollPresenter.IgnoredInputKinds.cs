using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    public ScrollingInputKinds IgnoredInputKinds
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            SetValue(IgnoredInputKindsProperty, value);
        }
    }
}