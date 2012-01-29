// -----------------------------------------------------------------------
// <copyright file="ResolvableResourceEventArgs.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace LocalReportsEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ResolvableResourceEventArgs<T> : EventArgs
    {
        public readonly T Resolving;

        public ResolvableResourceEventArgs(T resolving)
        {
            Resolving = resolving;
        }
    }
}
