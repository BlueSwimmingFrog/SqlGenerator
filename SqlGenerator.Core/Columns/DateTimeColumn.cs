using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.Core
{
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
}
