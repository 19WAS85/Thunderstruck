using System.Configuration;
using System.Data;
using System.Data.Common;
using Thunderstruck.Internal;

namespace Thunderstruck.Provider
{
    public abstract class DefaultProvider : IDataProvider
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        
        public Transaction TransactionMode { get; set; }

        public abstract string ParameterIdentifier { get; }

        public void CreateConnection(ConnectionStringSettings connectionSettings, Transaction transaction)
        {
            TransactionMode = transaction;

            _connection = DbProviderFactories.GetFactory(connectionSettings.ProviderName).CreateConnection();
            _connection.ConnectionString = connectionSettings.ConnectionString;
        }

        public IDataReader Query(string query, object queryParams)
        {
            return CreateDbCommand(query, queryParams, transactional: false).ExecuteReader();
        }

        public int Execute(string command, object commandParams)
        {
            return CreateDbCommand(command, commandParams).ExecuteNonQuery();
        }

        public object ExecuteGetValue(string query, object queryParams)
        {
            return CreateDbCommand(query, queryParams).ExecuteScalar();
        }

        public abstract int ExecuteGetIdentity(string command, object commandParams);

        public void Commit()
        {
            if(_transaction != null) _transaction.Commit();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        protected IDbCommand CreateDbCommand(string query, object objectParameters, bool transactional = true)
        {
            Open(transactional);

            var command = _connection.CreateCommand();
            command.CommandText = query;
            command.Connection = _connection;
            command.Transaction = _transaction;

            if (objectParameters != null)
            {
                new ParametersBinder(ParameterIdentifier, objectParameters).Bind(command);
            }

            return command;
        }

        private void Open(bool transactional)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            if (TransactionMode == Transaction.Begin && transactional && _transaction == null)
            {
                _transaction = _connection.BeginTransaction();
            }
        }
    }
}