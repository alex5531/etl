using etl.Impl;
using etl.Interface;
using System;
using System.Collections.Generic;

namespace etl.Impl
{

    /// <summary>
    /// class that performs transformation
    /// </summary>
    public class Transf
    {
        private readonly ISrc _src;
        private readonly IDest _dest;
        private readonly Dictionary<string, string> _mapFields;
        private readonly Dictionary<string, Func<object, object>> _func;

        public event Action<Dictionary<string, object>> RowBeforeProcess;
        public event Action<Dictionary<string, object>> RowAfterProcess;
        public event Action<Transf, Exception> Error;
        public event Action<Dictionary<string, object>, Exception> RowError;

        private Action<Dictionary<string, object>, RowValidation> DoRowValidation { get; set; }

        /// <summary>
        /// constructor for transformation class
        /// </summary>
        /// <param name="src">source</param>
        /// <param name="dest">destination</param>
        public Transf(ISrc src, IDest dest)
        {
            _src = src;
            _dest = dest;
            _mapFields = new Dictionary<string, string>();
            _func = new Dictionary<string, Func<object, object>>();
            DoRowValidation = (row, v) => { };
        }

        /// <summary>
        /// mapping for a single field with the same name in source and destination
        /// </summary>
        /// <param name="srcField"></param>
        /// <returns></returns>
        public Transf Map(string srcField)
        {
            _mapFields[srcField] = srcField;
            if (_func.ContainsKey(srcField))
            {
                _func.Remove(srcField);
            }
            return this;
        }

        /// <summary>
        /// maps source field to a destination when names are different
        /// </summary>
        /// <param name="srcField"></param>
        /// <param name="destField"></param>
        /// <returns></returns>
        public Transf Map(string srcField, string destField)
        {
            _mapFields[srcField] = destField;
            if (_func.ContainsKey(srcField))
            {
                _func.Remove(srcField);
            }
            return this;
        }

        /// <summary>
        /// maps source field to destination field and stores transformation function for the field
        /// </summary>
        /// <param name="srcField"></param>
        /// <param name="destField"></param>
        /// <param name="transfFunc"></param>
        /// <returns></returns>
        public Transf Map(string srcField, string destField, Func<object, object> transfFunc)
        {
            _mapFields[srcField] = destField;
            if (transfFunc != null)
            {
                _func[srcField] = transfFunc;
            }
            return this;
        }

        /// <summary>
        /// maps source field to destination field and stores transformation function for the field. 
        /// Source field and destination field are the same type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcField"></param>
        /// <param name="destField"></param>
        /// <param name="transfFunc"></param>
        /// <returns></returns>
        public Transf Map<T>(string srcField, string destField, Func<T, T> transfFunc)
        {
            _mapFields[srcField] = destField;
            if (transfFunc != null)
            { 
                _func[srcField] = x => transfFunc((T)x);
            }
            return this;
        }

        /// <summary>
        /// maps source field to destination field and stores transformation function for the field. 
        /// Source field type is TSrc, destination field type is TDest
        /// </summary>
        /// <typeparam name="TSrc"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="srcField"></param>
        /// <param name="destField"></param>
        /// <param name="transfFunc"></param>
        /// <returns></returns>
        public Transf Map<TSrc, TDest>(string srcField, string destField, Func<TSrc, TDest> transfFunc)
        {
            _mapFields[srcField] = destField;
            if (transfFunc != null)
            {
                _func[srcField] = x => transfFunc((TSrc)x);
            }
            return this;
        }

        /// <summary>
        /// Makes transformation of all data
        /// </summary>
        /// <returns></returns>
        public Transf Execute()
        {
            if (_mapFields.Count == 0)
            {
                throw new InvalidOperationException("No mappings defined.");
            }

            try
            {
                _src.TransfSrcContext(() =>
                {
                    _dest.TransfDestContext(() =>
                    {
                        foreach (var srcRow in _src.Rows)
                        {
                            RowValidation rowValidation = new RowValidation();
                            DoRowValidation(srcRow, rowValidation);
                            if (rowValidation.HasErrors)
                            {
                                
                            }
                            else
                            {
                                Dictionary<string, object> destRow = new Dictionary<string, object>();
                                foreach (var mapping in _mapFields)
                                {
                                    object rowValue = srcRow[mapping.Key];
                                    if (_func.TryGetValue(mapping.Key, out Func<object, object> transfFunc))
                                    {
                                        rowValue = transfFunc(rowValue);
                                    }
                                    destRow[mapping.Value] = rowValue;
                                }
                                ProcessRow(destRow);
                            }
                        }
                    });
                });
            }
            catch (Exception exTransform)
            {

                if (Error != null)
                {
                    Error(this, exTransform);
                }
                else
                {
                    throw exTransform;
                }
            }

            return this;
        }

        /// <summary>
        /// Called for each row after transformation to perfom action on it (eg write to file, insert into DB etc)
        /// </summary>
        /// <param name="destRow"></param>
        private void ProcessRow(Dictionary<string, object> destRow)
        {
            if (RowBeforeProcess != null)
            {
                RowBeforeProcess(destRow);
            }
            try
            {
                _dest.Process(destRow);
            }
            catch (Exception exRow)
            {
                if (RowError != null)
                {
                    RowError(destRow, exRow);
                }
                else
                {
                    throw exRow;
                }
                
                if (RowAfterProcess != null)
                {
                    RowAfterProcess(destRow);
                }
            }
        }
    }
}
