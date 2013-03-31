using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thunderstruck.Provider;
using Thunderstruck.Provider.Common;

namespace Thunderstruck.Runtime
{
    public class ProviderResolver
    {
        private static Dictionary<string, IDataProvider> providerBuffer = new Dictionary<string, IDataProvider>();

        public static IDataProvider CustomProvider { get; set; }

        public static IDataProvider Get(string providerName)
        {
            if (CustomProvider != null) return CustomProvider;
            if (providerBuffer.ContainsKey(providerName)) return providerBuffer[providerName];

            IDataProvider provider = CreateProvider(providerName);
            providerBuffer.Add(providerName, provider);

            return providerBuffer[providerName];
        }

        private static IDataProvider CreateProvider(string name)
        {
            switch (name)
            {
                case "System.Data.SqlClient": return new SqlProvider();
                case "System.Data.OracleClient": return new OracleProvider();
                case "MySql.Data.MySqlClient": return new MySqlProvider();
                default:
                    var exceptionMessage = String.Concat(
                        "Thunderstruck do not supports the ", name,
                        "provider. Try create and set a ProviderResolver.CustomProvider, it is easy.");
                    throw new ThunderException(exceptionMessage);
            }
        }
    }
}
