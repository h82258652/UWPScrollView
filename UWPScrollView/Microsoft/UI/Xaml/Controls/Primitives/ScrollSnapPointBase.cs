namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents the base class for scrolling snap points used by a <see cref="ScrollPresenter"/> or other scrolling control.
/// </summary>
public class ScrollSnapPointBase : SnapPointBase
{
    protected ScrollSnapPointsAlignment m_alignment = ScrollSnapPointsAlignment.Near;

    protected ScrollSnapPointBase()
    {
    }

    /// <summary>
    /// Gets a value that indicates where the snap point is located in relation to the viewport.
    /// </summary>
    public ScrollSnapPointsAlignment Alignment
    {
        get
        {
            return m_alignment;
        }
    }
}