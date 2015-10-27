using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Dapper.Extensions.Linq.Core.Context;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public class DapperSessionFactory : IDapperSessionFactory
    {
        public const string DefaultConnectionStringProviderName = "__Default";
        private readonly IKernel _kernel;
        private readonly Assembly _assembly;

        public DapperSessionFactory(IKernel kernel, Assembly assembly)
        {
            _kernel = kernel;
            _assembly = assembly;
        }

        public IDictionary<string, IDapperSession> OpenAndBind()
        {
            IDictionary<string, IDapperSession> result = new Dictionary<string, IDapperSession>();
            var conStrings = GetAllLoadedContexts();

            foreach (var conString in conStrings)
            {
                var provider = GetCsProvider(conString);
                var connection = new SqlConnection(provider.ConnectionString(conString));
                connection.Open();

                var session = new DapperSession(connection);
                result.Add(conString, session);
            }
            return result;
        }

        private IList<string> _loadedContexts;

        private IList<string> GetAllLoadedContexts()
        {

            if (_loadedContexts == null)
            {
                var entityType = typeof(IEntity);

                _loadedContexts = _assembly
                    .GetTypes()
                    .Where(t => t.IsClass && entityType.IsAssignableFrom(t))
                    .Select(t =>
                    {
                        var attribute = t.GetCustomAttribute<DataContextAttribute>(true);

                        if(attribute == null) return DefaultConnectionStringProviderName;
                        return attribute.ConnectionStringName;
                    })
                    .Distinct()
                    .ToList();
            }

            return _loadedContexts;
        }

        [DebuggerStepThrough]
        internal IConnectionStringProvider GetCsProvider(string csName)
        {
            IConnectionStringProvider result;
            try
            {
                result = _kernel.Resolve<IConnectionStringProvider>(csName);
            }
            catch (ComponentNotFoundException)
            {
                result = _kernel.Resolve<IConnectionStringProvider>(DefaultConnectionStringProviderName);

            }

            return result;
        }
    }
}
