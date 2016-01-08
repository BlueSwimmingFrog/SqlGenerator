using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlGenerator.Core
{
    public class Row
    {
        public Row()
        {
            Columns = new Dictionary<string, Column>();
        }

        public Dictionary<string, Column> Columns { get; set; }

    }
}
