using System;
using System.Linq;
using Thunderstruck.Runtime;
using System.Collections.Generic;

namespace Thunderstruck
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

        public IList<T> All(string where = null, object queryParams = null)
        {
            var context = GetDataContext();
            var query = context.Provider.SelectAllQuery(ProjectionMask, where);
            return Execute(context, query, queryParams);
        }

        public IList<T> Take(int count, string where = null, object queryParams = null)
        {
            var context = GetDataContext();
            var query = context.Provider.SelectTakeQuery(ProjectionMask, where, count);
            return Execute(context, query, queryParams);
        }

        public T First(string where = null, object queryParams = null)
        {
            return Take(1, where, queryParams).FirstOrDefault();
        }

        public DataObjectQuery<T> With(DataContext dataContext)
        {
            DataContext = dataContext;
            return this;
        }

        private IList<T> Execute(DataContext context, string query, object queryParams = null)
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
