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
            var targetType = typeof(T);
            var tableName = _customTableName ?? targetType.Name;
            var fields = GetFieldsOf(targetType);
            var csvFields = String.Join(", ", fields);
            var atFields = String.Join(", ", fields.Select(f => String.Concat("@", f)));
            var command = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, csvFields, atFields);

            return ExecuteOnDataContext(dataContext, data => data.GetIdentity(command, target));
        }

        public int Update(T target, DataContext dataContext = null)
        {
            var targetType = typeof(T);
            var tableName = _customTableName ?? targetType.Name;
            var fields = GetFieldsOf(targetType).Select(f => String.Concat(f, " = @", f));
            var csvFields = String.Join(", ", fields);
            var command = String.Format("UPDATE {0} SET {1} WHERE Id = @Id", tableName, csvFields);

            return ExecuteOnDataContext(dataContext, data => data.Execute(command, target));
        }

        private static IEnumerable<string> GetFieldsOf(Type targetType)
        {
            return DataExtensions.GetValidPropertiesOf(targetType).Select(p => p.Name).Skip(1);
        }

        private int ExecuteOnDataContext(DataContext dataContext, Func<DataContext, int> action)
        {
            var data = dataContext ?? new DataContext(Transaction.No);

            try
            {
                return action(data);
            }
            finally
            {
                if (dataContext == null) data.Dispose();
            }
        }
    }
}
