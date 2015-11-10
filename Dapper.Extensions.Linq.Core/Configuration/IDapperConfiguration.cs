using System;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Sessions;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public interface IDapperConfiguration
    {
        IDapperConfiguration UseContainer<T>(Action<IContainerCustomisations> customisations) where T : IContainer;
        IDapperConfiguration UsingConnectionProvider<T>(string name) where T : IConnectionStringProvider;
        IDapperConfiguration FromAssembly(string name);
        IDapperConfiguration FromAssemblyContaining(Type assemblyType);
        IDapperConfiguration UseClassMapper(Type typeOfMapper);
        IDapperConfiguration UseSqlDialect(ISqlDialect dialect);
        void Build();

        Type DefaultMapper { get; }
        ISqlDialect Dialect { get; }
        IClassMapper GetMap(Type entityType);
        IClassMapper GetMap<T>() where T : class;
        Guid GetNextGuid();
    }
}