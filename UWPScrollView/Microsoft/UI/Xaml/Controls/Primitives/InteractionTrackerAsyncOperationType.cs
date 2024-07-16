namespace Microsoft.UI.Xaml.Controls.Primitives;

internal enum InteractionTrackerAsyncOperationType
{
    None,
    TryUpdatePosition,
    TryUpdatePositionBy,
    TryUpdatePositionWithAnimation,
    TryUpdatePositionWithAdditionalVelocity,
    TryUpdateScale,
    TryUpdateScaleWithAnimation,
    TryUpdateScaleWithAdditionalVelocity,
}