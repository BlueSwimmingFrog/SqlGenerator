using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SqlGenerator.Core
{
    public class StatementGeneratorBase
    {        

        public ResultSet ResultSet { get; set; }
     
        public string StatementTemplate { get; set; }


        public virtual string GenerateStatementTemplate()
        {
            return "";
        }

        public virtual string GenerateStatement()
        {
            return "";
        }

        public virtual string GenerateBatch()
        {
            return "";
        }

        public virtual void SaveBatchToFile(string filename)
        {
            using (StreamWriter outfile = new StreamWriter(filename))
            {
                outfile.Write(GenerateBatch());
            }
        }

    }
}
