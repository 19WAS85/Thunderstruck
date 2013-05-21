using System;
using System.Collections.Generic;
using System.Linq;

namespace Thunderstruck.Runtime
{
    public class DataObjectQuery<T> where T : new()
    {
        private const string ProjectionMask = "__Projection__";

        private readonly DataRuntimeObject<T> _runtimeObject;
        private string _customProjection;

        public DataObjectQuery()
        {
            _runtimeObject = new DataRuntimeObject<T>();
        }

        public DataObjectQuery(string projection) : this()
        {
            _customProjection = projection;
        }

        public DataContext DataContext { get; private set; }

        public IEnumerable<T> All(string where, params object[] queryParams)
        {
            var context = GetDataContext();
            var query = context.Provider.SelectAllQuery(ProjectionMask, where).Trim();
            return Execute(context, query, queryParams);
        }

        public IEnumerable<T> All()
        {
            return All(null);
        }

        public IEnumerable<T> Take(int count, string where = null, params object[] queryParams)
        {
            var context = GetDataContext();
            var query = context.Provider.SelectTakeQuery(ProjectionMask, where, count).Trim();
            return Execute(context, query, queryParams);
        }

        public IEnumerable<T> Take(int count)
        {
            return Take(count, null);
        }

        public T First(string where, params object[] queryParams)
        {
            return Take(1, where, queryParams).FirstOrDefault();
        }

        public T First()
        {
            return First(null);
        }

        public DataObjectQuery<T> With(DataContext dataContext)
        {
            DataContext = dataContext;
            return this;
        }

        private IEnumerable<T> Execute(DataContext context, string query, params object[] queryParams)
        {
            var finalQuery = ResolveProjection(query, context);

            try
            {
                return context.All<T>(finalQuery, queryParams);
            }
            finally
            {
                if (DataContext == null) context.Dispose();
            }
        }

        private Thunderstruck.DataContext GetDataContext()
        {
            return DataContext ?? new DataContext(Transaction.No);
        }

        private string ResolveProjection(string query, DataContext context)
        {
            var projection = GetProjection(context);
            return query.Replace(ProjectionMask, projection);
        }

        public string GetProjection(DataContext context)
        {
            if (_customProjection != null)
            {
                return String.Format(_customProjection, GetCommaFields(context));
            }

            var targetType = typeof(T);
            var fields = GetCommaFields(context);
            var tableName = targetType.Name;

            return String.Format("{0} FROM {1}", fields, tableName);
        }

        private string GetCommaFields(DataContext context)
        {
            var fields = _runtimeObject.GetFields(removePrimaryKey: null);
            var formatedFields = fields.Select(f => String.Format(context.Provider.FieldFormat, f));
            return String.Join(", ", formatedFields);
        }
    }
}
