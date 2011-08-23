using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Thunderstruck
{
    public enum Transaction { Begin, No }

    public class DataContext : IDisposable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;

        static DataContext()
        {
            DefaultConnectionStringName = "Default";
        }

        public DataContext() : this(null, Transaction.Begin) { }

        public DataContext(Transaction transaction) : this(null, transaction) { }

        public DataContext(string connectionString, Transaction transaction)
        {
            TransactionMode = transaction;

            if (String.IsNullOrEmpty(connectionString))
            {
                connectionString = ConfigurationManager.ConnectionStrings[DefaultConnectionStringName].ConnectionString;
            }

            _connection = new SqlConnection(connectionString);
        }

        public static string DefaultConnectionStringName { get; set; }

        public Transaction TransactionMode { get; set; }

        public SqlDataReader Query(string query, object queryParams = null)
        {
            return CommandToExecute(query, queryParams, false).ExecuteReader();
        }

        public int Execute(string command, object commandParams = null)
        {
            return CommandToExecute(command, commandParams, true).ExecuteNonQuery();
        }

        public int GetIdentity(string command, object commandParams = null)
        {
            var identityQuery = String.Concat(command, "; SELECT SCOPE_IDENTITY()");
            var value = CommandToExecute(identityQuery, commandParams, true).ExecuteScalar();

            return CastQueryValue<int>(value);
        }

        public T GetValue<T>(string query, object queryParams = null)
        {
            var value = CommandToExecute(query, queryParams, false).ExecuteScalar();

            return CastQueryValue<T>(value);
        }

        public object GetValue(string query, object queryParams = null)
        {
            return GetValue<object>(query, queryParams);
        }

        public void Commit()
        {
            if(_transaction != null) _transaction.Commit();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        private SqlCommand CommandToExecute(string query, object objectParameters, bool openTransaction)
        {
            try
            {
                OpenConnection(openTransaction);
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

        private void OpenConnection(bool openTransaction)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            if (TransactionMode == Transaction.Begin && openTransaction && _transaction == null)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        private T CastQueryValue<T>(object value)
        {
            if (value is DBNull) return default(T);
            else return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}