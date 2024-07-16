using System;
using Windows.UI.Composition.Interactions;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class InteractionTrackerOwner : IInteractionTrackerOwner
{
    private WeakReference<ScrollPresenter> m_owner;

    public InteractionTrackerOwner(ScrollPresenter scrollPresenter)
    {
        m_owner = new WeakReference<ScrollPresenter>(scrollPresenter);
    }

    public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
    {
        if (m_owner is null)
        {
            return;
        }

        if (m_owner.TryGetTarget(out var rawOwner))
        {
            var scrollPresenter = rawOwner as ScrollPresenter;
            if (scrollPresenter is not null)
            {
                scrollPresenter.CustomAnimationStateEntered(args);
            }
        }
    }

    public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
    {
        if (m_owner is null)
        {
            return;
        }

        if (m_owner.TryGetTarget(out var rawOwner))
        {
            var scrollPresenter = rawOwner as ScrollPresenter;
            if (scrollPresenter is not null)
            {
                scrollPresenter.IdleStateEntered(args);
            }
        }
    }

    public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
    {
        if (m_owner is null)
        {
            return;
        }

        if (m_owner.TryGetTarget(out var rawOwner))
        {
            var scrollPresenter = rawOwner as ScrollPresenter;
            if (scrollPresenter is not null)
            {
                scrollPresenter.InertiaStateEntered(args);
            }
        }

    }

    public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
    {
        throw new NotImplementedException();
    }

    public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
    {
        throw new NotImplementedException();
    }

    public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
    {
        if (m_owner is null)
        {
            return;
        }

        if (m_owner.TryGetTarget(out var rawOwner))
        {
            var scrollPresenter = rawOwner as ScrollPresenter;
            if (scrollPresenter is not null)
            {
                scrollPresenter.ValuesChanged(args);
            }
        }
    }
}