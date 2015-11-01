using System;
using System.Configuration;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public class StaticConnectionStringProvider : IConnectionStringProvider
    {
        public string ConnectionString(string connectionStringName)
        {
            var config = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (config == null) throw new NullReferenceException(string.Format("Connection string '{0}' not found", connectionStringName));
            return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
        }
    }
}
