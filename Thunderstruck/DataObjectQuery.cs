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

        public T First(string where = null, object queryParams = null)
        {
            return Top(1, where, queryParams).FirstOrDefault();
        }

        public IList<T> Top(int count, string where = null, object queryParams = null)
        {
            var query = String.Format("SELECT TOP {0} {1} {2}", count, ProjectionMask, where);

            return Execute(query, queryParams);
        }

        public IList<T> All(string where = null, object queryParams = null)
        {
            var query = String.Format("SELECT {0} {1}", where, ProjectionMask);

            return Execute(query, queryParams);
        }

        public DataObjectQuery<T> With(DataContext dataContext)
        {
            DataContext = dataContext;
            return this;
        }

        private IList<T> Execute(string query, object queryParams = null)
        {
            var dataContext = DataContext ?? new DataContext(Transaction.No);

            var finalQuery = query.Replace(ProjectionMask, GetProjection(dataContext));

            try
            {
                return dataContext.All<T>(finalQuery, queryParams);
            }
            finally
            {
                if (DataContext == null) dataContext.Dispose();
            }
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
            var fields = _runtimeObject.GetFields(includePrimaryKey: true);
            var formatedFields = fields.Select(f => String.Format(context.Provider.FieldFormat, f));
            return String.Join(", ", formatedFields);
        }
    }
}
