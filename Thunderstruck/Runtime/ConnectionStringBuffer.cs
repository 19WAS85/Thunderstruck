using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public ConnectionStringBuffer()
        {
            _buffer = new Dictionary<string, ConnectionStringSettings>();
        }

        public ConnectionStringSettings Get(string connectionStringName)
        {
            if (!_buffer.ContainsKey(connectionStringName))
            {
                _buffer.Add(connectionStringName, GetFromConfig(connectionStringName));
            }
            return _buffer[connectionStringName];
        }

        private ConnectionStringSettings GetFromConfig(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName];
        }
    }
}