using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using Microsoft.Extensions.ObjectPool;

namespace DapperExtensions.Sql
{
    public class ThreadSafeSqlGeneratorImpl : ISqlGenerator
    {
        private readonly ObjectPool pool;

        public ThreadSafeSqlGeneratorImpl(IDapperExtensionsConfiguration configuration)
        {
            this.Configuration = configuration;
            this.pool = new ObjectPool(new DefaultPoolPolicy(configuration), Environment.ProcessorCount * 2);
        }

        public IDapperExtensionsConfiguration Configuration { get; }

        public IList<IColumn> AllColumns
        {
            get
            {
                var sqlGenerator = pool.Get();
                try
                {
                    return sqlGenerator.AllColumns;
                }
                finally
                {
                    pool.Return(sqlGenerator);
                }
            }
        }

        public IList<Table> MappedTables
        {
            get
            {
                var sqlGenerator = pool.Get();
                try
                {
                    return sqlGenerator.MappedTables;
                }
                finally
                {
                    pool.Return(sqlGenerator);
                }
            }
        }

        public bool SupportsMultipleStatements()
        {
            return this.Configuration.Dialect.SupportsMultipleStatements;
        }


        public string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort,
            IDictionary<string, object> parameters, IList<IProjection> colsToSelect,
            IList<IReferenceMap> includedProperties = null)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.Select(classMap, predicate, sort, parameters, colsToSelect, includedProperties);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page,
            int resultsPerPage,
            IDictionary<string, object> parameters, IList<IProjection> colsToSelect,
            IList<IReferenceMap> includedProperties = null)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters,
                    colsToSelect, includedProperties);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult,
            int maxResults,
            IDictionary<string, object> parameters, IList<IProjection> colsToSelect,
            IList<IReferenceMap> includedProperties = null)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters,
                    colsToSelect, includedProperties);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters,
            IList<IReferenceMap> includedProperties = null)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.Count(classMap, predicate, parameters, includedProperties);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string Insert(IClassMapper classMap)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.Insert(classMap);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters,
            bool ignoreAllKeyProperties,
            IList<IProjection> colsToUpdate)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties, colsToUpdate);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.Delete(classMap, predicate, parameters);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string IdentitySql(IClassMapper classMap)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.IdentitySql(classMap);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string GetTableName(IClassMapper map, bool useAlias = false)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.GetTableName(map, useAlias);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string GetColumnName(IClassMapper map, IMemberMap property, bool includeAlias, bool isDml = false,
            bool includePrefix = true)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.GetColumnName(map, property, includeAlias, isDml, includePrefix);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string GetColumnName(IClassMapper map, string propertyName, bool includeAlias, bool includePrefix = true)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.GetColumnName(map, propertyName, includeAlias, includePrefix);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }

        public string GetColumnName(IColumn column, bool includeAlias, bool includePrefix = true)
        {
            var sqlGenerator = pool.Get();
            try
            {
                return sqlGenerator.GetColumnName(column, includeAlias, includePrefix);
            }
            finally
            {
                pool.Return(sqlGenerator);
            }
        }
    }

    public sealed class DefaultPoolPolicy : PooledObjectPolicy<ISqlGenerator>
    {
        private readonly IDapperExtensionsConfiguration _configuration;

        public DefaultPoolPolicy(IDapperExtensionsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override ISqlGenerator Create()
        {
            return new SqlGeneratorImpl(_configuration);
        }

        public override bool Return(ISqlGenerator obj)
        {
            return true;
        }
    }

    public class ObjectPool
    {
        protected readonly ISqlGenerator[] _items;
        protected ISqlGenerator? _firstItem;
        private protected readonly PooledObjectPolicy<ISqlGenerator> _policy;

        public ObjectPool(PooledObjectPolicy<ISqlGenerator> policy, int maximumRetained)
        {
            this._policy = policy ?? throw new ArgumentNullException(nameof(policy));
            // -1 due to _firstItem
            _items = new ISqlGenerator[maximumRetained - 1];
        }

        public ISqlGenerator Get()
        {
            var item = _firstItem;
            if (item == null || Interlocked.CompareExchange(ref _firstItem, null, item) != item)
            {
                var items = _items;
                for (var i = 0; i < items.Length; i++)
                {
                    item = items[i];
                    if (item != null && Interlocked.CompareExchange(ref items[i], null, item) == item)
                    {
                        return item;
                    }
                }

                item = Create();
            }

            return item;
        }

        /// <inheritdoc />
        public void Return(ISqlGenerator obj)
        {
            if (_policy.Return(obj))
            {
                if (_firstItem != null || Interlocked.CompareExchange(ref _firstItem, obj, null) != null)
                {
                    var items = _items;
                    for (var i = 0;
                         i < items.Length && Interlocked.CompareExchange(ref items[i], obj, null) != null;
                         ++i)
                    {
                    }
                }
            }
        }

        // Non-inline to improve its code quality as uncommon path
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected virtual ISqlGenerator Create() => _policy.Create();
    }
}