using System.Configuration;
using System.Data;
using System.Data.Common;
using Thunderstruck.Runtime;

namespace Thunderstruck.Provider
{
    public abstract class DefaultProvider : IDataProvider
    {
        public IDbConnection DbConnection { get; set; }

        public IDbTransaction DbTransaction { get; set; }

        public Transaction TransactionMode { get; set; }

        public abstract string ParameterIdentifier { get; }

        public abstract string FieldFormat { get; }

        public abstract string SelectAllQuery(string projection, string where);

        public abstract string SelectTakeQuery(string projection, string where, int count);

        public void CreateConnection(ConnectionStringSettings connectionSettings, Transaction transaction)
        {
            TransactionMode = transaction;

            DbConnection = DbProviderFactories.GetFactory(connectionSettings.ProviderName).CreateConnection();
            DbConnection.ConnectionString = connectionSettings.ConnectionString;
        }

        public IDataReader Query(string query, object[] queryParams)
        {
            return CreateDbCommand(query, queryParams, transactional: false).ExecuteReader();
        }

        public int Execute(string command, object[] commandParams)
        {
            return CreateDbCommand(command, commandParams).ExecuteNonQuery();
        }

        public object ExecuteGetValue(string query, object[] queryParams)
        {
            return CreateDbCommand(query, queryParams, transactional: false).ExecuteScalar();
        }

        public abstract int ExecuteGetIdentity(string command, object[] commandParams);

        public void Commit()
        {
            if(DbTransaction != null) DbTransaction.Commit();
        }

        public void Dispose()
        {
            if (DbTransaction != null) DbTransaction.Dispose();
            if (DbConnection != null) DbConnection.Close();
        }

        protected IDbCommand CreateDbCommand(string query, object[] objectParameters, bool transactional = true)
        {
            Open(transactional);

            var command = DbConnection.CreateCommand();
            command.CommandText = query;
            command.Connection = DbConnection;
            command.Transaction = DbTransaction;

            if (objectParameters != null)
            {
                new ParametersBinder(ParameterIdentifier, objectParameters).Bind(command);
            }

            return command;
        }

        private void Open(bool transactional)
        {
            if (DbConnection.State == ConnectionState.Closed)
            {
                DbConnection.Open();
            }

            if (TransactionMode == Transaction.Begin && transactional && DbTransaction == null)
            {
                DbTransaction = DbConnection.BeginTransaction();
            }
        }
    }
}