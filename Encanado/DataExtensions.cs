using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Encanado.Access
{
    public static class DataExtensions
    {
        #region SqlDataReader

        public static T[] ToArray<T>(this SqlDataReader reader) where T : new()
        {
            var list = new List<T>();
            var properties = typeof(T).GetProperties();

            while (reader.Read())
            {
                var item = new T();

                foreach (var p in properties)
                {
                    try
                    {
                        var ordinal = reader.GetOrdinal(p.Name);
                        var value = reader.GetValue(ordinal);
                        var valueTyped = Convert.ChangeType(value, p.PropertyType);

                        p.SetValue(item, valueTyped, null);
                    }
                    catch (IndexOutOfRangeException) { continue; }
                }

                list.Add(item);
            }

            reader.Close();

            return list.ToArray();
        }

        #endregion

        #region SqlCommand

        public static void AddParameters(this SqlCommand command, object objectParameters)
        {
            foreach (var p in objectParameters.GetType().GetProperties())
            {
                var parameterName = String.Concat("@", p.Name);

                if (command.CommandText.Contains(parameterName))
                {
                    var parameterValue = p.GetValue(objectParameters, null) ?? DBNull.Value;

                    command.Parameters.AddWithValue(parameterName, parameterValue);
                }
            }
        }

        #endregion

        #region DataContext

        public static T[] All<T>(this DataContext data, string query, object queryParams = null) where T : new()
        {
            return data.Query(query, queryParams).ToArray<T>();
        }

        public static T First<T>(this DataContext data, string query, object queryParams = null) where T : new()
        {
            return data.All<T>(query, queryParams).First();
        }

        #endregion
    }
}