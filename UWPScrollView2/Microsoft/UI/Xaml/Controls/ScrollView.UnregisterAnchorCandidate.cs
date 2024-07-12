using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    public void UnregisterAnchorCandidate(UIElement element)
    {
        if (_scrollPresenter is { } scrollPresenter)
        {
            if (scrollPresenter is IScrollAnchorProvider scrollPresenterAsAnchorProvider)
            {
                scrollPresenterAsAnchorProvider.UnregisterAnchorCandidate(element);
                return;
            }

            throw new InvalidOperationException("Template part named PART_ScrollPresenter does not implement IScrollAnchorProvider.");
        }

        throw new InvalidOperationException("No template part named PART_ScrollPresenter was loaded.");
    }
}