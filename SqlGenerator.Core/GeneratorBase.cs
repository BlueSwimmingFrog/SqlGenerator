using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace SqlGenerator.Core
{
    public abstract class GeneratorBase
    {
        public GeneratorBase()
        {
            BlockSize = 50;
            IsInsertIdentity = false;
            IsUpdateByPrimaryKey = false;
            UpdateByColumnNames = new List<string>();                       
        }

        public string BlockEndTemplate { get; set; }
        public string BatchHeaderTemplate { get; set; }
        public string InsertTemplate { get; set; }
        public string UpdateTemplate { get; set; }
        public string UpdateValueAssignmentTemplate { get; set; }
        public string DeleteTemplate { get; set; }

        public string ConnectionString { get; set; }
        public string Query { get; set; }
        public int BlockSize { get; set; }
        public IDbConnection Connection { get; set; }
        
        public bool IsInsertIdentity { get; set; }
        public bool IsUpdateByPrimaryKey { get; set; }
        public bool IsInsertWhenNotExists { get; set; }
        public bool IsUpdateWhenExists { get; set; }
        public bool IsUpdateThroughDelete { get; set; }

        public List<String> UpdateByColumnNames { get; set; }

        public virtual DataSet GetDataSet()
        {
            Connection = GetConnection();

            DataSet ds = new DataSet();
            IDbCommand cmd = GetCommand();
            IDataAdapter adpt = GetDataAdapter(cmd);

            adpt.Fill(ds);

            return ds;
        }

        public virtual ResultSet GetResultSet()
        {
            var ds = GetDataSet();
            if (ds.Tables.Count > 1) throw new InvalidOperationException("Multiple datatables returned");

            var schema = GetSchema();

            int count = ds.Tables[0].Columns.Count;

            ResultSet rs = new ResultSet();
            rs.Schema = schema;

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Row row = new Row();

                foreach (DataColumn c in ds.Tables[0].Columns)
                {
                    var cs = schema.Columns[c.ColumnName];

                    Column col = (Column)Activator.CreateInstance(cs.ColumnType);
                    col.Schema = cs;
                    col.Value = r[c.ColumnName];

                    row.Columns.Add(cs.Name, col);
                }

                rs.Rows.Add(row);
            }


            return rs;
        }

        public abstract string GetTableName();
        public abstract RowSchema GetSchema();
        public abstract IDbCommand GetCommand();
        public abstract IDbCommand GetCommand(string sql);
        public abstract IDataAdapter GetDataAdapter(IDbCommand cmd);

        public virtual bool Evaluate()
        {
            return false;
        }

        protected string GetHeader(ResultSet rs)
        {
            string batchHeader = BatchHeaderTemplate;

            batchHeader = batchHeader.Replace("{Table}", rs.Schema.TableName);
            batchHeader = batchHeader.Replace("{Query}", Query);
            batchHeader = batchHeader.Replace("{Time}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            batchHeader = batchHeader.Replace("{BlockSize}", BlockSize.ToString());
            batchHeader = batchHeader.Replace("{IsInsertIdentity}", IsInsertIdentity ? "Yes" : "No");
            batchHeader = batchHeader.Replace("{IsUpdateByPrimaryKey}", IsUpdateByPrimaryKey ? "Yes" : "No");
            batchHeader = batchHeader.Replace("{PrimaryKey}", String.Join(", ", rs.Schema.Columns.Values.Where(n => n.IsPrimaryKey).OrderBy(n => n.Ordinal).Select(n => n.Name)));
            batchHeader = batchHeader.Replace("{UpdateByColumns}", String.Join(", ", UpdateByColumnNames));

            return batchHeader;
        }
     
        protected List<string> GetUpdateByColumns(ResultSet rs)
        {
            if (!IsUpdateByPrimaryKey && UpdateByColumnNames.Count > 0) return UpdateByColumnNames;

            List<String> PrimaryKeysInTable = rs.Schema.Columns.Values.Where(n => n.IsPrimaryKey).OrderBy(n => n.Ordinal).Select(n => n.Name).ToList();
            List<String> PrimaryKeysInQuery = rs.Rows.First().Columns.Values.Where(n => n.Schema.IsPrimaryKey).OrderBy(n => n.Schema.Ordinal).Select(n => n.Schema.Name).ToList();

            if (PrimaryKeysInQuery.Intersect(PrimaryKeysInTable).Count() != PrimaryKeysInQuery.Count) return new List<string>();

            return PrimaryKeysInQuery;

        }




        protected string GetUpdateStatementTemplate(ResultSet rs)
        {
            string sql = UpdateTemplate;

            List<string> updatebycolumns = GetUpdateByColumns(rs);

            if (updatebycolumns.Count == 0) throw new InvalidOperationException("Cannot build WHERE clause. No PRIMARY KEY defined on table or returned in query. Please provide update columns.");

            sql = sql.Replace("{Table}", rs.Schema.TableName);

            string assign = "";
            int assignCount = rs.Rows.First().Columns.Values.Where(n => !updatebycolumns.Contains(n.Schema.Name) && !n.Schema.IsIdentity).Count();
            int i = 0;

            foreach (var c in rs.Rows.First().Columns.Values.OrderBy(n => n.Schema.Ordinal))
            {
                if (updatebycolumns.Contains(c.Schema.Name)) continue;
                if (c.Schema.IsIdentity) continue;

                string assignvalue = UpdateValueAssignmentTemplate;
                assignvalue = assignvalue.Replace("{Column}", c.Schema.Name);
                assignvalue = assignvalue.Replace("{Value}", "{" + i.ToString() + "}");

                if (i < assignCount - 1)
                    assignvalue = assignvalue.Replace("{Comma}", ","); 
                else
                    assignvalue = assignvalue.Replace("{Comma}", ""); 

                assign = assign + assignvalue;
                i++;
            }

            sql = sql.Replace("{Assign}", assign);

            string whereclause = "";
            int j = 0;
            foreach (string key in updatebycolumns)
            {
                if (j > 0) whereclause = whereclause + " AND ";

                whereclause = key + " = {" + i.ToString() + "}";

                i++;
                j++;
            }

            sql = sql.Replace("{Where}", whereclause);

            return sql;
        }

        protected string GetUpdateStatement()
        {
            return "";
        }

        public virtual string GenerateUpdate()
        {
            ResultSet rs = GetResultSet();

            if (rs.Rows.Count == 0) return "";

            StringBuilder batch = new StringBuilder();
            batch.AppendLine(GetHeader(rs));

            string sql = GetUpdateStatementTemplate(rs);
            string insertsql = GetInsertStatementTemplate(rs);

            List<string> updatebycolumns = GetUpdateByColumns(rs);

            int i = 1;
            foreach (var r in rs.Rows)
            {
                List<String> values = r.Columns.Values.Where(n=>!updatebycolumns.Contains(n.Schema.Name) && !n.Schema.IsIdentity).OrderBy(n=>n.Schema.Ordinal).Select(n=>n.SqlValue).ToList();
                List<String> whereClauseValues = r.Columns.Values.Where(n=>updatebycolumns.Contains(n.Schema.Name)).OrderBy(n=>n.Schema.Ordinal).Select(n=>n.SqlValue).ToList();

                values.AddRange(whereClauseValues);

                //if (IsInsertWhenNotExists)
                //{
                //    batch.AppendLine("IF EXISTS (SELECT * FROM {Table} WHERE {Where})");
                //}

                batch.AppendLine(String.Format(sql, values.ToArray()));

                //if (IsInsertWhenNotExists)
                //{
                //    batch.AppendLine("ELSE");
                //    batch.AppendLine("");
                //}


                if (i % BlockSize == 0) batch.AppendLine(BlockEndTemplate);
                i++;
            }

            if (i % BlockSize != 0) batch.AppendLine(BlockEndTemplate);

            return batch.ToString();
        }

        public virtual void SaveUpdateToFile(string filename)
        {
            using (StreamWriter outfile = new StreamWriter(filename))
            {
                outfile.Write(GenerateUpdate());
            }
        }




        protected string GetInsertStatementTemplate(ResultSet rs)
        {
            string sql = InsertTemplate;
            string columns = "";

            foreach (var c in rs.Rows.First().Columns.Values.OrderBy(n => n.Schema.Ordinal))
            {
                if (!IsInsertIdentity && c.Schema.IsIdentity) continue;

                if (!String.IsNullOrEmpty(columns)) columns = columns + ", ";

                columns = columns + c.Schema.Name;
            }

            sql = sql.Replace("{Table}", rs.Schema.TableName);
            sql = sql.Replace("{Columns}", columns);
            sql = sql.Replace("{Values}", "{0}");

            return sql;
        }

        protected string GetInsertStatement()
        {
            return "";
        }

        public virtual string GenerateInsert()
        {
            ResultSet rs = GetResultSet();

            if (rs.Rows.Count == 0) return "";

            StringBuilder batch = new StringBuilder();
            batch.AppendLine(GetHeader(rs));

            string sql = GetInsertStatementTemplate(rs);

            if (IsInsertIdentity)
            {
                batch.AppendLine(String.Format("SET IDENTITY_INSERT {0} ON" + Environment.NewLine, rs.Schema.TableName));
                batch.AppendLine(BlockEndTemplate);
            }

            int i = 1;
            foreach (var r in rs.Rows)
            {

                string values = "";

                foreach (var c in r.Columns.Values.OrderBy(n => n.Schema.Ordinal))
                {
                    if (!IsInsertIdentity && c.Schema.IsIdentity) continue;

                    if (!String.IsNullOrEmpty(values)) values = values + ", ";
                    values = values + c.SqlValue;
                }

                batch.AppendLine(String.Format(sql, values));

                if (i % BlockSize == 0) batch.AppendLine(BlockEndTemplate);

                i++;
            }

            if (i % BlockSize != 0) batch.AppendLine(BlockEndTemplate);

            if (IsInsertIdentity)
            {
                batch.AppendLine(String.Format("SET IDENTITY_INSERT {0} OFF" + Environment.NewLine, rs.Schema.TableName));
                batch.AppendLine(BlockEndTemplate);
            }

            return batch.ToString();
        }

        public virtual void SaveInsertToFile(string filename)
        {
            using (StreamWriter outfile = new StreamWriter(filename))
            {
                outfile.Write(GenerateInsert());
            }
        }



        public bool TryTestConnection()
        {
            try
            {
                return TestConnection();
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool TestConnection()
        {

            if (String.IsNullOrEmpty(ConnectionString)) throw new InvalidOperationException("Connection String required");

            var cnn = GetConnection();

            if (cnn != null)
            {
                if (cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                    return true;
                }

                cnn.Dispose();
                return false;
            }

            return false;
        }

        public abstract IDbConnection GetConnection();
    }
}
