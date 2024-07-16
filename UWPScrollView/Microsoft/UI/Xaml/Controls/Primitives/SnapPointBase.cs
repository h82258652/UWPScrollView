namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents the base class for scrolling and zooming snap points used by a <see cref="ScrollPresenter"/> or other scrolling control.
/// </summary>
public class SnapPointBase
{
    internal SnapPointBase()
    {
    }

    public virtual int SnapCount()
    {
        return 0;
    }

    public virtual bool OnUpdateViewport(double newViewport)
    {
        return false;
    }
}