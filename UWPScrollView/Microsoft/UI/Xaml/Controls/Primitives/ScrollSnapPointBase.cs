using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents the base class for scrolling snap points used by a <see cref="ScrollPresenter"/> or other scrolling control.
/// </summary>
public class ScrollSnapPointBase
{
    private ScrollSnapPointBase()
    {
    }

    /// <summary>
    /// Gets a value that indicates where the snap point is located in relation to the viewport.
    /// </summary>
    public ScrollSnapPointsAlignment Alignment
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}