using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JsonToSql.ObjectModel
{
    public class JsonToSql
    {

        /// <summary>
        /// Holds the sql connection string
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Holds the sql connection
        /// </summary>
        private SqlConnection _connection;

        private List<Command> commands;

        public JsonToSql(string connectionString)
        {
            this._connectionString = connectionString;
            commands = new List<Command>();
        }

        /// <summary>
        /// Connect to the sql
        /// </summary>
        private void Connect()
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {

                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

            }
        }

        /// <summary>
        /// Save an array to the SQL correspondent table 
        /// </summary>
        public JsonToSql Save(string tableName, JArray json, CommandType type = CommandType.None)
        {

            Connect();

            foreach (var obj in json)
            {

                commands.Add(new Command()
                {
                    Json = obj as JObject,
                    Type = type,
                    Tablename = tableName
                });

            }

            return this;
        }

        /// <summary>
        /// Save a JSON object to the SQL correspondent table 
        /// </summary>
        public JsonToSql Save(string tableName, JObject json, CommandType type = CommandType.None)
        {

            Connect();

            commands.Add(new Command()
            {
                Json = json,
                Type = type,
                Tablename = tableName
            });

            return this;

        }

        /// <summary>
        /// Execute the queue of commands
        /// </summary>
        public void SaveChanges()
        {

            Connect();

            foreach (var command in commands)
            {

                command.Execute(_connection);

            }

        }

    }
}
