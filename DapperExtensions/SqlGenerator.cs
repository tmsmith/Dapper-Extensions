using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperExtensions
{
    internal interface ISqlGenerator
    {
        string Get(IClassMapper classMap);
        string Insert(IClassMapper classMap);
        string Update(IClassMapper classMap);
        string Delete(IClassMapper classMap);
        string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);
        string GetList(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters);
        string GetPage(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters);
        string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);
        string IdentitySql(IClassMapper classMap);
        string GetTableName(IClassMapper map);
        string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias);
        string GetColumnName(IClassMapper map, string propertyName, bool includeAlias);
    }

    internal class SqlGeneratorImpl : ISqlGenerator
    {
        private readonly ISqlDialect _dialect;

        public SqlGeneratorImpl(ISqlDialect dialect)
        {
            _dialect = dialect;
        }

        public string Get(IClassMapper classMap)
        {
            if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            return string.Format("SELECT {0} FROM {1} WHERE {2}",
                BuildSelectColumns(classMap),
                GetTableName(classMap),
                BuildWhere(classMap));
        }

        public string Insert(IClassMapper classMap)
        {
            if (classMap.Properties.Count(c => c.KeyType == KeyType.Identity) > 1)
            {
                throw new ArgumentException("Can only set 1 property to Identity.");
            }

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
            var columnNames = columns.Select(p => GetColumnName(classMap, p, false));
            var parameters = columns.Select(p => "@" + p.Name);

            return string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                 GetTableName(classMap),
                                 columnNames.AppendStrings(),
                                 parameters.AppendStrings());
        }

        public string Update(IClassMapper classMap)
        {
            if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
            var setSql = columns.Select(p => GetColumnName(classMap, p, false) + " = @" + p.Name);
            return string.Format("UPDATE {0} SET {1} WHERE {2}",
                GetTableName(classMap),
                setSql.AppendStrings(),
                BuildWhere(classMap));
        }

        public string Delete(IClassMapper classMap)
        {
            if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            return string.Format("DELETE FROM {0} WHERE {1}",
                GetTableName(classMap),
                BuildWhere(classMap));
        }

        public string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            StringBuilder sql = new StringBuilder(string.Format("DELETE FROM {0}", GetTableName(classMap)));
            if (predicate != null)
            {
                sql.Append(" WHERE ")
                    .Append(predicate.GetSql(parameters));
            }

            return sql.ToString();
        }

        public string GetList(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters)
        {
            StringBuilder sql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                BuildSelectColumns(classMap),
                GetTableName(classMap)));
            if (predicate != null)
            {
                sql.Append(" WHERE ")
                    .Append(predicate.GetSql(parameters));
            }

            if (sort != null && sort.Any())
            {
                sql.Append(" ORDER BY ")
                    .Append(sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings());
            }

            return sql.ToString();
        }

        public string GetPage(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters)
        {
            if (sort == null || !sort.Any())
            {
                throw new ArgumentException("Sort must be supplied for GetPage.");
            }

            StringBuilder innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                BuildSelectColumns(classMap),
                GetTableName(classMap)));
            if (predicate != null)
            {
                innerSql.Append(" WHERE ")
                    .Append(predicate.GetSql(parameters));
            }

            string orderBy = sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
            innerSql.Append(" ORDER BY " + orderBy);

            string sql = _dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters);
            return sql;
        }

        public string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            StringBuilder sql = new StringBuilder(string.Format("SELECT COUNT(*) AS [Total] FROM {0}",
                                GetTableName(classMap)));
            if (predicate != null)
            {
                sql.Append(" WHERE ")
                    .Append(predicate.GetSql(parameters));
            }

            return sql.ToString();
        }

        public string IdentitySql(IClassMapper classMap)
        {
            return _dialect.GetIdentitySql(GetTableName(classMap));
        }

        public string GetTableName(IClassMapper map)
        {
            return _dialect.GetTableName(map.SchemaName, map.TableName, null);
        }

        public string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
        {
            string alias = null;
            if (property.ColumnName != property.Name && includeAlias)
            {
                alias = property.Name;
            }

            return _dialect.GetColumnName(GetTableName(map), property.ColumnName, alias);
        }

        public string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
        {
            IPropertyMap propertyMap = map.Properties.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (propertyMap == null)
            {
                throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
            }

            return GetColumnName(map, propertyMap, includeAlias);
        }

        private string BuildSelectColumns(IClassMapper classMap)
        {
            var columns = classMap.Properties.Where(p => !p.Ignored).Select(p => GetColumnName(classMap, p, true));
            return columns.AppendStrings();
        }

        private string BuildWhere(IClassMapper classMap)
        {
            var where = classMap.Properties
                .Where(p => p.KeyType != KeyType.NotAKey)
                .Select(p => GetColumnName(classMap, p, false) + " = @" + p.Name);
            return where.AppendStrings(" AND ");
        }
    }
}