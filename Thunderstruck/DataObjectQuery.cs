using System;
using System.Linq;

namespace Thunderstruck
{
    public class DataObjectQuery<T> where T : new()
    {
        private string _customProjection;

        public DataObjectQuery() { }

        public DataObjectQuery(string projection)
        {
            _customProjection = String.Format(projection, GetTypeFields(typeof(T)));
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
            var fields = GetTypeFields(targetType);
            var tableName = targetType.Name;

            return String.Format("{0} FROM {1}", fields, tableName);
        }

        private string GetTypeFields(Type targetType)
        {
            return String.Join(", ", DataExtensions.GetValidPropertiesOf(targetType).Select(p => p.Name));
        }

        private T[] Execute(string query, object queryParams = null)
        {
            using (var data = new DataContext(Transaction.No)) return data.All<T>(query, queryParams);
        }
    }
}
