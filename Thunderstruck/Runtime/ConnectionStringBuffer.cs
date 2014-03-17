using System;
using System.Collections.Generic;
using System.Configuration;

namespace Thunderstruck.Runtime
{
    public class ConnectionStringBuffer
    {
        #region Singleton

        private static ConnectionStringBuffer _instance;

        static ConnectionStringBuffer()
        {
            _instance = new ConnectionStringBuffer();
        }

        public static ConnectionStringBuffer Instance
        {
            get { return _instance; }
        }

        #endregion

		private Dictionary<string, ConnectionStringSettings> _buffer;
		private object _syncRoot = new object();

        public ConnectionStringBuffer()
        {
            _buffer = new Dictionary<string, ConnectionStringSettings>();
        }

        public ConnectionStringSettings Get(string connectionStringName)
        {
            if (!_buffer.ContainsKey(connectionStringName))
            {
				lock (_syncRoot)
				{
					if (!_buffer.ContainsKey(connectionStringName))
					{
						_buffer.Add(connectionStringName, GetFromConfig(connectionStringName));
					}
				}
            }

            return _buffer[connectionStringName];
        }

        private ConnectionStringSettings GetFromConfig(string connectionName)
        {
            var setting = ConfigurationManager.ConnectionStrings[connectionName];

            if (setting == null)
            {
                var exceptionMessage = String.Concat("ConnectionString '", connectionName ,"' not found in config file.");
                throw new ThunderException(exceptionMessage);
            }

            return setting;
        }
    }
}