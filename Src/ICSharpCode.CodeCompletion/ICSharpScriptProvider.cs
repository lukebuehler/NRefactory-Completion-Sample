using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.CodeCompletion
{
    /// <summary>
    /// This interface allows to provide more information for scripts such as using statements, etc.
    /// </summary>
    public interface ICSharpScriptProvider
    {
        string GetUsing();
        string GetVars();
    }
}
