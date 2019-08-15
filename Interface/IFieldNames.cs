using System.Collections.Generic;

namespace etl.Interface
{
    public interface IFieldNames
    {
        IEnumerable<string> GetFieldNames();
    }
}
