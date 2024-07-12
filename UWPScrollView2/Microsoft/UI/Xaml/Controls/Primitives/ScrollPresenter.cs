using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter : FrameworkElement
{
    private ScrollingInteractionState _state = ScrollingInteractionState.Idle;
    private double _unzoomedExtentHeight;
    private double _unzoomedExtentWidth;
    private double _viewportHeight;
    private double _viewportWidth;
    private double _zoomedHorizontalOffset;
    private double _zoomedVerticalOffset;
    private float _zoomFactor = 1f;

  private  List<InteractionTrackerAsyncOperation> m_interactionTrackerAsyncOperations;

    private IScrollController m_horizontalScrollController;

    private IScrollController m_verticalScrollController;

    private float m_animationRestartZoomFactor = 1f;
    private Vector2 m_endOfInertiaPosition;
    private CompositionPropertySet m_expressionAnimationSources;
    private CompositionPropertySet m_horizontalScrollControllerExpressionAnimationSources;
    private bool m_horizontalSnapPointsNeedViewportUpdates;

    private InteractionTracker? m_interactionTracker = null;
    private IInteractionTrackerOwner? m_interactionTrackerOwner;
    private ExpressionAnimation m_maxPositionExpressionAnimation;
    private ExpressionAnimation m_minPositionExpressionAnimation;
    private ExpressionAnimation m_positionSourceExpressionAnimation;
    private CompositionPropertySet m_verticalScrollControllerExpressionAnimationSources;
    private bool m_verticalSnapPointsNeedViewportUpdates;
    private ExpressionAnimation m_zoomFactorSourceExpressionAnimation;
}