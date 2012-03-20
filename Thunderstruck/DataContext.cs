using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Thunderstruck
{
    public enum Transaction { Begin, No }

    public class DataContext : IDisposable
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        static DataContext()
        {
            DefaultConnectionStringName = "Default";
        }

        /// <summary>
        /// Creates a new transactional data context to connection string named "Default".
        /// </summary>
        public DataContext() : this(null, Transaction.Begin) { }

        /// <summary>
        /// Creates a new data context to connection string named "Default". 
        /// </summary>
        /// <param name="transaction">Defines if data context is transactional.</param>
        public DataContext(Transaction transaction) : this(null, transaction) { }

        /// <summary>
        /// Creates a new data context.
        /// </summary>
        /// <param name="connectionString">Connection string name of target database.</param>
        /// <param name="transaction">Defines if data context is transactional.</param>
        public DataContext(string connectionStringName, Transaction transaction)
        {
            TransactionMode = transaction;

            var theConnectionStringName = connectionStringName == null ? DefaultConnectionStringName : connectionStringName;
            var connectionSettings = ConfigurationManager.ConnectionStrings[theConnectionStringName];

            _connection = CreateConnection(connectionSettings);
        }

        /// <summary>
        /// Defines the name of default connection string on application config.
        /// </summary>
        public static string DefaultConnectionStringName { get; set; }

        /// <summary>
        /// Defines the transaction mode of data context.
        /// </summary>
        public Transaction TransactionMode { get; private set; }

        /// <summary>
        /// Executes a sql query. Avoid.
        /// </summary>
        /// <param name="query">Query sql to execute on database.</param>
        /// <param name="queryParams">Object that contains parameters to bind in query.</param>
        /// <returns>An open data reader.</returns>
        public IDataReader Query(string query, object queryParams = null)
        {
            return CommandToExecute(query, queryParams, false).ExecuteReader();
        }

        /// <summary>
        /// Executes a sql command.
        /// </summary>
        /// <param name="command">Sql command to execute on database.</param>
        /// <param name="commandParams">Object that contains parameters to bind in query.</param>
        /// <returns>The number of rows affected.</returns>
        public int Execute(string command, object commandParams = null)
        {
            return CommandToExecute(command, commandParams, true).ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a sql command and returns the last identity value inserted.
        /// </summary>
        /// <param name="command">Sql command to execute on database.</param>
        /// <param name="commandParams">Object that contains parameters to bind in query.</param>
        /// <returns>The last identity value inserted into an identity column in the same scope.</returns>
        public int GetIdentity(string command, object commandParams = null)
        {
            var identityQuery = String.Concat(command, "; SELECT SCOPE_IDENTITY()");
            var value = CommandToExecute(identityQuery, commandParams, true).ExecuteScalar();

            return CastQueryValue<int>(value);
        }

        /// <summary>
        /// Executes a sql query and returns the value of first column of the first row. 
        /// </summary>
        /// <typeparam name="T">Type of return value.</typeparam>
        /// <param name="query">Query sql to execute on database.</param>
        /// <param name="commandParams">Object that contains parameters to bind in query.</param>
        /// <returns>The value of first column of the first row of the type specified on T.</returns>
        public T GetValue<T>(string query, object queryParams = null)
        {
            var value = CommandToExecute(query, queryParams, false).ExecuteScalar();

            return CastQueryValue<T>(value);
        }

        /// <summary>
        /// Executes a sql query and returns the value of first column of the first row. 
        /// </summary>
        /// <param name="query">Query sql to execute on database.</param>
        /// <param name="commandParams">Object that contains parameters to bind in query.</param>
        /// <returns>The value of first column of the first row.</returns>
        public object GetValue(string query, object queryParams = null)
        {
            return GetValue<object>(query, queryParams);
        }

        /// <summary>
        /// Commit data context commands on database. Just works on transactional data contexts.
        /// </summary>
        public void Commit()
        {
            if(_transaction != null) _transaction.Commit();
        }

        /// <summary>
        /// Closes the data context and database connection. Don't commit commands on transactional data contexts.
        /// </summary>
        public void Dispose()
        {
            _connection.Close();
        }

        private IDbConnection CreateConnection(ConnectionStringSettings connectionSettings)
        {
            var connection = DbProviderFactories.GetFactory(connectionSettings.ProviderName).CreateConnection();
            connection.ConnectionString = connectionSettings.ConnectionString;
            return connection;
        }

        private IDbCommand CommandToExecute(string query, object objectParameters, bool openTransaction)
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

            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.Connection = _connection;
            command.Transaction = _transaction;

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
            else return (T) Convert.ChangeType(value, typeof(T));
        }
    }
}