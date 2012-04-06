using System;
using System.Data;

namespace Thunderstruck.Provider.Common
{
    public class OracleProvider : DefaultProvider
    {
        private const string ParameterName = ":VPK";

        public override string ParameterIdentifier
        {
            get { return ":"; }
        }

        public override int ExecuteGetIdentity(string command, object commandParams)
        {
            var identityCommand = String.Concat("BEGIN ", command, " RETURNING IDPROJETO INTO ", ParameterName, "; END;");
            var dbCommand = CreateDbCommand(identityCommand, commandParams, true);

            var primarykeyParameter = dbCommand.CreateParameter();
            primarykeyParameter.ParameterName = ParameterName;
            primarykeyParameter.Direction = ParameterDirection.Output;
            primarykeyParameter.DbType = DbType.Int32;

            dbCommand.Parameters.Add(primarykeyParameter);
            dbCommand.ExecuteNonQuery();

            return Convert.ToInt32(primarykeyParameter.Value);
        }
    }
}
