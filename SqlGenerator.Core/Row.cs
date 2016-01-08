using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class Row
    {
        public Row()
        {
            Columns = new Dictionary<string, Column>();
        }

        public Dictionary<string, Column> Columns { get; set; }

    }

    public class Column
    {
        public ColumnSchema Schema { get; set; }
        public object Value { get; set; }
        public virtual string SqlValue { get { return null; } }              

        public bool IsNull() { return Value == null; }
        public bool IsDbNull() { return Value.GetType() == typeof(DBNull); }
    }

    public class StringColumn : Column
    {
        public override string SqlValue
        {
            get
            {
                if (IsNull()) return "NULL";
                if (IsDbNull()) return "NULL";

                return "'" + Value.ToString().Replace("'", "''") + "'";
            }
        }
    }

    public class UnicodeStringColumn : Column
    {
        public override string SqlValue
        {
            get
            {
                if (IsNull()) return "NULL";
                if (IsDbNull()) return "NULL";

                return "N'" + Value.ToString().Replace("'", "''") + "'";
            }
        }
    }
    
    public class NumericColumn : Column
    {
        public override string SqlValue
        {
            get
            {
                if (IsNull()) return "NULL";
                if (IsDbNull()) return "NULL";

                return Value.ToString();
            }
        }
    }

    public class ImageColumn : Column
    {
        public override string SqlValue
        {
            get
            {
                if (IsNull()) return "NULL";
                if (IsDbNull()) return "NULL";

                return "'" + Value.ToString().Replace("'", "''") + "'";
            }
        }
    }

    public class DateColumn : Column
    {
        public override string SqlValue
        {
            get
            {
                if (IsNull()) return "NULL";
                if (IsDbNull()) return "NULL";

                return "'" + ((DateTime)Value).ToString("yyyy-MM-dd") + "'";
            }
        }
    }

    public class TimeColumn : Column
    {
        public override string SqlValue
        {
            get
            {
                if (IsNull()) return "NULL";
                if (IsDbNull()) return "NULL";

                return "'" + ((DateTime)Value).ToString("HH:mm:ss") + "'";
            }
        }
    }

    public class DateTimeColumn : Column
    {
        public override string SqlValue
        {
            get
            {
                if (IsNull()) return "NULL";
                if (IsDbNull()) return "NULL";

                return "'" + ((DateTime)Value).ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }
        }
    }

    public class RowSchema
    {
        public RowSchema()
        {
            Columns = new Dictionary<string, ColumnSchema>();
        }

        public string TableName { get; set; }
        public Dictionary<string, ColumnSchema> Columns { get; set; }
    }

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
