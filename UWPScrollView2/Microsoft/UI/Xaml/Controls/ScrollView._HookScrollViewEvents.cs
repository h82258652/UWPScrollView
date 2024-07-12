using System;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    private void HookScrollViewEvents()
    {
        GettingFocus += OnScrollViewGettingFocus;
        IsEnabledChanged += OnScrollViewIsEnabledChanged;
        Unloaded += OnScrollViewUnloaded;

        throw new NotImplementedException();

        AddHandler(PointerEnteredEvent, OnScrollViewPointerEntered, false);

        AddHandler(PointerMovedEvent, OnScrollViewPointerMoved, false);

        AddHandler(PointerExitedEvent, OnScrollViewPointerExited, false);

        AddHandler(PointerPressedEvent, OnScrollViewPointerPressed, false);

        AddHandler(PointerReleasedEvent, OnScrollViewPointerReleased, true);

        AddHandler(PointerCanceledEvent, OnScrollViewPointerCanceled, true);
    }
}