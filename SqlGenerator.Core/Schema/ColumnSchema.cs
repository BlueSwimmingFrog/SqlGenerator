using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.Core
{
    public class ColumnSchema
    {
        public string Name { get; set; }
        public int Ordinal { get; set; }
        public bool IsNullable { get; set; }
        public string SqlType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public int CharLength { get; set; }
        public int NumericPrecision { get; set; }
        public Type ColumnType { get; set; }

    }
}
