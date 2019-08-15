using etl.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace etl.Impl
{

    /// <summary>
    /// Class that holds source file
    /// </summary>
    public class Source : ISrc, IFieldNames
    {
        private readonly List<string> _fieldName;
        private readonly List<string> _values;

        /// <summary>
        /// Constructor for source file
        /// </summary>
        /// <param name="fieldNames">Headers from source file</param>
        /// <param name="values">values from source file</param>
        public Source(IEnumerable<string> fieldNames, IEnumerable<string> values)
        {
            _fieldName = fieldNames.ToList();
            _values = values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns headers from source file</returns>
        public IEnumerable<string> GetFieldNames()
        {
            return _fieldName;
        }

        /// <summary>
        /// data from source file
        /// </summary>
        public IEnumerable<Dictionary<string, object>> Rows
        {
            get
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int j = 0; j < _values.Count(); j += _fieldName.Count())
                {
                    for (var i = 0; i < _fieldName.Count(); i++)
                    {
                        row[_fieldName[i]] = _values[j + i];
                    }
                    yield return row;
                }
            }
         }

        /// <summary>
        /// action to be performed before transformation in source context
        /// </summary>
        /// <param name="action"></param>
        public void TransfSrcContext(Action action)
        {
            action();
        }
    }
}
