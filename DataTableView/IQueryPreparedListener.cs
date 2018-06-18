using System;
using System.Collections.Generic;
using System.Text;

namespace Messert.Controls.Droid
{
    public interface IQueryPreparedListener
    {
        IEnumerable<DataTableRow> OnExecute(string query);
    }
}
