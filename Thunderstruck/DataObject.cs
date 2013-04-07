using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thunderstruck.Runtime;

namespace Thunderstruck
{
    public class DataObject<T> where T : new()
    {
        private string _projection;
        private string _table;
        private string _primaryKey;

        public DataObject(string table = null, string primaryKey = null)
        {
            _table = table;
            _primaryKey = primaryKey;
            _projection = CreateProjection(table);
        }

        public DataObjectQuery<T> Select
        {
            get { return new DataObjectQuery<T>(_projection); }
        }

        public void Insert(T target, DataContext context = null)
        {
            CreateDataCommand().Insert(target, context);
        }

        public void Update(T target, DataContext context = null)
        {
            CreateDataCommand().Update(target, context);
        }

        public void Delete(T target, DataContext context = null)
        {
            CreateDataCommand().Delete(target, context);
        }

        private string CreateProjection(string table)
        {
            if (table == null) return null;
            else return String.Concat("{0} FROM ", table);
        }

        private DataObjectCommand<T> CreateDataCommand()
        {
            return new DataObjectCommand<T>(_table, _primaryKey);
        }
    }
}