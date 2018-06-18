using System.Collections.Generic;

namespace Messert.Controls.Droid
{
    public interface IQueryPreparedListener
    {
        IEnumerable<DataTableRow> OnExecute(string query);
    }
}
