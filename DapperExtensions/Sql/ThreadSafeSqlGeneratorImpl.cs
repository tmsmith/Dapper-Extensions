using System;
using System.Collections.Generic;
using System.Threading;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;

namespace DapperExtensions.Sql
{
    public class ThreadSafeSqlGeneratorImpl : ISqlGenerator
    {

        [ThreadStatic]
        private static SqlGeneratorImpl _sqlGeneratorImpl;

        private SqlGeneratorImpl Current => LazyInitializer.EnsureInitialized(ref _sqlGeneratorImpl, () => new SqlGeneratorImpl(Configuration));
        
        public ThreadSafeSqlGeneratorImpl(IDapperExtensionsConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IDapperExtensionsConfiguration Configuration { get; }

        public IList<IColumn> AllColumns => Current.AllColumns;

        public IList<Table> MappedTables => Current.MappedTables;

        public bool SupportsMultipleStatements()
        {
            return this.Configuration.Dialect.SupportsMultipleStatements;
        }


        public string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort,
            IDictionary<string, object> parameters, IList<IProjection> colsToSelect,
            IList<IReferenceMap> includedProperties = null)
        {
            return this.Current.Select(classMap, predicate, sort, parameters, colsToSelect, includedProperties);
        }

        public string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page,
            int resultsPerPage,
            IDictionary<string, object> parameters, IList<IProjection> colsToSelect,
            IList<IReferenceMap> includedProperties = null)
        {
            return this.Current.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters,
                    colsToSelect, includedProperties);
           
        }

        public string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult,
            int maxResults,
            IDictionary<string, object> parameters, IList<IProjection> colsToSelect,
            IList<IReferenceMap> includedProperties = null)
        {
            return this.Current.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters,
                    colsToSelect, includedProperties);
          
        }

        public string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters,
            IList<IReferenceMap> includedProperties = null)
        {
            return this.Current.Count(classMap, predicate, parameters, includedProperties);
        }

        public string Insert(IClassMapper classMap)
        {
            return this.Current.Insert(classMap);
        }

        public string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters,
            bool ignoreAllKeyProperties,
            IList<IProjection> colsToUpdate)
        {
            return this.Current.Update(classMap, predicate, parameters, ignoreAllKeyProperties, colsToUpdate);
        }

        public string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            return this.Current.Delete(classMap, predicate, parameters);
        }

        public string IdentitySql(IClassMapper classMap)
        {
            return this.Current.IdentitySql(classMap);
        }

        public string GetTableName(IClassMapper map, bool useAlias = false)
        {
            return this.Current.GetTableName(map, useAlias);
        }

        public string GetColumnName(IClassMapper map, IMemberMap property, bool includeAlias, bool isDml = false,
            bool includePrefix = true)
        {
            return this.Current.GetColumnName(map, property, includeAlias, isDml, includePrefix);
        }

        public string GetColumnName(IClassMapper map, string propertyName, bool includeAlias, bool includePrefix = true)
        {
            return this.Current.GetColumnName(map, propertyName, includeAlias, includePrefix);
        }

        public string GetColumnName(IColumn column, bool includeAlias, bool includePrefix = true)
        {
            return this.Current.GetColumnName(column, includeAlias, includePrefix);
        }
    }
}