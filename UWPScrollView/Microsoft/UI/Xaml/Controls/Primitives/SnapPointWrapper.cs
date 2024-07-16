using System;
using Windows.UI.Composition;

namespace Microsoft.UI.Xaml.Controls.Primitives;

internal class SnapPointWrapper<T>
{
    private T m_snapPoint;

    public SnapPointWrapper(T snapPoint)
    {
        throw new NotImplementedException();
    }

    public bool ResetIgnoredValue()
    {
        throw new NotImplementedException();
    }

    public bool SnapsAt(double value)
    {
        throw new NotImplementedException();
    }

    public void SetIgnoredValue(double ignoredValue)
    {
        throw new NotImplementedException();

    }

    public static SnapPointBase GetSnapPointFromWrapper(SnapPointWrapper<T> snapPointWrapper)
    {
        throw new NotImplementedException();
    }

    private int m_combinationCount;

    private ExpressionAnimation m_conditionExpressionAnimation;

    public Tuple<ExpressionAnimation, ExpressionAnimation> GetUpdatedExpressionAnimationsForImpulse()
    {
        throw new NotImplementedException();
    }

    public void Combine(SnapPointWrapper<T> snapPointWrapper)
    {
        throw new NotImplementedException();
    }
}