using System;
using System.Diagnostics;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class InteractionTrackerAsyncOperation
{
    /// <summary>
    /// Used as a workaround for InteractionTracker bug "12465209 - InteractionTracker remains silent when calling TryUpdatePosition with the current position":
    /// Maximum number of UI thread ticks processed while waiting for non-animated operations to complete.
    /// </summary>
    private const int c_maxNonAnimatedOperationTicks = 10;

    /// <summary>
    /// Number of UI thread ticks elapsed before a queued operation gets processed to allow any pending size
    /// changes to be propagated to the InteractionTracker.
    /// </summary>
    private const int c_queuedOperationTicks = 3;

    /// <summary>
    /// Identifies the InteractionTracker request type for this operation.
    /// </summary>
    private readonly InteractionTrackerAsyncOperationType m_operationType = InteractionTrackerAsyncOperationType.None;

    /// <summary>
    /// Set to True when the operation was canceled early enough to take effect.
    /// </summary>
    private bool m_isCanceled;

    /// <summary>
    /// Set to True when the operation completed and was assigned a final ScrollPresenterViewChangeResult result.
    /// </summary>
    private bool m_isCompleted;

    /// <summary>
    /// Set to True when the operation is delayed until the scrollPresenter is loaded.
    /// </summary>
    private bool m_isDelayed;

    /// <summary>
    /// Identifies the InteractionTracker trigger type for this operation.
    /// </summary>
    private InteractionTrackerAsyncOperationTrigger m_operationTrigger = InteractionTrackerAsyncOperationTrigger.DirectViewChange;

    /// <summary>
    /// Number of UI thread ticks remaining before a non-animated InteractionTracker request is declared completed
    /// in case no ValuesChanged or status change notification is raised.
    /// </summary>
    private int m_postProcessingTicksCountdown;

    /// <summary>
    /// Number of UI thread ticks remaining before this queued operation gets processed.
    /// Positive between the time the operation is queued in ScrollPresenter::ScrollTo/By/From, ScrollPresenter::ZoomTo/By/From or
    /// ScrollPresenter::OnCompositionTargetRendering and the time it is processed in ScrollPresenter::ProcessOffsetsChange or ScrollPresenter::ProcessZoomFactorChange.
    /// </summary>
    private int m_preProcessingTicksCountdown = c_queuedOperationTicks;

    /// <summary>
    /// Initial value of m_preProcessingTicksCountdown when this operation is queued up.
    /// </summary>
    private int m_queuedOperationTicks = c_queuedOperationTicks;

    /// <summary>
    /// InteractionTracker RequestId associated with this operation.
    /// </summary>
    private int m_requestId;

    /// <summary>
    /// Null by default and optionally set to a prior operation that needs to complete before this one can start.
    /// </summary>
    private InteractionTrackerAsyncOperation? m_requiredOperation;

    /// <summary>
    /// OffsetsChange or ZoomFactorChange instance associated with this operation.
    /// </summary>
    private ViewChangeBase m_viewChangeBase;

    /// <summary>
    /// ViewChangeCorrelationId associated with this operation.
    /// </summary>
    private int m_viewChangeCorrelationId = -1;

    public InteractionTrackerAsyncOperation(
        InteractionTrackerAsyncOperationType operationType,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        bool isDelayed,
        ViewChangeBase viewChangeBase)
    {
        m_operationType = operationType;
        m_operationTrigger = operationTrigger;
        m_isDelayed = isDelayed;
        m_viewChangeBase = viewChangeBase;

        if (!IsAnimated())
        {
            m_postProcessingTicksCountdown = c_maxNonAnimatedOperationTicks;
        }
    }

    public InteractionTrackerAsyncOperationTrigger GetOperationTrigger()
    {
        return m_operationTrigger;
    }

    public InteractionTrackerAsyncOperationType GetOperationType()
    {
        return m_operationType;
    }

    public int GetRequestId()
    {
        return m_requestId;
    }

    public InteractionTrackerAsyncOperation GetRequiredOperation()
    {
        return m_requiredOperation;
    }

    public int GetTicksCountdown()
    {
        return m_preProcessingTicksCountdown;
    }

    public ViewChangeBase GetViewChangeBase()
    {
        return m_viewChangeBase;
    }

    public int GetViewChangeCorrelationId()
    {
        return m_viewChangeCorrelationId;
    }

    public bool IsAnimated()
    {
        switch (m_operationType)
        {
            case InteractionTrackerAsyncOperationType.TryUpdatePosition:
            case InteractionTrackerAsyncOperationType.TryUpdatePositionBy:
            case InteractionTrackerAsyncOperationType.TryUpdateScale:
                return false;
        }
        return true;
    }

    public bool IsCanceled()
    {
        return m_isCanceled;
    }

    public bool IsCompleted()
    {
        return m_isCompleted;
    }

    public bool IsDelayed()
    {
        return m_isDelayed;
    }

    /// <summary>
    /// Returns True when the operation fulfills a horizontal IScrollController request.
    /// </summary>
    /// <returns></returns>
    public bool IsHorizontalScrollControllerRequest()
    {
        return ((int)(m_operationTrigger) & (int)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest)) > 0;
    }

    public bool IsQueued()
    {
        return m_preProcessingTicksCountdown > 0;
    }

    public bool IsUnqueueing()
    {
        return m_preProcessingTicksCountdown > 0 && m_preProcessingTicksCountdown < m_queuedOperationTicks;
    }

    /// <summary>
    /// Returns True when the operation fulfills a vertical IScrollController request.
    /// </summary>
    /// <returns></returns>
    public bool IsVerticalScrollControllerRequest()
    {
        return ((int)(m_operationTrigger) & (int)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest)) > 0;
    }

    public void SetIsCanceled(bool isCanceled)
    {
        m_isCanceled = isCanceled;
    }

    public void SetIsCompleted(bool isCompleted)
    {
        m_isCompleted = isCompleted;
    }

    public void SetIsDelayed(bool isDelayed)
    {
        m_isDelayed = isDelayed;
    }

    public void SetIsScrollControllerRequest(bool isFromHorizontalScrollController)
    {
        if (isFromHorizontalScrollController)
            m_operationTrigger = (InteractionTrackerAsyncOperationTrigger)((int)(m_operationTrigger) |
                                 (int)(InteractionTrackerAsyncOperationTrigger.HorizontalScrollControllerRequest));
        else
            m_operationTrigger = (InteractionTrackerAsyncOperationTrigger)((int)(m_operationTrigger) |
                                 (int)(InteractionTrackerAsyncOperationTrigger.VerticalScrollControllerRequest));
    }

    /// <summary>
    /// Sets the ticks countdown to the max value of c_queuedOperationTicks == 3. This is invoked for queued operations
    /// when the extent or viewport size changed in order to let it propagate to the Composition thread and thus let the
    /// InteractionTracker operate on the latest sizes.
    /// </summary>
    public void SetMaxTicksCountdown()
    {
        int ticksCountdownIncrement = Math.Max(0, c_queuedOperationTicks - m_preProcessingTicksCountdown);

        if (ticksCountdownIncrement > 0)
        {
            m_preProcessingTicksCountdown += ticksCountdownIncrement;
            m_queuedOperationTicks += ticksCountdownIncrement;
        }
    }

    public void SetRequestId(int requestId)
    {
        m_requestId = requestId;
    }

    public void SetRequiredOperation(InteractionTrackerAsyncOperation requiredOperation)
    {
        m_requiredOperation = requiredOperation;
    }

    public void SetTicksCountdown(int ticksCountdown)
    {
        Debug.Assert(ticksCountdown > 0);
        m_preProcessingTicksCountdown = m_queuedOperationTicks = ticksCountdown;
    }

    public void SetViewChangeCorrelationId(int viewChangeCorrelationId)
    {
        if (m_viewChangeCorrelationId != viewChangeCorrelationId)
        {
            m_viewChangeCorrelationId = viewChangeCorrelationId;
        }
    }

    /// <summary>
    /// Returns True when the post-processing ticks count has reached 0
    /// </summary>
    /// <returns></returns>
    public bool TickNonAnimatedOperation()
    {
        Debug.Assert(!IsAnimated());
        Debug.Assert(m_postProcessingTicksCountdown > 0);

        m_postProcessingTicksCountdown--;

        return m_postProcessingTicksCountdown == 0;
    }

    /// <summary>
    /// Returns True when the pre-processing ticks count has reached 0
    /// </summary>
    /// <returns></returns>
    public bool TickQueuedOperation()
    {
        Debug.Assert(m_preProcessingTicksCountdown > 0);

        m_preProcessingTicksCountdown--;

        return m_preProcessingTicksCountdown == 0;
    }
}