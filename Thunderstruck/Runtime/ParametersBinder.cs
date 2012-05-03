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
                if (command.CommandText.Contains(parameterName))
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Value = item.Value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }
        }

        public IList<KeyValuePair<string, object>> CreateValuesList(object target)
        {
            var dictionary = target as Dictionary<string, object>;
            if (dictionary != null) return dictionary.Select(i => i).ToList();
            
            var validProperties = target.GetType().GetProperties();
            return validProperties.Select(p => CreateValue(target, p)).ToList();
        }

        private KeyValuePair<string, object> CreateValue(object target, PropertyInfo property)
        {
            return new KeyValuePair<string, object>(property.Name, property.GetValue(target, null));
        }
    }
}
