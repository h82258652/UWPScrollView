using System;

namespace Microsoft.UI.Xaml.Controls.Primitives;

/// <summary>
/// Represents multiple equidistant points that zoomed content can snap to.
/// </summary>
public class RepeatedZoomSnapPoint : ZoomSnapPointBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepeatedZoomSnapPoint"/> class with the specified values.
    /// </summary>
    /// <param name="offset">The distance by which points are shifted from 0.</param>
    /// <param name="interval">The distance between two successive points.</param>
    /// <param name="start">The beginning of the range in which the snap points are effective.</param>
    /// <param name="end">The end of the range in which the snap points are effective.</param>
    public RepeatedZoomSnapPoint(double offset, double interval, double start, double end)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the end of the range in which the snap points are effective.
    /// </summary>
    public double End
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the distance between two successive points.
    /// </summary>
    public double Interval
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the distance by which points are shifted from 0.
    /// </summary>
    public double Offset
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the beginning of the range in which the snap points are effective.
    /// </summary>
    public double Start
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}