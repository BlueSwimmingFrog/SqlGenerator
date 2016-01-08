using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator.Core
{
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
}
