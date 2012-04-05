using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Thunderstruck.Runtime
{
    internal static class DataHelpers
    {
        internal static T[] DataReaderToObjectArray<T>(IDataReader reader) where T : new()
        {
            try
            {
                var list = new List<T>();
                var properties = typeof(T).GetProperties();
                var readerFields = GetDataReaderFields(reader);

                while (reader.Read())
                {
                    var item = new T();

                    foreach (var field in readerFields)
                    {
                        var property = properties.FirstOrDefault(p => p.Name.ToUpper() == field.ToUpper());
                        if (property == null || !property.CanWrite) continue;

                        try
                        {
                            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            object safeValue = (reader[field] == null || reader[field] is DBNull) ? null : Convert.ChangeType(reader[field], propertyType);
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

                return list.ToArray();
            }
            finally
            {
                reader.Close();
            }
        }

        internal static T[] DataReaderToPrimaryArray<T>(IDataReader reader)
        {
            try
            {
                var list = new List<T>();
                while (reader.Read())
                {
                    list.Add(CastTo<T>(reader[0]));
                }
                return list.ToArray();
            }
            finally
            {
                reader.Close();
            }
        }

        internal static string[] GetDataReaderFields(IDataReader reader)
        {
            var fields = new String[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++) fields[i] = reader.GetName(i);

            return fields;
        }

        internal static T CastTo<T>(object value)
        {
            if (value is DBNull) return default(T);
            else return (T) Convert.ChangeType(value, typeof(T));
        }
    }
}