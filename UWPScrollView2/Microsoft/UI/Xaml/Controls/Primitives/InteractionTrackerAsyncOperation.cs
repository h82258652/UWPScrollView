using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
    internal class InteractionTrackerAsyncOperation
    {
        private bool m_isDelayed  = false ;
        internal   bool IsDelayed()
        {
        return m_isDelayed;
    }

}
}
