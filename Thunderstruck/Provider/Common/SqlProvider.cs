using System;

namespace Thunderstruck.Provider.Common
{
    public class SqlProvider : DefaultProvider
    {
        public override string ParameterIdentifier
        {
            get { return "@"; }
        }

        public override int ExecuteGetIdentity(string command, object commandParams)
        {
            var identityQuery = String.Concat(command, "; SELECT SCOPE_IDENTITY()");
            var value = CreateDbCommand(identityQuery, commandParams).ExecuteScalar();
            return Convert.ToInt32(value);
        }
    }
}
