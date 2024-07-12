namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Defines constants that specify how scroll- and zoom-chaining is handled by the <see cref="ScrollView"/> control.
/// </summary>
public enum ScrollingChainMode
{
    /// <summary>
    /// The <see cref="ScrollView"/> chains to the closest scrollable/zoomable outer component, if any.
    /// </summary>
    Auto,

    /// <summary>
    /// The <see cref="ScrollView"/> acts as if chaining occurred even when no scrollable/zoomable outer component is present.
    /// </summary>
    Always,

    /// <summary>
    /// The <see cref="ScrollView"/> ignores any scrollable/zoomable outer component that is present and no chaining occurs.
    /// </summary>
    Never
}