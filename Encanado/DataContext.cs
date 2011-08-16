using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Encanado.Access
{
    public class DataContext : IDisposable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;

        static DataContext()
        {
            DefaultConnectionStringName = "Default";
        }

        public DataContext() : this(null) { }

        public DataContext(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                connectionString = ConfigurationManager.ConnectionStrings[DefaultConnectionStringName].ConnectionString;
            }

            _connection = new SqlConnection(connectionString);
        }

        public static string DefaultConnectionStringName { get; set; }

        public SqlDataReader Query(string query, object queryParams = null)
        {
            return CommandToExecute(query, queryParams).ExecuteReader();
        }

        public int Execute(string command, object commandParams = null)
        {
            return CommandToExecute(command, commandParams).ExecuteNonQuery();
        }

        public int GetIdentity(string command, object commandParams = null)
        {
            var identityQuery = String.Concat(command, "; SELECT SCOPE_IDENTITY()");
            return GetValue<int>(identityQuery, commandParams);
        }

        public T GetValue<T>(string query, object queryParams = null)
        {
            var value = CommandToExecute(query, queryParams).ExecuteScalar();

            if (value is DBNull) return default(T);
            else return (T) Convert.ChangeType(value, typeof(T));
        }

        public object GetValue(string query, object queryParams = null)
        {
            return GetValue<object>(query, queryParams);
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        private SqlCommand CommandToExecute(string query, object objectParameters)
        {
            try
            {
                OpenConnection();
            }
            catch (Exception err)
            {
                var message = String.Format("Error to open connection: {0}", err.Message);
                throw new DataException(message, err);
            }

            var command = new SqlCommand { CommandText = query, Connection = _connection, Transaction = _transaction };
            if (objectParameters != null) command.AddParameters(objectParameters);

            return command;
        }

        private void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
                _transaction = _connection.BeginTransaction();
            }
        }
    }
}