using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace Microsoft.UI.Xaml.Controls;

public partial class ScrollView
{
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _hasNoIndicatorStateStoryboardCompletedHandler = false;
        _keepIndicatorsShowing = false;

        ScrollPresenter scrollPresenter = (ScrollPresenter)GetTemplateChild(_scrollPresenterPartName);

        UpdateScrollPresenter(scrollPresenter);

        UIElement horizontalScrollControllerElement = (UIElement)GetTemplateChild(_horizontalScrollBarPartName);
        IScrollController? horizontalScrollController = horizontalScrollControllerElement as IScrollController;
        ScrollBar? horizontalScrollBar = null;

        if (horizontalScrollControllerElement is not null && horizontalScrollController is null)
        {
            horizontalScrollBar = horizontalScrollControllerElement as ScrollBar;

            if (horizontalScrollBar is not null)
            {
                if (_horizontalScrollBarController is null)
                {
                    _horizontalScrollBarController = new ScrollBarController();
                }

                horizontalScrollController = _horizontalScrollBarController as IScrollController;
            }
        }

        if (horizontalScrollBar is not null)
        {
            _horizontalScrollBarController.SetScrollBar(horizontalScrollBar);
        }
        else
        {
            _horizontalScrollBarController = null;
        }

        UpdateHorizontalScrollController(horizontalScrollController, horizontalScrollControllerElement);

        UIElement verticalScrollControllerElement = (UIElement)GetTemplateChild(_verticalScrollBarPartName);
        IScrollController? verticalScrollController = verticalScrollControllerElement as IScrollController;
        ScrollBar? verticalScrollBar = null;

        if (verticalScrollControllerElement is not null && verticalScrollController is null)
        {
            verticalScrollBar = verticalScrollControllerElement as ScrollBar;

            if (verticalScrollBar is not null)
            {
                if (_verticalScrollBarController is null)
                {
                    _verticalScrollBarController = new ScrollBarController();
                }

                verticalScrollController = _verticalScrollBarController as IScrollController;
            }
        }

        if (verticalScrollBar is not null)
        {
            _verticalScrollBarController.SetScrollBar(verticalScrollBar);
        }
        else
        {
            _verticalScrollBarController = null;
        }

        UpdateVerticalScrollController(verticalScrollController, verticalScrollControllerElement);

        UIElement scrollControllersSeparator = (UIElement)GetTemplateChild(_scrollBarsSeparatorPartName);

        UpdateScrollControllersSeparator(scrollControllersSeparator);

        UpdateScrollControllersVisibility(true, true);

        FrameworkElement root = (FrameworkElement)GetTemplateChild(_rootPartName);

        if (root is not null)
        {
            var rootVisualStateGroups = VisualStateManager.GetVisualStateGroups(root);

            if (rootVisualStateGroups is not null)
            {
                var groupCount = rootVisualStateGroups.Count;

                for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
                {
                    VisualStateGroup group = rootVisualStateGroups[groupIndex];

                    if (group is not null)
                    {
                        var visualStates = group.States;

                        if (visualStates is not null)
                        {
                            var stateCount = visualStates.Count;

                            for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
                            {
                                VisualState state = visualStates[stateIndex];

                                if (state is not null)
                                {
                                    var stateName = state.Name;
                                    Storyboard stateStoryboard = state.Storyboard;

                                    if (stateStoryboard is not null)
                                    {
                                        if (stateName == _noIndicatorStateName)
                                        {
                                            stateStoryboard.Completed += OnNoIndicatorStateStoryboardCompleted;
                                            _hasNoIndicatorStateStoryboardCompletedHandler = true;
                                        }
                                        else if (stateName == _touchIndicatorStateName || stateName == _mouseIndicatorStateName)
                                        {
                                            stateStoryboard.Completed += OnIndicatorStateStoryboardCompleted;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        UpdateVisualStates(false);
    }
}