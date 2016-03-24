using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Attributes;
using Dapper.Extensions.Linq.Core.Configuration;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public class DapperSessionFactory : IDapperSessionFactory
    {
        private readonly DapperConfiguration _configuration;

        public DapperSessionFactory(DapperConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, IDapperSession> OpenAndBind()
        {
            IDictionary<string, IDapperSession> result = new Dictionary<string, IDapperSession>();
            var conStrings = GetAllLoadedContexts();

            foreach (var conString in conStrings)
            {
                IConnectionStringProvider provider = GetCsProvider(conString);
                var connection = _configuration.Dialect.GetConnection(provider.ConnectionString(conString));
                connection.Open();

                var session = new DapperSession(connection);
                result.Add(conString, session);
            }

            return result;
        }

        private IConnectionStringProvider GetCsProvider(string name)
        {
            if (_configuration.Providers.ContainsKey(name))
                return _configuration.Providers[name];

            throw new NullReferenceException(string.Format("IConnectionStringProvider '{0}' not found", name));
        }

        private IList<string> _loadedContexts;

        private IList<string> GetAllLoadedContexts()
        {

            if (_loadedContexts == null)
            {
                _loadedContexts = _configuration
                    .Assemblies
                    .SelectMany(a =>
                        a.GetTypes()
                        .Where(t => t.IsClass && typeof(IEntity).IsAssignableFrom(t))
                        .Select(t =>
                        {
                            var attribute = t.GetCustomAttribute<DataContextAttribute>(true);

                            if (attribute == null) return _configuration.DefaultConnectionStringName;
                            return attribute.ConnectionStringName;
                        }))
                        .Distinct()
                        .ToList();
            }

            return _loadedContexts;
        }
    }
}
