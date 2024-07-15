using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
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
}
