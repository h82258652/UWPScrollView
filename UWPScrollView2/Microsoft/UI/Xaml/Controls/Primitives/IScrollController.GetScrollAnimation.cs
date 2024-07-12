using System.Numerics;
using Windows.UI.Composition;

namespace Microsoft.UI.Xaml.Controls.Primitives;

public partial interface IScrollController
{
    CompositionAnimation? GetScrollAnimation(int correlationId, Vector2 startPosition, Vector2 endPosition, CompositionAnimation defaultAnimation);
}