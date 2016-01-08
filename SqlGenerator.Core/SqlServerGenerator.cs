using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SqlGenerator.Core
{
    public class SqlServerGenerator : GeneratorBase
    {
        public SqlServerGenerator()
            : base()
        {
            BatchHeaderTemplate = "/*" + Environment.NewLine +
                                 "Table: {Table}" + Environment.NewLine +
                                 "Query: {Query}" + Environment.NewLine +
                                 "Generated at: {Time}" + Environment.NewLine +
                                 "Block Size: {BlockSize}" + Environment.NewLine +
                                 "Insert Identity: {IsInsertIdentity}" + Environment.NewLine +
                                 "Update By Primary Key: {IsUpdateByPrimaryKey}" + Environment.NewLine +
                                 "Primary Key: {PrimaryKey}" + Environment.NewLine +
                                 "Update by Columns: {UpdateByColumns}" + Environment.NewLine +
                                 "*/" + Environment.NewLine;

            InsertTemplate = "INSERT INTO {Table} ({Columns})" + Environment.NewLine +
                              "VALUES ({Values})" + Environment.NewLine;

            UpdateTemplate = "UPDATE {Table} SET " + Environment.NewLine + "{Assign} WHERE {Where}" + Environment.NewLine;

            UpdateValueAssignmentTemplate = "{Column} = {Value}{Comma} " + Environment.NewLine;

            BlockEndTemplate = "GO" + Environment.NewLine;
        }

        public override System.Data.IDataAdapter GetDataAdapter(IDbCommand cmd)
        {
            SqlCommand sqlCmd = (SqlCommand)cmd;
            SqlDataAdapter sqlAdpt = new SqlDataAdapter(sqlCmd);

            return sqlAdpt;
        }

        public override IDbCommand GetCommand()
        {
            SqlCommand cmd = new SqlCommand(Query, (SqlConnection)Connection);

            return cmd;
        }

        public override IDbConnection GetConnection()
        {

            var cnn = new SqlConnection(ConnectionString);
            cnn.Open();

            return cnn;

        }

        public override string GetTableName()
        {
            var match = System.Text.RegularExpressions.Regex.Match(Query, @"((?<=FROM )[^\s]+)");

            return match.Value;
        }

        public Type GetColumnType(string dbtype)
        {
            string[] UnicodeStringTypes = { "nvarchar", "nchar" };
            if (UnicodeStringTypes.Contains(dbtype, StringComparer.OrdinalIgnoreCase))
                return typeof(UnicodeStringColumn);

            string[] StringTypes = { "varchar", "char","uniqueidentifier" };
            if (StringTypes.Contains(dbtype, StringComparer.OrdinalIgnoreCase))
                return typeof(StringColumn);

            string[] NumericTypes = { "decimal", "int", "float", "bit" };
            if (NumericTypes.Contains(dbtype, StringComparer.OrdinalIgnoreCase))
                return typeof(NumericColumn);

            string[] ImageTypes = { "image", "binary", "varbinary" };
            if (ImageTypes.Contains(dbtype, StringComparer.OrdinalIgnoreCase))
                return typeof(ImageColumn);

            string[] DateTypes = { "date"};
            if (DateTypes.Contains(dbtype, StringComparer.OrdinalIgnoreCase))
                return typeof(DateColumn);

            string[] DateTimeTypes = { "datetime" };
            if (DateTimeTypes.Contains(dbtype, StringComparer.OrdinalIgnoreCase))
                return typeof(DateTimeColumn);
            
            string[] TimeTypes = { "time" };
            if (TimeTypes.Contains(dbtype, StringComparer.OrdinalIgnoreCase))
                return typeof(TimeColumn);


            throw new InvalidOperationException(String.Format("Unsupported dbtype '{0}'", dbtype));

        }

        public override RowSchema GetSchema()
        {
            string sql = @"SELECT *, 
COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IDENTITY_COL,
CASE WHEN EXISTS(SELECT Col.Column_Name from 
    INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, 
    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col 
WHERE 
    Col.Constraint_Name = Tab.Constraint_Name
    AND Col.Table_Name = Tab.Table_Name
    AND Constraint_Type = 'PRIMARY KEY'
    AND Col.Table_Name = C1.TABLE_NAME AND Col.Column_Name=C1.COLUMN_NAME) THEN 1 ELSE 0 END AS PRIMARY_KEY
FROM Information_Schema.COLUMNS C1 WHERE TABLE_NAME='{0}'";
          
            string tableName = GetTableName();

            var cmd = GetCommand(String.Format(sql, tableName ));
            var adpt = GetDataAdapter(cmd);

            DataSet ds = new DataSet();
            adpt.Fill(ds);
            var tbl = ds.Tables[0];

            RowSchema rs = new RowSchema();
            rs.TableName = tableName;
            foreach (DataRow r in tbl.Rows)
            {
                ColumnSchema cs = new ColumnSchema();
                cs.Name = r["COLUMN_NAME"].ToString();
                cs.SqlType = r["DATA_TYPE"].ToString();
                cs.ColumnType = GetColumnType(cs.SqlType);
                cs.IsNullable = r["IS_NULLABLE"].ToString() == "YES";
                cs.CharLength = r["CHARACTER_MAXIMUM_LENGTH"] == DBNull.Value ? 0 : Convert.ToInt32(r["CHARACTER_MAXIMUM_LENGTH"]);
                cs.NumericPrecision = r["NUMERIC_PRECISION"] == DBNull.Value ? 0 : Convert.ToInt32(r["NUMERIC_PRECISION"]);
                cs.IsIdentity = (int)r["IDENTITY_COL"] == 1;
                cs.IsPrimaryKey = (int)r["PRIMARY_KEY"] == 1;

                rs.Columns.Add(cs.Name, cs);
            }

            return rs;

        }
        
        public override IDbCommand GetCommand(string sql)
        {
            var cmd = GetCommand();
            cmd.CommandText = sql;
            return cmd;
        }
    }
}
