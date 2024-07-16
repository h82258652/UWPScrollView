namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents a single point that zoomed content can snap to.
/// </summary>
public class ZoomSnapPoint : ZoomSnapPointBase
{
    private double m_value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZoomSnapPoint"/> class with the specified value.
    /// </summary>
    /// <param name="snapPointValue"></param>
    public ZoomSnapPoint(double snapPointValue)
    {
        m_value = snapPointValue;
    }

    /// <summary>
    /// Gets the location of the snap point relative to the origin of the viewport.
    /// </summary>
    public double Value
    {
        get
        {
            return m_value;
        }
    }
}