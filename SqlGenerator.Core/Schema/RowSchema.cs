using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.Core
{

    public class RowSchema
    {
        public RowSchema()
        {
            Columns = new Dictionary<string, ColumnSchema>();
        }

        public string TableName { get; set; }
        public Dictionary<string, ColumnSchema> Columns { get; set; }
    }

}
