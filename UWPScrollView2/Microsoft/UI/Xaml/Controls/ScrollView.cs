using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView : Control
{
    private FocusInputDeviceKind m_focusInputDeviceKind = FocusInputDeviceKind.None;

    private List<ScrollViewBringIntoViewOperation> m_bringIntoViewOperations;

    private bool m_hasNoIndicatorStateStoryboardCompletedHandler = false;

    private UIElement m_scrollControllersSeparatorElement;

    private UIElement m_horizontalScrollControllerElement;

    private UIElement m_verticalScrollControllerElement;

    private AutoHideScrollBarsState m_autoHideScrollBarsState;

    public ScrollView()
    {
        m_autoHideScrollBarsState = MakeAutoHideScrollBarsState();
        DefaultStyleKey = typeof(ScrollView);
    }
    private DispatcherQueue m_dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private int m_verticalAddScrollVelocityOffsetChangeCorrelationId = -1;

    private int m_horizontalAddScrollVelocityOffsetChangeCorrelationId = -1;

    private int m_verticalAddScrollVelocityDirection = 0;

    private int m_horizontalAddScrollVelocityDirection = 0;

    private IScrollController m_horizontalScrollController;

    private IScrollController m_verticalScrollController;

    private bool m_showingMouseIndicators = false;

    private const string _horizontalScrollBarPartName = "PART_HorizontalScrollBar";
    private const string _mouseIndicatorStateName = "MouseIndicator";
    private const string _noIndicatorStateName = "NoIndicator";
    private const string _rootPartName = "PART_Root";
    private const string _scrollBarsSeparatorPartName = "PART_ScrollBarsSeparator";
    private const string _scrollPresenterPartName = "PART_ScrollPresenter";
    private const string _touchIndicatorStateName = "TouchIndicator";
    private const string _verticalScrollBarPartName = "PART_VerticalScrollBar";
    private bool _hasNoIndicatorStateStoryboardCompletedHandler = false;
    private ScrollBarController? _horizontalScrollBarController = null;
    private bool _isPointerOverHorizontalScrollController = false;
    private bool _isPointerOverVerticalScrollController = false;
    private bool _keepIndicatorsShowing = false;
    private ScrollPresenter? _scrollPresenter;
    private ScrollBarController? _verticalScrollBarController = null;

    private const long s_noIndicatorCountdown = 2000 * 10000;

    private bool m_keepIndicatorsShowing = false;
    private bool m_autoHideScrollControllers = false;
    private bool m_autoHideScrollControllersValid = false;

    private DispatcherTimer? m_hideIndicatorsTimer;
    private bool m_isLeftMouseButtonPressedForFocus = false;
    private bool m_preferMouseIndicators = false;

    private AutoHideScrollBarsState MakeAutoHideScrollBarsState()
    {
        return new AutoHideScrollBarsState();
    }

    internal struct AutoHideScrollBarsState
    {
        internal UISettings? m_uiSettings5;

        public void HookUISettingsEvent(ScrollView scrollView)
        {
            if (m_uiSettings5 is null)
            {
                if (ApiInformation.IsReadOnlyPropertyPresent("Windows.UI.ViewManagement.UISettings", "AutoHideScrollBars"))
                {
                    m_uiSettings5 = new UISettings();
                }

                if (m_uiSettings5 is not null)
                {
                    m_uiSettings5.AutoHideScrollBarsChanged += scrollView.OnAutoHideScrollBarsChanged;
                }
            }
        }
    }
}