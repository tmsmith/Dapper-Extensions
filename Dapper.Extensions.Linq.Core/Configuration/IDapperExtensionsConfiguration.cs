using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public interface IDapperExtensionsConfiguration
    {
        Type DefaultMapper { get; }
        IList<Assembly> MappingAssemblies { get; }
        ISqlDialect Dialect { get; }
        IClassMapper GetMap(Type entityType);
        IClassMapper GetMap<T>() where T : class;
        void ClearCache();
        Guid GetNextGuid();
    }
}