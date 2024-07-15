using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
    internal class ScrollPresenterTestHooks
    {
        private static ScrollPresenterTestHooks s_testHooks;

        private bool m_areAnchorNotificationsRaised;

        private bool m_areExpressionAnimationStatusNotificationsRaised;
    }
}
