using Windows.Foundation;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class BringIntoViewOffsetsChange : OffsetsChange
{
    private UIElement m_element;
    private Rect m_elementRect;
    private double m_horizontalAlignmentRatio;
    private double m_horizontalOffset;
    private object m_owner;
    private double m_verticalAlignmentRatio;
    private double m_verticalOffset;

    public BringIntoViewOffsetsChange(
        object owner,
        double zoomedHorizontalOffset,
        double zoomedVerticalOffset,
        ScrollPresenterViewKind offsetsKind,
        object options,
        UIElement element,
        Rect elementRect,
        double horizontalAlignmentRatio,
        double verticalAlignmentRatio,
        double horizontalOffset,
        double verticalOffset) : base(zoomedHorizontalOffset, zoomedVerticalOffset, offsetsKind, options)
    {
        m_owner = owner;
        m_elementRect = elementRect;
        m_horizontalAlignmentRatio = horizontalAlignmentRatio;
        m_verticalAlignmentRatio = verticalAlignmentRatio;
        m_horizontalOffset = horizontalOffset;
        m_verticalOffset = verticalOffset;
        m_element = element;
    }

    public UIElement Element()
    {
        return m_element;
    }

    public Rect ElementRect()
    {
        return m_elementRect;
    }

    public double HorizontalAlignmentRatio()
    {
        return m_horizontalAlignmentRatio;
    }

    public double HorizontalOffset()
    {
        return m_horizontalOffset;
    }

    public double VerticalAlignmentRatio()
    {
        return m_verticalAlignmentRatio;
    }

    public double VerticalOffset()
    {
        return m_verticalOffset;
    }
}