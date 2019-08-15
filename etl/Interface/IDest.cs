using System;
using System.Collections.Generic;

namespace etl.Interface
{
    public interface IDest
    {
        void TransfDestContext(Action action);
        void Process(Dictionary<string, object> destRow);
    }
}
