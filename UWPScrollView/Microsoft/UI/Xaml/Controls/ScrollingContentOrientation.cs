namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Defines constants that specify the orientation of content scrolling in a <see cref="ScrollView"/>.
/// </summary>
public enum ScrollingContentOrientation
{
    /// <summary>
    /// Content can scroll vertically.
    /// </summary>
    Vertical,

    /// <summary>
    /// Content can scroll horizontally.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Content cannot scroll either horizontally or vertically.
    /// </summary>
    None,

    /// <summary>
    /// Content can scroll both horizontally and vertically.
    /// </summary>
    Both
}