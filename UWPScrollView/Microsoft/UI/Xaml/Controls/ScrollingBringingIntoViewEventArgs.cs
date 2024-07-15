using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.BringingIntoView"/> event.
/// </summary>
public sealed class ScrollingBringingIntoViewEventArgs
{
    private bool m_cancel;

    private int m_offsetsChangeCorrelationId = -1;

    private BringIntoViewRequestedEventArgs m_requestEventArgs;

    private ScrollingSnapPointsMode m_snapPointsMode = ScrollingSnapPointsMode.Ignore;

    private double m_targetHorizontalOffset;

    private double m_targetVerticalOffset;

    internal ScrollingBringingIntoViewEventArgs()
    {
    }

    /// <summary>
    /// Gets or sets a value that indicates whether or not the participation in the bring-into-view request must be cancelled.
    /// </summary>
    public bool Cancel
    {
        get
        {
            return m_cancel;
        }
        set
        {
            m_cancel = value;
        }
    }

    /// <summary>
    /// Gets the correlation ID for the imminent scroll offset change participation.
    /// </summary>
    public int CorrelationId
    {
        get
        {
            return m_offsetsChangeCorrelationId;
        }
    }

    /// <summary>
    /// Gets the <see cref="BringIntoViewRequestedEventArgs"/> argument from the <see cref="FrameworkElement.RequestBringIntoView"/> event that is being processed.
    /// </summary>
    public BringIntoViewRequestedEventArgs RequestEventArgs
    {
        get
        {
            return m_requestEventArgs;
        }
        internal set
        {
            m_requestEventArgs = value;
        }
    }

    /// <summary>
    /// Gets or sets the snap points mode used during the <see cref="ScrollView"/>'s participation in the bring-into-view request.
    /// </summary>
    public ScrollingSnapPointsMode SnapPointsMode
    {
        get
        {
            return m_snapPointsMode;
        }
        set
        {
            m_snapPointsMode = value;
        }
    }

    /// <summary>
    /// Gets the target <see cref="ScrollView.HorizontalOffset"/> for the default participation.
    /// </summary>
    public double TargetHorizontalOffset
    {
        get
        {
            return m_targetHorizontalOffset;
        }
    }

    /// <summary>
    /// Gets the target <see cref="ScrollView.VerticalOffset"/> for the default participation.
    /// </summary>
    public double TargetVerticalOffset
    {
        get
        {
            return m_targetVerticalOffset;
        }
    }

    internal void OffsetsChangeCorrelationId(int offsetsChangeCorrelationId)
    {
        m_offsetsChangeCorrelationId = offsetsChangeCorrelationId;
    }

    internal void TargetOffsets(double targetHorizontalOffset, double targetVerticalOffset)
    {
        m_targetHorizontalOffset = targetHorizontalOffset;
        m_targetVerticalOffset = targetVerticalOffset;
    }
}