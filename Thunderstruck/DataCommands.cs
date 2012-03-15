using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thunderstruck
{
    public class DataCommands<T> where T : new()
    {
        private string _customTableName;

        public DataCommands(string tableName = null)
        {
            _customTableName = tableName;
        }

        public int Insert(T target, DataContext dataContext = null)
        {
            var data = dataContext ?? new DataContext(Transaction.No);

            try
            {
                var targetType = typeof(T);
                var tableName = _customTableName ?? targetType.Name;

                var fields = DataExtensions.GetValidPropertiesOf(targetType).Select(p => p.Name).Skip(1);
                var csvFields = String.Join(", ", fields);
                var atFields = String.Join(", ", fields.Select(f => String.Concat("@", f)));

                var command = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, csvFields, atFields);

                return data.GetIdentity(command, target);
            }
            finally
            {
                if (dataContext == null) data.Dispose();
            }
        }
    }
}
