using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.Core
{

    public class ResultSet
    {
        public ResultSet()
        {
            Rows = new List<Row>();
        }

        public List<Row> Rows { get; set; }
        public RowSchema Schema { get; set; }
    }
}
