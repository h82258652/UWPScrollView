﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
    public partial interface IScrollController
    {
        void SetValues(double minOffset,
        double maxOffset,
        double offset,
        double viewportLength);
    }
}