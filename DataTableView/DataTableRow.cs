using System.Collections.Generic;
using System.Text;

namespace DataTableView
{
    public class DataTableRow
    {
        public int ID { get; set; }
        public List<string> Cells { get; set; } = new List<string>();

        public override string ToString()
        {
            var builder = new StringBuilder();

            for (var i = 0; i < Cells.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(" ");
                }

                builder.Append(Cells[i]);
            }

            return builder.ToString();
        }
    }
}
