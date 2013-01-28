using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Thunderstruck.Runtime
{
    public class ParametersBinder
    {
        public ParametersBinder(string parameterIdentifier, object objectParameters)
        {
            ParameterIdentifier = parameterIdentifier;
            ObjectParameters = objectParameters;
        }

        public string ParameterIdentifier { get; private set; }

        public object ObjectParameters { get; private set; }

        public void Bind(IDbCommand command)
        {
            var values = CreateValuesList(ObjectParameters);

            foreach (var item in values)
            {
                var parameterName = String.Concat(ParameterIdentifier, item.Key);
                if (!command.CommandText.Contains(parameterName)) continue;
                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = item.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        public IList<KeyValuePair<string, object>> CreateValuesList(object target)
        {
            var dictionary = target as Dictionary<string, object>;
            if (dictionary != null) return dictionary.Select(i => i).ToList();
            var properties = target.GetType().GetProperties();
            var notIgnoredProperties = properties.Where(p => !HasIgnoreAttribute(p));
            return notIgnoredProperties.Select(p => CreateValue(target, p)).ToList();
        }

        private KeyValuePair<string, object> CreateValue(object target, PropertyInfo property)
        {
            object value;
            if (property.PropertyType.IsEnum) value = (int)property.GetValue(target, null);
            else value = property.GetValue(target, null);
            return new KeyValuePair<string, object>(property.Name, value);
        }

        private bool HasIgnoreAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(IgnoreAttribute), false).Length > 0;
        }
    }
}
