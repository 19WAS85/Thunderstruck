using System;
using System.Collections.Generic;
using System.Linq;
using Thunderstruck.Runtime;
using Thunderstruck.Provider;

namespace Thunderstruck
{
    public class DataObjectCommand<T> where T : new()
    {
        private const string InsertSql = "INSERT INTO {0} ({1}) VALUES ({2})";
        private const string UpdateSql = "UPDATE {0} SET {1} WHERE {2} = {3}{2}";
        private const string DeleteSql = "DELETE FROM {0} Where {1} = {2}{1}";

        private readonly DataRuntimeObject<T> _runtimeObject;
        private readonly string _customTableName;

        public DataObjectCommand(string tableName = null)
        {
            _runtimeObject = new DataRuntimeObject<T>();
            _customTableName = tableName;
        }

        public int Insert(T target, DataContext dataContext = null)
        {
            var data = dataContext ?? CreateDataContext();
            var fields = _runtimeObject.GetCommaFields();
            var parameters = _runtimeObject.CreateCommaParameters(data.Provider.ParameterIdentifier);
            var command = String.Format(InsertSql, GetTableName(), fields, parameters);

            var identity = data.ExecuteGetIdentity(command, target);
            _runtimeObject.GetPrimaryKey().SetValue(target, identity, null);

            return identity;
        }

        public int Update(T target, DataContext dataContext = null)
        {
            var data = dataContext ?? CreateDataContext();
            var fields = _runtimeObject.CreateCommaFieldsAndParameters(data.Provider.ParameterIdentifier);
            var primaryKey = _runtimeObject.GetPrimaryKey();
            var command = String.Format(UpdateSql, GetTableName(), fields, primaryKey.Name, data.Provider.ParameterIdentifier);
            return data.Execute(command, target);
        }

        public int Delete(T target, DataContext dataContext = null)
        {
            var data = dataContext ?? CreateDataContext();
            var primaryKey = _runtimeObject.GetPrimaryKey();
            var command = String.Format(DeleteSql, GetTableName(), primaryKey.Name, data.Provider.ParameterIdentifier);
            return data.Execute(command, target);
        }

        private string GetTableName()
        {
            return _customTableName ?? _runtimeObject.TypeName;
        }

        private DataContext CreateDataContext()
        {
            return new DataContext(Transaction.No);
        }
    }
}
