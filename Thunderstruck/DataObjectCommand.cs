using System;
using System.Collections.Generic;
using System.Linq;
using Thunderstruck.Internal;

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
            var data = dataContext ?? CreateDataContext();
            var parameters = _fields.Select(f => String.Concat(data.Provider.ParameterIdentifier, f));
            var command = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", GetTableName(), Comma(_fields), Comma(parameters));

            var identity = data.ExecuteGetIdentity(command, target);
            DataHelpers.GetPrimaryKey(_targetType).SetValue(target, identity, null);

            return identity;
        }

        public int Update(T target, DataContext dataContext = null)
        {
            var data = dataContext ?? CreateDataContext();
            var fieldsAndValues = GetFieldsOf(_targetType).Select(f => String.Concat(f, " = ", data.Provider.ParameterIdentifier, f));
            var primaryKey = DataHelpers.GetPrimaryKey(_targetType);

            var command = String.Format("UPDATE {0} SET {1} WHERE {2} = {3}{2}",
                GetTableName(), Comma(fieldsAndValues), primaryKey.Name, data.Provider.ParameterIdentifier);

            return data.Execute(command, target);
        }

        public int Delete(T target, DataContext dataContext = null)
        {
            var data = dataContext ?? CreateDataContext();
            var primaryKey = DataHelpers.GetPrimaryKey(_targetType);
            var command = String.Format("DELETE FROM {0} Where {1} = {2}{1}",
                GetTableName(), primaryKey.Name, data.Provider.ParameterIdentifier);

            return data.Execute(command, target);
        }

        private string GetTableName()
        {
            return _customTableName ?? _targetType.Name;
        }

        private static IEnumerable<string> GetFieldsOf(Type targetType)
        {
            return DataHelpers.GetValidPropertiesOf(targetType).Select(p => p.Name).Skip(1);
        }

        private string Comma(IEnumerable<string> list)
        {
            return String.Join(", ", list);
        }

        private DataContext CreateDataContext()
        {
            return new DataContext(Transaction.No);
        }
    }
}
