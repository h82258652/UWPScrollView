namespace Microsoft.UI.Xaml.Controls;

internal partial class ScrollBarController
{
    private void RaiseCanScrollChanged()
    {
        if (m_canScrollChanged is null)
        {
            return;
        }

        m_canScrollChanged(this, null);
    }
}