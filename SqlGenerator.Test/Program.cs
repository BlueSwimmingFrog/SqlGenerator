using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SqlGenerator.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlGenerator.Core.SqlServerGenerator gen = new Core.SqlServerGenerator();
            
            //gen.ConnectionString = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=ARMSPrototype;Data Source=mtbsql14v-dev";            
            gen.ConnectionString = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=ARMS;Data Source=MT-PC00607\\MTM";
            gen.UpdateByColumnNames.Add("TemplateName");
            gen.Query = "SELECT * FROM EmailTemplates";                        
            gen.SaveUpdateToFile(@"C:\SQL UPDATE " + gen.GetTableName() + ".txt");
            
            
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
