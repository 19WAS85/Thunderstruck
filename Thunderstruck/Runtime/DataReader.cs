using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Thunderstruck.Runtime
{
	public class DataReader
	{
		private readonly IDataReader _dataReader;

		public DataReader(IDataReader dataReader)
		{
			_dataReader = dataReader;
		}

		public IEnumerable<T> ToObjectList<T>() where T : new()
		{
			try
			{
				var list = new List<T>();
				var properties = typeof(T).GetProperties();
				var readerFields = GetFields();

				bool isDynamic = (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)));
				IDictionary<string, object> dynamicValues = null;

				while (_dataReader.Read())
				{
					var item = new T();

					foreach (var field in readerFields)
					{
						if (isDynamic)
						{
							if (dynamicValues == null)
							{
								dynamicValues = (IDictionary<string, object>)item;
							}

							dynamicValues[field] = GetSafeValue(_dataReader[field].GetType(), field);

							continue;
						}

						var property = properties.FirstOrDefault(p => p.Name.ToUpper() == field.ToUpper());
						if (property == null || !property.CanWrite) continue;

						try
						{
							var safeValue = GetSafeValue(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType, field);
							property.SetValue(item, safeValue, null);
						}
						catch (FormatException err)
						{
							var message = String.Format("Error to convert column {0} to property {1} {2}.{3}",
								property.Name, property.PropertyType.Name, typeof(T).Name, property.Name);

							throw new FormatException(message, err);
						}
					}

					list.Add(item);
				}

				return list;
			}
			finally
			{
				_dataReader.Close();
			}
		}

		private object GetSafeValue(Type propertyType, string field)
		{
			object safeValue = null;

			if (propertyType.IsEnum)
			{
				var enumValue = Convert.ToInt32(_dataReader[field].ToString());
				safeValue = Enum.ToObject(propertyType, enumValue);
			}
			else
			{
				var isNull = _dataReader[field] == null || _dataReader[field] is DBNull;
				safeValue = (isNull) ? null : Convert.ChangeType(_dataReader[field], propertyType);
			}

			var stringSafeValue = safeValue as String;
			if (stringSafeValue != null)
			{
				safeValue = stringSafeValue.Trim();
			}

			return safeValue;
		}

		public IEnumerable<T> ToEnumerable<T>()
		{
			try
			{
				var list = new List<T>();
				while (_dataReader.Read())
				{
					list.Add(CastTo<T>(_dataReader[0]));
				}
				return list;
			}
			finally
			{
				_dataReader.Close();
			}
		}

		public string[] GetFields()
		{
			var fields = new String[_dataReader.FieldCount];
			for (int i = 0; i < _dataReader.FieldCount; i++) fields[i] = _dataReader.GetName(i);
			return fields;
		}

		public static T CastTo<T>(object value)
		{
			if (value is DBNull) return default(T);
			return (T)Convert.ChangeType(value, typeof(T));
		}
	}
}