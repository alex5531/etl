using System;
using System.Collections.Generic;

namespace etl.Interface
{
    public interface ISrc
    {
        void TransfSrcContext(Action action);
        IEnumerable<Dictionary<string, object>> Rows { get; }
    }
}
