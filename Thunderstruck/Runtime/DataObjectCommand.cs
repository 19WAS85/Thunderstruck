using System;
using System.Linq;
using System.Reflection;

namespace Thunderstruck.Runtime
{
    public class DataObjectCommand<T> where T : new()
    {
        private const string InsertSql = "INSERT INTO {0} ({1}) VALUES ({2})";
        private const string UpdateSql = "UPDATE {0} SET {1} WHERE {2} = {3}{2}";
        private const string DeleteSql = "DELETE FROM {0} Where {1} = {2}{1}";

        private readonly DataRuntimeObject<T> _runtimeObject;
        private readonly string _customTableName;
        private readonly PropertyInfo _primaryKey;

        public DataObjectCommand(string table = null, string primaryKey = null)
        {
            _runtimeObject = new DataRuntimeObject<T>();
            _customTableName = table;
            _primaryKey = _runtimeObject.GetPrimaryKey(primaryKey);
        }

        public int Insert(T target, DataContext dataContext = null)
        {
            return ExecuteOnDataContext(dataContext, data =>
            {
                var fields = GetCommaFields(data);
                var parameters = _runtimeObject.CreateCommaParameters(data.Provider.ParameterIdentifier, _primaryKey.Name);
                var command = String.Format(InsertSql, GetTableName(), fields, parameters);
                var identity = data.ExecuteGetIdentity(command, target);
                _primaryKey.SetValue(target, identity, null);
                return identity;
            });
        }

        public int Update(T target, DataContext dataContext = null)
        {
            return ExecuteOnDataContext(dataContext, data =>
            {
                var fields = _runtimeObject.CreateCommaFieldsAndParameters(data.Provider.ParameterIdentifier, _primaryKey.Name);
                var command = String.Format(UpdateSql, GetTableName(), fields, _primaryKey.Name, data.Provider.ParameterIdentifier);
                return data.Execute(command, target);
            });
        }

        public int Delete(T target, DataContext dataContext = null)
        {
            return ExecuteOnDataContext(dataContext, data =>
            {
                var command = String.Format(DeleteSql, GetTableName(), _primaryKey.Name, data.Provider.ParameterIdentifier);
                return data.Execute(command, target);
            });
        }

        private int ExecuteOnDataContext(DataContext dataContext, Func<DataContext, int> function)
        {
            var data = dataContext ?? CreateDataContext();

            try
            {
                return function(data);
            }
            finally
            {
                if (dataContext == null) data.Dispose();
            }
        }

        private string GetTableName()
        {
            return _customTableName ?? _runtimeObject.TypeName;
        }

        private DataContext CreateDataContext()
        {
            return new DataContext(Transaction.No);
        }

        private string GetCommaFields(DataContext context)
        {
            var fields = _runtimeObject.GetFields(removePrimaryKey: _primaryKey.Name);
            var formatedFields = fields.Select(f => String.Format(context.Provider.FieldFormat, f));
            return String.Join(", ", formatedFields);
        }
    }
}
