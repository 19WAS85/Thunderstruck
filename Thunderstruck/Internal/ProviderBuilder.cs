using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thunderstruck.Provider;

namespace Thunderstruck.Internal
{
    internal class ProviderBuilder
    {
        internal IDataProvider Build(string providerName)
        {
            if (providerName.ToUpper().Contains("ORACLE")) return new OracleProvider();
            else if (providerName.ToUpper().Contains("SQL")) return new SqlProvider();
            else return null;
        }
    }
}
