using System;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.BringingIntoView"/> event.
/// </summary>
public sealed class ScrollingBringingIntoViewEventArgs
{
    private ScrollingBringingIntoViewEventArgs()
    {
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not the participation in the bring-into-view request must be cancelled.
    /// </summary>
    public bool Cancel
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the correlation ID for the imminent scroll offset change participation.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the <see cref="BringIntoViewRequestedEventArgs"/> argument from the <see cref="FrameworkElement.RequestBringIntoView"/> event that is being processed.
    /// </summary>
    public BringIntoViewRequestedEventArgs RequestEventArgs
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets or sets the snap points mode used during the <see cref="ScrollView"/>'s participation in the bring-into-view request.
    /// </summary>
    public ScrollingSnapPointsMode SnapPointsMode
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the target <see cref="ScrollView.HorizontalOffset"/> for the default participation.
    /// </summary>
    public double TargetHorizontalOffset
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets the target <see cref="ScrollView.VerticalOffset"/> for the default participation.
    /// </summary>
    public double TargetVerticalOffset
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}