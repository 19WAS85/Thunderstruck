using System;
using System.Configuration;
using System.Data;

namespace Thunderstruck.Provider
{
    public interface IDataProvider : IDisposable
    {
        void CreateConnection(ConnectionStringSettings settings, Transaction transaction);

        IDbConnection DbConnection { get; set; }

        IDbTransaction DbTransaction { get; set; }

        Transaction TransactionMode { get; set; }

        string ParameterIdentifier { get; }

        string FieldFormat { get; }

        IDataReader Query(string query, object[] queryParams);

        int Execute(string command, object[] commandParams);

        int ExecuteGetIdentity(string command, object[] commandParams);

        object ExecuteGetValue(string query, object[] queryParams);

        string SelectAllQuery(string projection, string where);

        string SelectTakeQuery(string projection, string where, int count);

        void Commit();
    }
}