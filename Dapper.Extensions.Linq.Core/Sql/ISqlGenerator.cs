using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Predicates;

namespace Dapper.Extensions.Linq.Core.Sql
{
    public interface ISqlGenerator
    {
        IDapperConfiguration Configuration { get; }
        
        string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters);
        string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters);
        string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters);
        string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);
        string Insert(IClassMapper classMap);
        string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);
        string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);
        string IdentitySql(IClassMapper classMap);
        string GetTableName(IClassMapper map);
        string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias);
        string GetColumnName(IClassMapper map, string propertyName, bool includeAlias);
        string BuildSelectColumns(IClassMapper classMap);
        bool SupportsMultipleStatements();
    }
}