using System;

namespace Thunderstruck.Provider.Common
{
    public class SqlProvider : DefaultProvider
    {
        public override string ParameterIdentifier
        {
            get { return "@"; }
        }

        public override int ExecuteGetIdentity(string command, object[] commandParams)
        {
            var identityQuery = String.Concat(command, "; SELECT SCOPE_IDENTITY()");
            var value = CreateDbCommand(identityQuery, commandParams).ExecuteScalar();
            return Convert.ToInt32(value);
        }

        public override string FieldFormat
        {
            get { return "[{0}]"; }
        }

        public override string SelectAllQuery(string projection, string where)
        {
            return String.Format("SELECT {0} {1}", projection, where);
        }

        public override string SelectTakeQuery(string projection, string where, int count)
        {
            return String.Format("SELECT TOP {0} {1} {2}", count, projection, where);
        }
    }
}
