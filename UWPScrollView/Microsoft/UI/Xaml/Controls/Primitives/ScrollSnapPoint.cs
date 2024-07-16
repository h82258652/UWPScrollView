using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents a single point that scrolled content can snap to.
/// </summary>
public class ScrollSnapPoint : ScrollSnapPointBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollSnapPoint"/> class with the specified value and alignment.
    /// </summary>
    /// <param name="snapPointValue">The location of the snap point relative to the origin of the viewport.</param>
    /// <param name="alignment">A value that indicates where the snap point is located in relation to the viewport.</param>
    public ScrollSnapPoint(double snapPointValue, ScrollSnapPointsAlignment alignment)
    {
        m_value = snapPointValue;
        m_alignment = alignment;
    }

    private double m_value;

    /// <summary>
    /// Gets the location of the snap point relative to the origin of the viewport.
    /// </summary>
    public double Value
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}