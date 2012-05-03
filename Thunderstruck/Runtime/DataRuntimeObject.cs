using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Thunderstruck.Runtime
{
    public class DataRuntimeObject<T>
    {
        private static readonly Type Ignore = typeof(IgnoreAttribute);
        private static readonly Type _type = typeof(T);
        private static IList<PropertyInfo> _allProperties;
        private static IList<PropertyInfo> _validProperties;
        private static PropertyInfo _primaryKey;

        public string TypeName
        {
            get { return _type.Name; }
        }

        public IList<PropertyInfo> GetProperties()
        {
            if (_allProperties == null)
            {
                _allProperties = _type.GetProperties();
            }

            return _allProperties;
        }

        public IList<PropertyInfo> GetValidProperties()
        {
            if (_validProperties == null)
            {
                var properties = GetProperties();
                _validProperties = properties.Where(p => IsValidProperty(p)).ToList();
            }

            return _validProperties;
        }

        public PropertyInfo GetPrimaryKey(string primaryKeyName)
        {
            if (_primaryKey == null)
            {
                var validProperties = GetValidProperties();
                if (primaryKeyName != null) return validProperties.FirstOrDefault(p => p.Name == primaryKeyName);
                _primaryKey = validProperties.FirstOrDefault(p => p.Name == "Id") ?? validProperties.First();
            }
            
            return _primaryKey;
        }

        public IList<string> GetFields(string removePrimaryKey)
        {
            var fields = GetValidProperties().ToList();
            if (removePrimaryKey != null) fields.Remove(GetPrimaryKey(removePrimaryKey));
            return fields.Select(p => p.Name).ToList();
        }

        public IList<string> CreateParameters(string paramIdentifier, string primaryKey)
        {
            var fields = GetFields(removePrimaryKey: primaryKey);
            return fields.Select(f => String.Concat(paramIdentifier, f)).ToList();
        }

        public string CreateCommaParameters(string paramIdentifier, string primaryKey)
        {
            return Comma(CreateParameters(paramIdentifier, primaryKey));
        }

        public string CreateCommaFieldsAndParameters(string paramIdentifier, string primaryKey)
        {
            var fields = GetFields(removePrimaryKey: primaryKey);
            return Comma(fields.Select(f => CreateFieldAndParameter(f, paramIdentifier)));
        }

        private bool IsValidProperty(PropertyInfo property)
        {
            var type = property.PropertyType;
            return !type.IsInterface && NotIsDataObject(type) && NotIsIgnored(property);
        }

        private bool NotIsDataObject(Type type)
        {
            return type.Name != "DataObjectCommand`1" && type.Name != "DataObjectQuery`1";
        }

        private bool NotIsIgnored(PropertyInfo property)
        {
            return property.GetCustomAttributes(Ignore, false).Length == 0;
        }

        private string CreateFieldAndParameter(string field, string paramIdentifier)
        {
            return String.Concat(field, " = ", paramIdentifier, field);
        }

        private string Comma(IEnumerable<string> list)
        {
            return String.Join(", ", list);
        }
    }
}