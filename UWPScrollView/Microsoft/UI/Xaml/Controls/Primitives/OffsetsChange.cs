namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class OffsetsChange : ViewChange
{
    public OffsetsChange(
        double zoomedHorizontalOffset,
        double zoomedVerticalOffset,
        ScrollPresenterViewKind offsetsKind,
        object options) : base(offsetsKind, options)
    {
        ZoomedHorizontalOffset = zoomedHorizontalOffset;
        ZoomedVerticalOffset = zoomedVerticalOffset;
    }

    public double ZoomedHorizontalOffset { get; set; }

    public double ZoomedVerticalOffset { get; set; }
}