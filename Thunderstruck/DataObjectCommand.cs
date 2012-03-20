using System;
using System.Collections.Generic;
using System.Linq;

namespace Thunderstruck
{
    public class DataObjectCommand<T> where T : new()
    {
        private string _customTableName;
        private Type _targetType;
        private IEnumerable<string> _fields;

        public DataObjectCommand(string tableName = null)
        {
            _customTableName = tableName;
            _targetType = typeof(T);
            _fields = GetFieldsOf(_targetType);
        }

        public int Insert(T target, DataContext dataContext = null)
        {
            var parameters = _fields.Select(f => String.Concat("@", f));
            var command = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", GetTableName(), Comma(_fields), Comma(parameters));
            
            var identity = ExecuteOnDataContext(dataContext, data => data.GetIdentity(command, target));
            DataExtensions.GetPrimaryKey(_targetType).SetValue(target, identity, null);

            return identity;
        }

        public int Update(T target, DataContext dataContext = null)
        {
            var fieldsAndValues = GetFieldsOf(_targetType).Select(f => String.Concat(f, " = @", f));
            var primaryKey = DataExtensions.GetPrimaryKey(_targetType);
            var command = String.Format("UPDATE {0} SET {1} WHERE {2} = @{2}", GetTableName(), Comma(fieldsAndValues), primaryKey.Name);

            return ExecuteOnDataContext(dataContext, data => data.Execute(command, target));
        }

        public int Delete(T target, DataContext dataContext = null)
        {
            var primaryKey = DataExtensions.GetPrimaryKey(_targetType);
            var command = String.Format("DELETE FROM {0} Where {1} = @{1}", GetTableName(), primaryKey.Name);

            return ExecuteOnDataContext(dataContext, data => data.Execute(command, target));
        }

        private string GetTableName()
        {
            return _customTableName ?? _targetType.Name;
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

        private string Comma(IEnumerable<string> list)
        {
            return String.Join(", ", list);
        }
    }
}
