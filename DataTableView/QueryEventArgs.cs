using System;
using System.Collections.Generic;
using System.Text;

namespace Messert.Controls.Droid
{
    public class QueryEventArgs : EventArgs
    {
        public string Query { get; private set; }

        public QueryEventArgs(string query)
        {
            Query = query;
        }
    }
}
