using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thunderstruck.Provider;

namespace Thunderstruck.Runtime
{
    public class ProviderResolver
    {
        public static Type CustomProviderType { get; set; }

        public IDataProvider Create(string providerName)
        {
            if (CustomProviderType != null) return CreateCustomProvider();

            switch (providerName)
            {
                case "System.Data.SqlClient": return new SqlProvider();
                case "System.Data.OracleClient": return new OracleProvider();
                default: return null;
            }
        }

        private IDataProvider CreateCustomProvider()
        {
            return Activator.CreateInstance(CustomProviderType) as IDataProvider;
        }
    }
}
