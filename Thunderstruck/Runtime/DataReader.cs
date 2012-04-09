using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Thunderstruck.Runtime
{
    public class DataReader
    {
        private IDataReader _dataReader;

        public DataReader(IDataReader dataReader)
        {
            _dataReader = dataReader;
        }

        public IList<T> ToObjectList<T>() where T : new()
        {
            try
            {
                var list = new List<T>();
                var properties = typeof(T).GetProperties();
                var readerFields = GetFields();

                while (_dataReader.Read())
                {
                    var item = new T();

                    foreach (var field in readerFields)
                    {
                        var property = properties.FirstOrDefault(p => p.Name.ToUpper() == field.ToUpper());
                        if (property == null || !property.CanWrite) continue;

                        try
                        {
                            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            object safeValue = (_dataReader[field] == null || _dataReader[field] is DBNull) ? null : Convert.ChangeType(_dataReader[field], propertyType);
                            property.SetValue(item, safeValue, null);
                        }
                        catch (FormatException err)
                        {
                            var message = String.Format("Erro to convert column {0} to property {1} {2}.{3}",
                                property.Name, property.PropertyType.Name, typeof(T).Name, property.Name);

                            throw new FormatException(message, err);
                        }
                    }

                    list.Add(item);
                }

                return list;
            }
            finally
            {
                _dataReader.Close();
            }
        }

        public IList<T> ToList<T>()
        {
            try
            {
                var list = new List<T>();
                while (_dataReader.Read())
                {
                    list.Add(CastTo<T>(_dataReader[0]));
                }
                return list;
            }
            finally
            {
                _dataReader.Close();
            }
        }

        public string[] GetFields()
        {
            var fields = new String[_dataReader.FieldCount];
            for (int i = 0; i < _dataReader.FieldCount; i++) fields[i] = _dataReader.GetName(i);
            return fields;
        }

        public static T CastTo<T>(object value)
        {
            if (value is DBNull) return default(T);
            else return (T) Convert.ChangeType(value, typeof(T));
        }
    }
}