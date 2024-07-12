namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial class ScrollPresenter
{
    private void CompleteDelayedOperations()
    {
        if (m_interactionTrackerAsyncOperations.Count <= 0)
        {
            return;
        }

        foreach (var operationsIter in m_interactionTrackerAsyncOperations)
        {
            var interactionTrackerAsyncOperation = operationsIter;

            if (interactionTrackerAsyncOperation.IsDelayed())
            {
                CompleteViewChange(interactionTrackerAsyncOperation, ScrollPresenterViewChangeResult.Interrupted);
                m_interactionTrackerAsyncOperations.Remove(interactionTrackerAsyncOperation);
            }
        }
    }
}