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

        public override int ExecuteGetIdentity(string command, object[] commandParams)
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

        public override string FieldFormat
        {
            get { return "\"{0}\""; }
        }

        public override string SelectAllQuery(string projection, string where)
        {
            return String.Format("SELECT {0} {1}", projection, where);
        }

        public override string SelectTakeQuery(string projection, string where, int count)
        {
            return String.Format("SELECT {0} {1} LIMIT {2}", projection, where, count);
        }
    }
}
