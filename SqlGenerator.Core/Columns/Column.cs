using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.Core
{

    public class Column
    {
        public ColumnSchema Schema { get; set; }
        public object Value { get; set; }
        public virtual string SqlValue { get { return null; } }

        public bool IsNull() { return Value == null; }
        public bool IsDbNull() { return Value.GetType() == typeof(DBNull); }
    }
}
