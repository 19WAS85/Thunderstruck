using System;
using System.Configuration;
using System.Data;

namespace Thunderstruck.Provider
{
    public interface IDataProvider : IDisposable
    {
        void CreateConnection(ConnectionStringSettings settings, Transaction transaction);

        Transaction TransactionMode { get; set; }

        string ParameterIdentifier { get; }

        IDataReader Query(string query, object queryParams);

        int Execute(string command, object commandParams);

        int ExecuteGetIdentity(string command, object commandParams);

        object ExecuteGetValue(string query, object queryParams);

        void Commit();
    }
}