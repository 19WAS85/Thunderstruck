using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Thunderstruck.Provider.Common;

namespace Thunderstruck.Provider
{
    public class ProviderFactory
    {
        public static Func<string, IDataProvider> CustomProvider { get; set; }

        public static Func<string, IDbConnection> ConnectionFactory { get; set; }

        public IDataProvider Create(ConnectionStringSettings settings, Transaction transactionMode)
        {
            var provider = ResolveDataProvider(settings.ProviderName);
            provider.DbConnection = CreateConnection(settings.ProviderName, settings);
            provider.TransactionMode = transactionMode;
            return provider;
        }

        public IDataProvider ResolveDataProvider(string providerName)
        {
            if (CustomProvider != null) return CustomProvider(providerName);

            switch (providerName)
            {
                case "System.Data.SqlClient": return new SqlProvider();
                case "System.Data.OracleClient": return new OracleProvider();
                case "MySql.Data.MySqlClient": return new MySqlProvider();
                default:
                    var exceptionMessage = String.Concat(
                        "Thunderstruck do not supports the '", providerName,
                        "' provider. Try create and set a ProviderResolver.CustomProvider, it is easy.");
                    throw new ThunderException(exceptionMessage);
            }
        }

        private IDbConnection CreateConnection(string providerName, ConnectionStringSettings settings)
        {
            IDbConnection connection;

            if (ConnectionFactory != null) connection = ConnectionFactory(providerName);
            else connection = DbProviderFactories.GetFactory(providerName).CreateConnection();

            connection.ConnectionString = settings.ConnectionString;

            return connection;
        }
    }
}
