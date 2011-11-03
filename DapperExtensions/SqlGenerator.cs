using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DapperExtensions
{
    public static class SqlGenerator
    {
        public static string Get(IClassMapper classMap)
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

        public static string Insert(IClassMapper classMap)
        {
            if (classMap.Properties.Count(c => c.KeyType == KeyType.Identity) > 1)
            {
                throw new ArgumentException("Can only set 1 property to Identity.");
            }

            var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly ||  p.KeyType == KeyType.Identity));
            var columnNames = columns.Select(p => GetColumnName(classMap, p, false));
            var parameters = columns.Select(p => "@" + p.Name);

            return string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                 GetTableName(classMap),
                                 columnNames.AppendStrings(),
                                 parameters.AppendStrings());
        }

        public static string Update(IClassMapper classMap)
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

        public static string Delete(IClassMapper classMap)
        {
            if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            return string.Format("DELETE FROM {0} WHERE {1}",
                GetTableName(classMap),
                BuildWhere(classMap));
        }

        public static string GetList(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters)
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

        public static string GetPage(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters)
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
            string sql;
            if (DapperExtensions.IsUsingSqlCe)
            {
                sql = string.Format("{0} ORDER BY {1} OFFSET @pageStartRowNbr ROWS FETCH NEXT @resultsPerPage ROWS ONLY", innerSql, orderBy);
                int startValue = ((page - 1) * resultsPerPage);
                parameters.Add("@pageStartRowNbr", startValue);
                parameters.Add("@resultsPerPage", resultsPerPage);
            }
            else
            {
                var projColumns = classMap.Properties.Select(p => "proj.[" + p.Name + "]");
                sql = string.Format("SELECT {0} FROM ({1} ORDER BY {2}) proj WHERE proj.[RowNbr] BETWEEN @pageStartRowNbr AND @pageStopRowNbr ORDER BY proj.[RowNbr]",
                    projColumns.AppendStrings(), innerSql, orderBy);

                int startValue = (page * resultsPerPage) + 1;
                parameters.Add("@pageStartRowNbr", startValue);
                parameters.Add("@pageStopRowNbr", startValue + resultsPerPage);
            }

            return sql;
        }

        public static string IdentitySql(IClassMapper classMap)
        {
            if (DapperExtensions.IsUsingSqlCe)
            {
                return "SELECT @@IDENTITY AS [Id]";
            }

            return string.Format("SELECT IDENT_CURRENT('{0}') AS [Id]", GetTableName(classMap));
        }

        public static string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            return string.Format("SELECT COUNT(*) Total FROM {0} WHERE {1}",
                GetTableName(classMap), 
                predicate.GetSql(parameters));
        }

        public static string GetTableName(IClassMapper map)
        {
            string result = (string.IsNullOrWhiteSpace(map.SchemaName) ? null : "[" + map.SchemaName + "].") + "[" + map.TableName + "]";
            return result;
        }

        public static string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
        {
            string result = GetTableName(map) + ".[" + property.ColumnName + "]";
            if (property.ColumnName == property.Name || !includeAlias)
            {
                return result;
            }

            return result + " AS [" + property.Name + "]";
        }

        public static string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
        {
            IPropertyMap propertyMap = map.Properties.Where(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (propertyMap == null)
            {
                throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
            }

            return GetColumnName(map, propertyMap, includeAlias);
        }

        private static string BuildSelectColumns(IClassMapper classMap)
        {
            var columns = classMap.Properties.Where(p => !p.Ignored).Select(p => GetColumnName(classMap, p, true));
            return columns.AppendStrings();
        }

        private static string BuildWhere(IClassMapper classMap)
        {
            var where = classMap.Properties
                .Where(p => p.KeyType != KeyType.NotAKey)
                .Select(p => GetColumnName(classMap, p, false) + " = @" + p.Name);
            return where.AppendStrings(" AND ");
        }

        private static string AppendStrings(this IEnumerable<string> list, string seperator = ", ")
        {
            return list.Aggregate(
                new StringBuilder(),
                (sb, s) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(s),
                sb => sb.ToString());
        }
    }

}