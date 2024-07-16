using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Provides data for the <see cref="ScrollView.AnchorRequested"/> event.
/// </summary>
public sealed class ScrollingAnchorRequestedEventArgs
{
    private ScrollingAnchorRequestedEventArgs()
    {
    }

    /// <summary>
    /// Gets the collection of anchor element candidates to pick from.
    /// </summary>
    public IList<UIElement> AnchorCandidates
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Gets or sets the selected anchor element.
    /// </summary>
    public UIElement AnchorElement
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
}