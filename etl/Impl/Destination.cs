using etl.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace etl.Impl
{

    /// <summary>
    /// Class handles destination file
    /// </summary>
    public class Destination : IDest
    {
        private List<string> FieldNames { get; set; }
        private bool WithHeaderFlag { get; set; }
        private Action<string> _action;

        /// <summary>
        /// Constructor with a list of headers for the destination file
        /// </summary>
        /// <param name="fieldNames"></param>
        public Destination(List<string> fieldNames)
        {
            FieldNames = fieldNames;
        }

        /// <summary>
        /// Flag indicates presents header line in the destination file
        /// </summary>
        /// <param name="withHeader"></param>
        /// <returns></returns>
        public Destination WithHeader(bool withHeader)
        {
            WithHeaderFlag = withHeader;
            return this;
        }

        /// <summary>
        /// Action that should be performed on each line after transformationd
        /// For example (x) => Console.WriteLine(x) to send the line to Console
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Destination WithAction(Action<string> action)
        {
            _action = action;
            return this;
        }

        /// <summary>
        /// called in Destination context before transformation begins
        /// </summary>
        /// <param name="action"></param>
        public void TransfDestContext(Action action)
        {
            if (WithHeaderFlag)
            {
                _action(string.Join(",", FieldNames.ToArray()));
            }
             
            action();
        }

        /// <summary>
        /// Outputs all data
        /// </summary>
        /// <param name="destRow"></param>
        public void Process(Dictionary<string, object> destRow)
        {
            string str = string.Empty;
            foreach(var hdr in FieldNames)
            {
                if (destRow.TryGetValue(hdr, out object val))
                {
                    str = string.Concat(str, val.ToString(), ",");
                }
                else
                {
                    str = string.Concat(str, ",");
                }
            }
            _action(str.Substring(0, str.Length - 1));
        }
    }
}
