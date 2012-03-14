using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thunderstruck;
using System.Reflection;
using System.Globalization;

namespace Thunderstruck
{
    public class DataQueryObject<T> where T : new()
    {
        private string _customProjection;

        public DataQueryObject() { }

        public DataQueryObject(string projection)
        {
            _customProjection = projection;
        }

        public T First(string where = null, object queryParams = null)
        {
            return Top(1, where, queryParams).FirstOrDefault();
        }

        public T[] Top(int count, string where = null, object queryParams = null)
        {
            var query = String.Format("SELECT TOP {0} {1} {2}", count, GetProjection(), where);

            return Execute(query, queryParams);
        }

        public T[] All(string where = null, object queryParams = null)
        {
            var query = String.Format("SELECT {0} {1}", GetProjection(), where);

            return Execute(query, queryParams);
        }

        public string GetProjection()
        {
            if (_customProjection != null) return _customProjection;

            var targetType = typeof(T);
            var fields = String.Join(", ", DataExtensions.GetValidPropertiesOf(targetType).Select(p => p.Name));
            var tableName = DataExtensions.Pluralization.Pluralize(targetType.Name);

            return String.Format("{0} FROM {1}", fields, tableName);
        }

        private T[] Execute(string query, object queryParams = null)
        {
            using (var data = new DataContext(Transaction.No)) return data.All<T>(query, queryParams);
        }
    }
}
