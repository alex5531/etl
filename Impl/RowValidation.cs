using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace etl.Impl
{

    /// <summary>
    /// class to hold errors
    /// </summary>
    public class RowValidation
    {
        private IDictionary<string, string> _errors;

        public IDictionary<string, string> Errors
        {
            get
            {
                if (_errors == null)
                    _errors = new Dictionary<string, string>();
                return _errors;
            }
        }

        public bool HasErrors
        {
            get
            {
                return _errors != null && _errors.Count > 0;
            }
        }
    }
}
