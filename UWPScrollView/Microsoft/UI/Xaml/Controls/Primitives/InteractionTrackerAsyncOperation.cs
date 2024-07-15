using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class InteractionTrackerAsyncOperation
{
    public bool IsDelayed()
    {
        throw new NotImplementedException();
    }

    public bool IsAnimated()
    {
        throw new NotImplementedException();
    }

    public void SetMaxTicksCountdown()
    {
        throw new NotImplementedException();
    }

    public int GetRequestId()
    {
        throw new NotImplementedException();
    }

    public bool IsCompleted()
    {
        throw new NotImplementedException();
    }

    public bool IsQueued()
    {
        throw new NotImplementedException();
    }

    public bool IsCanceled()
    {
        throw new NotImplementedException();
    }

    public int GetViewChangeCorrelationId()
    {
        throw new NotImplementedException();
    }

    public InteractionTrackerAsyncOperationType GetOperationType()
    {
        throw new NotImplementedException();
    }

    public void SetIsCompleted(bool isCompleted)
    {
        throw new NotImplementedException();
    }

    public InteractionTrackerAsyncOperationTrigger GetOperationTrigger()
    {
        return m_operationTrigger;
    }

    private InteractionTrackerAsyncOperationTrigger m_operationTrigger = InteractionTrackerAsyncOperationTrigger.DirectViewChange;
}
