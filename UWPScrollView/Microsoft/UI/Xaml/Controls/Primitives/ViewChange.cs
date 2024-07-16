namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class ViewChange : ViewChangeBase
{
    /// <summary>
    /// ScrollingScrollOptions or ScrollingZoomOptions instance associated with this view change.
    /// </summary>
    private object m_options;

    private ScrollPresenterViewKind m_viewKind = ScrollPresenterViewKind.Absolute;

    public ViewChange(
        ScrollPresenterViewKind viewKind,
        object options)
    {
        m_viewKind = viewKind;
        m_options = options;
    }

    public object Options()
    {
        return m_options;
    }

    public ScrollPresenterViewKind ViewKind()
    {
        return m_viewKind;
    }
}