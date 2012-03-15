using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace Thunderstruck
{
    public static class DataExtensions
    {
        #region Utils

        public static PropertyInfo[] GetValidPropertiesOf(Type type)
        {
            var ignore = typeof(DataIgnoreAttribute);

            return type
                .GetProperties()
                .Where(p =>
                    p.PropertyType.Name != "DataCommands`1" &&
                    p.PropertyType.Name != "DataQueryObject`1" &&
                    p.GetCustomAttributes(ignore, false).Length == 0)
                .ToArray();
        }

        #endregion

        #region SqlDataReader

        public static T[] ToArray<T>(this SqlDataReader reader) where T : new()
        {
            var list = new List<T>();
            var properties = typeof(T).GetProperties();
            var readerFields = GetReaderFields(reader);

            while (reader.Read())
            {
                var item = new T();

                foreach (var field in readerFields)
                {
                    var property = properties.FirstOrDefault(p => p.Name == field);
                    if (property == null) continue;

                    try
                    {
                        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        object safeValue = (reader[field] == null) ? null : Convert.ChangeType(reader[field], propertyType);
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

            reader.Close();

            return list.ToArray();
        }

        private static string[] GetReaderFields(SqlDataReader reader)
        {
            var fields = new String[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++) fields[i] = reader.GetName(i);

            return fields;
        }

        #endregion

        #region SqlCommand

        public static void AddParameters(this SqlCommand command, object objectParameters)
        {
            var dictionary = objectParameters as Dictionary<string, object> ?? CreateDictionary(objectParameters);

            foreach (var item in dictionary)
            {
                var parameterName = String.Concat("@", item.Key);

                if (command.CommandText.Contains(parameterName))
                {
                    var parameterValue = item.Value ?? DBNull.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }
            }
        }

        private static Dictionary<string, object> CreateDictionary(object objectParameters)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var p in GetValidPropertiesOf(objectParameters.GetType()))
            {
                dictionary.Add(p.Name, p.GetValue(objectParameters, null));
            }

            return dictionary;
        }

        #endregion

        #region DataContext

        public static T[] All<T>(this DataContext data, string query, object queryParams = null) where T : new()
        {
            return data.Query(query, queryParams).ToArray<T>();
        }

        public static T First<T>(this DataContext data, string query, object queryParams = null) where T : new()
        {
            return data.All<T>(query, queryParams).FirstOrDefault();
        }

        #endregion
    }
}