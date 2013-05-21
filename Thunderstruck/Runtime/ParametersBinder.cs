using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Thunderstruck.Runtime
{
    public class ParametersBinder
    {
        public ParametersBinder(string parameterIdentifier, object[] objectParameters)
        {
            ParameterIdentifier = parameterIdentifier;
            ObjectParameters = objectParameters;
        }

        public string ParameterIdentifier { get; private set; }

        public object[] ObjectParameters { get; private set; }

        public void Bind(IDbCommand command)
        {
            if (ObjectParameters == null || ObjectParameters.Length == 0) return;

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

        public IEnumerable<KeyValuePair<string, object>> CreateValuesList(object[] target)
        {
            if (target.Length == 1 && IsComplexType(target.First()))
            {
                var firstItem = target.First();
                var dictionary = firstItem as Dictionary<string, object>;
                if (dictionary != null) return dictionary.Select(i => i).ToList();

                var properties = firstItem.GetType().GetProperties();
                var notIgnoredProperties = properties.Where(p => !HasIgnoreAttribute(p));
                return notIgnoredProperties.Select(p => CreateKeyValuePair(firstItem, p)).ToList();
            }
            else return target.Select((value, index) => CreateKeyValuePair(value, index));
        }

        private bool IsComplexType(object target)
        {
            return !(target.GetType().IsValueType || target is String);
        }

        private KeyValuePair<string, object> CreateKeyValuePair(object target, PropertyInfo property)
        {
            object value;
            if (property.PropertyType.IsEnum) value = (int) property.GetValue(target, null);
            else value = property.GetValue(target, null);
            return new KeyValuePair<string, object>(property.Name, value);
        }

        private KeyValuePair<string, object> CreateKeyValuePair(object value, int index)
        {
            return new KeyValuePair<string, object>(Convert.ToString(index), value);
        }

        private bool HasIgnoreAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(IgnoreAttribute), false).Length > 0;
        }
    }
}
