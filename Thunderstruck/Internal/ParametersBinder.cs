using System;
using System.Collections.Generic;
using System.Data;

namespace Thunderstruck.Internal
{
    internal class ParametersBinder
    {
        internal ParametersBinder(string parameterIdentifier, object objectParameters)
        {
            ParameterIdentifier = parameterIdentifier;
            ObjectParameters = objectParameters;
        }

        internal string ParameterIdentifier { get; private set; }

        internal object ObjectParameters { get; private set; }

        internal void Bind(IDbCommand command)
        {
            var dictionary = ObjectParameters as Dictionary<string, object> ?? DataHelpers.CreateDictionary(ObjectParameters);

            foreach (var item in dictionary)
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
    }
}
