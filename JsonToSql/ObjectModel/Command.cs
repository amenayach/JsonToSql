using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JsonToSql.ObjectModel
{
    /// <summary>
    /// Represent a SQL command
    /// </summary>
    public class Command
    {

        /// <summary>
        /// An incremental parameter
        /// </summary>
        private int _paramCounter;

        /// <summary>
        /// The list of parameters
        /// </summary>
        private List<SqlParameter> _params;

        /// <summary>
        /// The type of SQL command
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        /// The targeted SQL table
        /// </summary>
        public string Tablename { get; set; }

        /// <summary>
        /// The json that holds the table data
        /// </summary>
        public JObject Json { get; set; }

        /// <summary>
        /// Execute the command
        /// </summary>
        public void Execute(SqlConnection connection)
        {

            CleanParams();

            AdjustCommandType(connection);

            ValidateInput(connection);

            switch (Type)
            {
                case CommandType.Delete:
                    SqlManager.ExecuteCommand($"DELETE FROM [{ Tablename }] WHERE { GetTablePrimaryConditions(connection) }", connection, _params.ToArray());
                    break;

                case CommandType.Insert:
                    SqlManager.ExecuteCommand($"INSERT INTO [{ Tablename }] { GetInsertFieldsScript(connection) } ", connection, _params.ToArray());
                    break;

                case CommandType.Update:
                    SqlManager.ExecuteCommand($"UPDATE [{ Tablename }] SET { GetUpdateFieldsScript(connection) } WHERE { GetTablePrimaryConditions(connection) }", connection, _params.ToArray());
                    break;
            }
        }

        private void ValidateInput(SqlConnection connection)
        {
            if (Json == null)
            {
                throw new Exception("Empty data");
            }

            if (string.IsNullOrWhiteSpace(Tablename))
            {
                throw new Exception("Empty table name");
            }
        }

        private void AdjustCommandType(SqlConnection connection)
        {
            if (Type == CommandType.None)
            {

                var keys = GetTablePrimaryKeys(connection);

                foreach (var key in keys)
                {

                    var jsonEquivalent = Json.Properties().FirstOrDefault(m => m.Name.ToLower().Trim() == key.ToLower().Trim());

                    if (jsonEquivalent == null || ((JValue)jsonEquivalent.Value).Value == null)
                    {

                        Type = CommandType.Insert;
                        return;
                        
                    }
                    
                }

                if (keys != null && keys.Count > 0)
                {
                    Type = CommandType.Update;
                }

            }
        }

        /// <summary>
        /// Construct the insert script
        /// </summary>
        private string GetInsertFieldsScript(SqlConnection connection)
        {

            var fields = "(";

            var values = "VALUES (";

            var columns = GetTableColumns(connection).Except(GetTableAutoNumbers(connection));

            foreach (JProperty property in Json.Properties())
            {
                if (Json[property.Name] is JValue && columns.FirstOrDefault(m => m.ToLower().Trim() == property.Name.ToLower().Trim()) != null)
                {

                    var paramName = GetParamName();

                    fields += (fields == "(" ? "" : ", ") + $"[{property.Name}]";

                    values += (values == "VALUES (" ? "" : ", ") + $"@{paramName}";

                    _params.Add(new SqlParameter(paramName, ((JValue)property.Value).Value));

                }
            }

            return fields + ") " + values + "); " + Environment.NewLine;

        }

        /// <summary>
        /// Construct the update query for a given SQL table
        /// </summary>
        private string GetUpdateFieldsScript(SqlConnection connection)
        {
            var result = string.Empty;

            var columns = GetTableNonPrimaryColumns(connection);

            foreach (JProperty property in Json.Properties())
            {
                if (Json[property.Name] is JValue && columns.FirstOrDefault(m => m.ToLower().Trim() == property.Name.ToLower().Trim()) != null)
                {

                    var paramName = GetParamName();

                    result += (string.IsNullOrEmpty(result) ? string.Empty : ",") + $" [{property.Name}] = @{paramName}";

                    _params.Add(new SqlParameter(paramName, ((JValue)property.Value).Value));

                }
            }

            return result + " " + Environment.NewLine;
        }

        /// <summary>
        /// Cleans the params or initiate it
        /// </summary>
        private void CleanParams()
        {
            if (_params == null)
            {
                _params = new List<SqlParameter>();
            }
            else
            {
                _params.Clear();
            }
        }

        /// <summary>
        /// Gets the primary keys conditions
        /// </summary>
        private string GetTablePrimaryConditions(SqlConnection connection)
        {

            var result = string.Empty;

            var keys = GetTablePrimaryKeys(connection);

            foreach (var key in keys)
            {

                // ReSharper disable once SimplifyLinqExpression
                if (!Json.Properties().Any(m => m.Name.ToLower().Trim() == key.ToLower().Trim()))
                {
                    throw new ArgumentException("missing key");
                }

                var paramName = GetParamName();

                result += (string.IsNullOrEmpty(result) ? "" : " AND") + $" [{key}] = @{paramName} ";

                _params.Add(new SqlParameter(paramName, ((JValue)Json[key]).Value));

            }

            return result;

        }

        /// <summary>
        /// Gets the table columns
        /// </summary>
        private List<string> GetTableColumns(SqlConnection connection)
        {
            return SqlManager.ExecuteQueryNoCon<string>(@"select cols.COLUMN_NAME 
                        from INFORMATION_SCHEMA.COLUMNS cols 
                        WHERE cols.TABLE_NAME = @tablename", connection, new SqlParameter("tablename", Tablename));
        }

        /// <summary>
        /// Gets the table columns except the primary keys
        /// </summary>
        private List<string> GetTableNonPrimaryColumns(SqlConnection connection)
        {
            return GetTableColumns(connection).Except(GetTablePrimaryKeys(connection)).ToList();
        }

        /// <summary>
        /// Gets the table primary keys
        /// </summary>
        private List<string> GetTablePrimaryKeys(SqlConnection connection)
        {
            return SqlManager.ExecuteQueryNoCon<string>(@"SELECT Col.Column_Name from 
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, 
                INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col 
            WHERE 
                Col.Constraint_Name = Tab.Constraint_Name
                AND Col.Table_Name = Tab.Table_Name
                AND Constraint_Type = 'PRIMARY KEY'
                AND Col.Table_Name = @tablename", connection, new SqlParameter("tablename", Tablename));
        }

        /// <summary>
        /// Gets the table auto-increment fields if exists
        /// </summary>
        private List<string> GetTableAutoNumbers(SqlConnection connection)
        {
            return SqlManager.ExecuteQueryNoCon<string>(@"SELECT cols.name from sys.columns as cols 
                        inner join sys.tables as tabs on cols.object_id = tabs.object_id
                        where OBJECT_NAME(cols.object_id) = @tablename and cols.is_identity = 1", connection, new SqlParameter("tablename", Tablename));
        }

        /// <summary>
        /// Gets a new parameter name
        /// </summary>
        private string GetParamName()
        {
            return "p_" + DateTime.Now.Ticks + "_" + ++_paramCounter;
        }
    }
}
