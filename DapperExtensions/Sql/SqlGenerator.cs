using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DapperExtensions.Sql
{
    public interface ISqlGenerator
    {
        IDapperExtensionsConfiguration Configuration { get; }
        IList<IColumn> AllColumns { get; }
        IList<Table> MappedTables { get; }
        bool SupportsMultipleStatements();

        string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null);
        string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null);
        string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null);
        string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, IList<IReferenceMap> includedProperties = null);

        string Insert(IClassMapper classMap);
        string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, bool ignoreAllKeyProperties, IList<IProjection> colsToUpdate);
        string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);

        string IdentitySql(IClassMapper classMap);
        string GetTableName(IClassMapper map, bool useAlias = false);
        string GetColumnName(IClassMapper map, IMemberMap property, bool includeAlias, bool isDml = false, bool includePrefix = true);
        string GetColumnName(IClassMapper map, string propertyName, bool includeAlias, bool includePrefix = true);
        string GetColumnName(IColumn column, bool includeAlias, bool includePrefix = true);
    }

    public class SqlGeneratorImpl : ISqlGenerator
    {
        private IList<Table> Tables { get; set; }
        private int TableCount;
        private readonly IList<Table> TablesAdded = new List<Table>();
        private readonly IList<Table> TableReferencesAdded = new List<Table>();

        public IList<Table> MappedTables => Tables;

        public SqlGeneratorImpl(IDapperExtensionsConfiguration configuration)
        {
            Configuration = configuration;
            Tables = new List<Table>();
        }

        public IDapperExtensionsConfiguration Configuration { get; }

        private string GetPartitionBy()
        {
            var partitionBy = AllColumns.Where(c =>
                c.Property.KeyType == KeyType.Assigned ||
                c.Property.KeyType == KeyType.Identity ||
                c.Property.KeyType == KeyType.SequenceIdentity)
                .Select(c => c.SimpleAlias).FirstOrDefault();

            if (!string.IsNullOrEmpty(partitionBy))
            {
                var keyTable = AllColumns.Where(c => c.SimpleAlias.Equals(partitionBy)).Select(c => c.Table).First();

                return AllColumns.Where(c => (c.Property.KeyType == KeyType.Assigned || c.Property.KeyType == KeyType.Identity ||
                     c.Property.KeyType == KeyType.SequenceIdentity) && c.Table.Equals(keyTable))
                    .Select(c => c.SimpleAlias)
                    .Aggregate((prior, next) => $"{prior}, {next}");
            }
            else
            {
                return AllColumns.Select(c => c.SimpleAlias).First();
            }
        }

        private bool IsInsertable(IMemberMap member)
        {
            return !(member.Ignored ||
                member.IsReadOnly ||
                member.KeyType == KeyType.Identity ||
                member.KeyType == KeyType.TriggerIdentity);
        }

        private bool IsUpdatable(IMemberMap member, bool ignoreAllKeys)
        {
            return ignoreAllKeys ? !(member.Ignored || member.IsReadOnly) && member.KeyType == KeyType.NotAKey
                : !(member.Ignored ||
                member.IsReadOnly ||
                member.KeyType == KeyType.Identity ||
                member.KeyType == KeyType.Assigned ||
                member.KeyType == KeyType.SequenceIdentity ||
                member.KeyType == KeyType.TriggerIdentity);
        }

        public virtual string Select(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null");
            }

            MapTables(classMap);

            AllColumns = GetColumns().ToList();

            var sql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                BuildSelectColumns(classMap, colsToSelect, includedProperties),
                GetTables(classMap, parameters, includedProperties)));

            if (predicate != null)
            {
                sql.Append(" WHERE ")
                    .Append(predicate.GetSql(this, parameters));
            }

            if (sort?.Any() == true)
            {
                var orderBy = sort.Select(s =>
                {
                    var property = (s.Properties?.Count > 1) ? s.Properties?.Last() : null;
                    var type = property?.DeclaringType;

                    var map = (type != null) ? Configuration.GetMap(type) : classMap;
                    var propertyName = property?.Name ?? s.PropertyName;

                    return GetColumnName(map, propertyName, false) + (s.Ascending ? " ASC" : " DESC");
                }).AppendStrings();

                sql.Append(" ORDER BY ").Append(orderBy);
            }

            return sql.ToString();
        }

        public virtual string SelectPaged(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null)
        {
            if (sort?.Any() != true)
            {
                throw new ArgumentNullException(nameof(Sort), $"{nameof(Sort)} cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null");
            }

            var innerSql = new StringBuilder(Select(classMap, predicate, sort, parameters, colsToSelect, includedProperties));

            var partitionBy = GetPartitionBy();

            return Configuration.Dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters, partitionBy);
        }

        public virtual string SelectSet(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDictionary<string, object> parameters, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null)
        {
            if (sort?.Any() != true)
            {
                throw new ArgumentNullException(nameof(Sort), $"{nameof(Sort)} cannot be null or empty.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            var innerSql = new StringBuilder($"SELECT {BuildSelectColumns(classMap, colsToSelect, includedProperties)} FROM {GetTables(classMap, parameters, includedProperties)}");

            if (predicate != null)
            {
                innerSql.Append(" WHERE ")
                    .Append(predicate.GetSql(this, parameters));
            }

            var orderBy = sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
            innerSql.Append(" ORDER BY ").Append(orderBy);

            return Configuration.Dialect.GetSetSql(innerSql.ToString(), firstResult, maxResults, parameters);
        }

        public virtual string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, IList<IReferenceMap> includedProperties = null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            var sql = new StringBuilder();

            if (includedProperties?.Count > 0 && Configuration.Dialect.SupportsCountOfSubquery)
            {
                var countSql = new StringBuilder();
                var resultSet = BuildSelectColumns(classMap, null, includedProperties);

                sql.Append("SELECT ").Append(resultSet).Append(" FROM ").Append(GetTables(classMap, parameters, includedProperties));

                if (predicate != null)
                {
                    sql.Append(" WHERE ")
                        .Append(predicate.GetSql(this, parameters));
                }

                var partitionBy = GetPartitionBy();

                countSql.Append("SELECT ")
                    .Append(partitionBy)
                    .Append(", COUNT(*) OVER(PARTITION BY ")
                    .Append(partitionBy)
                    .Append(" ORDER BY ")
                    .Append(partitionBy)
                    .Append(") AS QTDPARTIONED FROM (")
                    .Append(sql)
                    .Append(") GROUP BY ")
                    .Append(partitionBy);

                return Configuration.Dialect.GetCountSql($"({countSql})");
            }
            else
            {
                sql.Append(GetTables(classMap, parameters, includedProperties));

                if (predicate != null)
                {
                    sql.Append(" WHERE ")
                        .Append(predicate.GetSql(this, parameters));
                }

                return Configuration.Dialect.GetCountSql(sql.ToString());
            }
        }

        public virtual string Insert(IClassMapper classMap)
        {
            MapTables(classMap);

            var i = 0;
            AllColumns = GetColumns().ToList();

            AllColumns = AllColumns.Select(c => new Column
            {
                Alias = c.Alias,
                ClassMapper = c.ClassMapper,
                Property = c.Property,
                SimpleAlias = $"{Configuration.Dialect.ParameterPrefix}i_{i++}",
                TableIdentity = c.TableIdentity,
                Table = c.Table
            }).ToList<IColumn>();

            var parameters = AllColumns
                .Where(p => IsInsertable(p.Property))
                .Select(col => col.SimpleAlias)
                .ToList();

            if (parameters.Count == 0)
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var columnNames = AllColumns
                .Where(p => IsInsertable(p.Property))
                .Select(p => GetColumnName(p, false, false)).ToList();

            var sql = $"INSERT INTO {GetTableName(classMap)} ({columnNames.AppendStrings()}) VALUES ({parameters.AppendStrings()})";

            var triggerIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.TriggerIdentity).ToList();
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

            if (triggerIdentityColumn.Count > 0)
            {
                if (triggerIdentityColumn.Count > 1)
                    throw new ArgumentException("TriggerIdentity generator cannot be used with multi-column keys");

                sql += $" RETURNING {triggerIdentityColumn.Select(p => GetColumnName(classMap, p, false, includePrefix: false)).First()} INTO {Configuration.Dialect.ParameterPrefix}IdOutParam";
            }

            return sql;
        }

        public virtual string Update(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters, bool ignoreAllKeyProperties, IList<IProjection> colsToUpdate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), $"{nameof(predicate)} cannot be null.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            MapTables(classMap);

            var i = 0;
            AllColumns = GetColumns().ToList();

            AllColumns = AllColumns.Select(c => new Column
            {
                Alias = c.Alias,
                ClassMapper = c.ClassMapper,
                Property = c.Property,
                SimpleAlias = $"{Configuration.Dialect.ParameterPrefix}u_{i++}",
                TableIdentity = c.TableIdentity,
                Table = c.Table
            }).ToList<IColumn>();

            var columns = AllColumns.Where(p => IsUpdatable(p.Property, ignoreAllKeyProperties));

            if (!columns.Any())
            {
                throw new ArgumentException("No columns were mapped.");
            }

            var setSql = columns
                .Where(c => colsToUpdate == null || colsToUpdate?.Any(cu => cu.PropertyName.Equals(c.Property.ColumnName, StringComparison.OrdinalIgnoreCase)) == true)
                .Select(p => $"{GetColumnName(p, false, false)} = {p.SimpleAlias}");

            return $"UPDATE {GetTableName(classMap)} SET {setSql.AppendStrings()} WHERE {predicate.GetSql(this, parameters, true)}";
        }

        public virtual string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), $"{nameof(predicate)} cannot be null.");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            var sql = new StringBuilder($"DELETE FROM {GetTableName(classMap)}");
            sql.Append(" WHERE ").Append(predicate.GetSql(this, parameters, true));
            return sql.ToString();
        }

        public virtual string IdentitySql(IClassMapper classMap)
        {
            return Configuration.Dialect.GetIdentitySql(GetTableName(classMap));
        }

        public virtual string GetReferenceKey(IMemberMap map)
        {
            return $"{map.ClassMapper.TableName}.{map.ColumnName}";
        }

        private static IMemberMap GetPropertyMap(IClassMapper mainMap, MemberInfo propertyInfo)
        {
            if (!mainMap.Properties.Any(p => p.MemberInfo == propertyInfo))
                throw new KeyNotFoundException($"The property {propertyInfo.Name} was not found in {mainMap.EntityType.Name} entity");
            return mainMap
                .Properties
                .Where(p => p.MemberInfo == propertyInfo)
                .Select(propertyMap => propertyMap)
                .Single();
        }

        private string GetJointTables(IClassMapper mainMap, Table table, IDictionary<string, object> parameters, IList<IReferenceMap> includedProperties = null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            var result = new StringBuilder();
            var joins = new StringBuilder();
            var sql = new StringBuilder();
            var parent = Tables.Single(t => t.Identity == table.ParentIdentity);
            JoinType joinType;
            IPredicateGroup predicate = null;
            var useIncludedProperties = includedProperties?.Any(r => r.PropertyInfo == table.PropertyInfo);

            if (useIncludedProperties == true)
            {
                var map = includedProperties.FirstOrDefault(r => r.PropertyInfo.PropertyType == table.PropertyInfo.PropertyType && r.Identity == table.Identity);

                joinType = map.JoinType;
                predicate = map.JoinPredicate;
            }
            else
            {
                joinType = mainMap.References
                    .Where(r => r.ParentIdentity == mainMap.Identity && r.PropertyInfo == table.PropertyInfo && r.EntityType == table.EntityType)
                    .Select(r => r.JoinType)
                    .SingleOrDefault();
            }

            sql.Append(' ').Append(Enum.GetName(typeof(JoinType), joinType)).Append(" join ").Append(table.Name).Append(' ').Append(table.Alias).Append(" on ");

            joins.AppendLine(sql.ToString());

            var leftClasMap = Configuration.GetMap(table.EntityType);
            var rightClasMap = mainMap;

            var properties = mainMap
               .References
               .Where(r => r.ParentIdentity == mainMap.Identity && r.PropertyInfo == table.PropertyInfo)
              .SelectMany(c => c.ReferenceProperties)
              .ToList();

            var isFirstComparison = true;

            if (properties.Count > 0)
            {
                foreach (var property in properties)
                {
                    var leftPropertyMap = GetPropertyMap(leftClasMap, property.LeftProperty.PropertyInfo);
                    var rightPropertyMap = GetPropertyMap(rightClasMap, property.RightProperty.PropertyInfo);

                    sql.Clear();
                    sql.Append(isFirstComparison ? " " : " and ");
                    sql.Append(' ').Append(table.Alias).Append('.').Append(leftPropertyMap.ColumnName)
                        .Append(' ').Append(property.ComparatorSignal).Append(' ').Append(parent.Alias).Append('.').Append(rightPropertyMap.ColumnName);
                    joins.AppendLine(sql.ToString());
                    isFirstComparison = false;
                }
            }

            if (predicate != null)
            {
                joins.Append(" AND ").AppendLine(predicate.GetSql(this, parameters));
            }

            result.Append(joins);
            return result.ToString();
        }

        public virtual string GetAllJointTables(IClassMapper mainMap, string mainTable, IDictionary<string, object> parameters, string referenceName = "", IList<IReferenceMap> includedProperties = null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            var result = new StringBuilder();
            var main = Tables.Where(t => t.Identity == t.ParentIdentity).ToList();
            var dependents = Tables.Where(t => t.Identity != t.ParentIdentity).ToList();

            foreach (var dependent in dependents)
            {
                var parentEntity = Tables
                    .Where(t => t.Identity == dependent.ParentIdentity)
                    .Select(c => c.EntityType)
                    .Single();

                var map = Configuration.GetMap(parentEntity);

                result.AppendLine(GetJointTables(map, dependent, parameters, includedProperties));

                var sqlInjectionDictionary = Configuration.GetOrSetSqlInjection(map.EntityType);
                var sqlInjection = GetJoinFromSqlInjection(sqlInjectionDictionary);

                if (sqlInjectionDictionary != null && !result.ToString().Contains(sqlInjection))
                {
                    result.AppendLine(sqlInjection);
                }
            }

            return result.ToString();
        }

        private string GetJoinFromSqlInjection(SqlInjection sqlInjection)
        {
            if (sqlInjection == null)
                return "";
            var map = Configuration.GetMap(sqlInjection.EntityType);
            var sql = sqlInjection.Sql;

            var columName = map.Properties
               .Where(p => p.Name.Equals(sqlInjection.Property, StringComparison.InvariantCultureIgnoreCase))
               .Select(c => c.ColumnName)
               .FirstOrDefault();
            return string.Format(sql, GetAliasFromTableName(map.Identity) + "." + columName);
        }

        public virtual string GetTables(IClassMapper map, IDictionary<string, object> parameters, IList<IReferenceMap> includedProperties = null)
        {
            var _includeRelacionalEntities = includedProperties?.Count > 0;

            if (parameters == null && _includeRelacionalEntities)
            {
                throw new ArgumentNullException(nameof(parameters), $"{nameof(parameters)} cannot be null.");
            }

            var tableName = new StringBuilder();

            var mainTableName = GetTableName(map, true);
            var joints = _includeRelacionalEntities ? GetAllJointTables(map, map.TableName, parameters, includedProperties: includedProperties) : "";
            var sqlInjection = GetJoinFromSqlInjection(Configuration.GetOrSetSqlInjection(map.EntityType));

            tableName.AppendLine(mainTableName);

            if (!string.IsNullOrEmpty(joints))
                tableName.AppendLine(joints);

            if (!tableName.ToString().Contains(sqlInjection))
                tableName.AppendLine(sqlInjection);

            return tableName.ToString();
        }

        public virtual string GetTableName(IClassMapper map, bool useAlias = false)
        {
            return Configuration.Dialect.GetTableName(map.SchemaName, map.TableName, useAlias ? GetAliasFromTableName(map.Identity) : null);
        }

        public virtual string GetTableName(IClassMapper map, IColumn column)
        {
            if (column.Table == null)
            {
                throw new KeyNotFoundException("Table column not set.");
            }

            return column.Table.Alias;
        }

        public virtual string GetColumnName(IColumn column, bool includeAlias, bool includePrefix = true)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column), $"{nameof(column)} cannot be null.");
            }

            var alias = includeAlias ? column.SimpleAlias : null;
            var prefix = includePrefix ? GetTableName(column.ClassMapper, column) : null;

            return Configuration.Dialect.GetColumnName(prefix, column.Property.ColumnName, alias);
        }

        public virtual string GetColumnName(IClassMapper map, IMemberMap property, bool includeAlias, bool isDml = false, bool includePrefix = true)
        {
            if (isDml)
                return Configuration.Dialect.GetColumnName(GetTableName(map), property.ColumnName, "");

            if (AllColumns?.Any(c => c.Property == property) == true)
            {
                foreach (var c in AllColumns.Where(c => c.TableIdentity == map.Identity))
                {
                    c.ClassMapper.GetType().GetProperty("SimpleAlias").SetValue(c.ClassMapper, GetTableName(c.ClassMapper, c), null);
                }
            }

            var alias = (property.ColumnName != property.Name && includeAlias) ? property.Name : null;
            var prefix = includePrefix ? (!string.IsNullOrEmpty(map.SimpleAlias) ? map.SimpleAlias : GetTableName(map)) : null;

            return Configuration.Dialect.GetColumnName(prefix, property.ColumnName, alias);
        }

        public virtual string GetColumnName(IClassMapper map, string propertyName, bool includeAlias, bool includePrefix = true)
        {
            var propertyMap = map?.Properties?.SingleOrDefault(p => propertyName.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (propertyMap == null)
            {
                throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
            }

            return GetColumnName(map, propertyMap, includeAlias, false, includePrefix);
        }

        public virtual bool SupportsMultipleStatements()
        {
            return Configuration.Dialect.SupportsMultipleStatements;
        }

        public virtual IList<IColumn> AllColumns { get; private set; }

        public Table GetMappedTables(Type entityType, Type parentEntityType = null)
        {
            return Tables.Single(t => t.ParentEntityType == parentEntityType
                                  && t.EntityType == entityType);
        }

        public Table GetMappedTables(Guid identity, Guid parentIdentity)
        {
            return Tables.Single(t => t.Identity.Equals(identity)
                                  && t.ParentIdentity.Equals(parentIdentity));
        }

        private IClassMapper GetVirtualClassMapper(IClassMapper mapper)
        {
            var assemblyBuilder = ReflectionHelper.CreateAssemblyBuilder(mapper.EntityType.Assembly.GetName().Name);
            var moduleBuilder = ReflectionHelper.CreateModuleBuilder(assemblyBuilder, "VirtualModules.dll");

            var tbEntity = ReflectionHelper.CreateTypeBuilder(moduleBuilder, mapper.EntityType.Name, mapper.EntityType);
            var virtualEntity = ReflectionHelper.CreateVirtualType(tbEntity, mapper.EntityType);

            var tbMapper = ReflectionHelper.CreateTypeBuilder(moduleBuilder, mapper.EntityType.Name + "Map", mapper.GetType());
            ReflectionHelper.CreateMapType(tbMapper, virtualEntity, mapper.GetType());

            var virtualMapInstance = Configuration.GetMap(virtualEntity);

            object[] argsP = { virtualEntity };
            virtualMapInstance.GetType().GetMethod("SetEntityType").Invoke(virtualMapInstance, argsP);

            return virtualMapInstance;
        }

        private IClassMapper CreateVirtualClassMap(Type entityType, Guid parentIdentity)
        {
            var originalMap = Configuration.GetMap(entityType);
            var virtualClassMapper = GetVirtualClassMapper(originalMap);

            object[] argsP = { parentIdentity };
            virtualClassMapper.GetType().GetMethod("SetParentIdentity").Invoke(virtualClassMapper, argsP);

            return virtualClassMapper;
        }

        private IReferenceMap GetIdentityFromIncludedProperties(IList<IReferenceMap> includedProperties, PropertyInfo property, Guid parentIdentity)
        {
            return includedProperties.FirstOrDefault(p => p.ParentIdentity == parentIdentity && p.PropertyInfo == property &&
                      !TablesAdded.Any(t => t.Identity == p.Identity));
        }

        private static void SetReferencePropertiesParentIdentity(IClassMapper mapper, Guid identity)
        {
            mapper.References.ToList().ForEach(e =>
            {
                e.SetParentIdentity(identity);
                e.ReferenceProperties.ToList().ForEach(r => r.SetParentIdentity(identity));
            });
        }

        private IClassMapper GetVirtualReferenceMap(ref Table table, bool isVirtual, Guid parentIdentity, Type parentType, IList<IReferenceMap> includedProperties)
        {
            if (isVirtual)
            {
                var identity = TablesAdded.Where(c => c.Identity == parentIdentity && c.IsVirtual).ToList();
                if (identity.Count > 0)
                    parentIdentity = identity.Select(i => i.Identity).Last();

                table.LastIdentity = table.Identity;

                var virtualReferenceMap = CreateVirtualClassMap(parentType, parentIdentity);

                //Set new identity to virtual map
                var newIdentity = GetIdentityFromIncludedProperties(includedProperties, table.PropertyInfo, table.ParentIdentity);

                if (newIdentity != null)
                {
                    virtualReferenceMap.SetIdentity(newIdentity.Identity);
                    SetReferencePropertiesParentIdentity(virtualReferenceMap, newIdentity.Identity);
                }

                table.Identity = virtualReferenceMap.Identity;
                table.ParentIdentity = virtualReferenceMap.ParentIdentity;
                table.EntityType = virtualReferenceMap.EntityType;

                return virtualReferenceMap;
            }
            else
                return null;
        }

        private void ProcessRelationationalIdentities(ref IClassMapper mapper, ref IClassMapper parent, PropertyInfo propertyInfo, IList<IReferenceMap> includedProperties)
        {
            if (includedProperties?.Count > 0)
                if (mapper.Identity == parent.Identity)
                {
                    var parentIdentity = includedProperties[0].ParentIdentity;
                    mapper.SetIdentity(parentIdentity);
                    mapper.SetParentIdentity(parentIdentity);

                    parent.SetIdentity(parentIdentity);
                    parent.SetParentIdentity(parentIdentity);

                    SetReferencePropertiesParentIdentity(mapper, parentIdentity);
                    SetReferencePropertiesParentIdentity(parent, parentIdentity);
                }
                else
                {
                    var parentIdentity = parent.Identity;
                    var childIdentityFromIncluded = includedProperties.FirstOrDefault(i => TablesAdded.Any(a => a.Identity == i.ParentIdentity) &&
                        i.ParentIdentity == parentIdentity && i.PropertyInfo == propertyInfo);

                    if (childIdentityFromIncluded != null)
                    {
                        mapper.SetIdentity(childIdentityFromIncluded.Identity);
                        SetReferencePropertiesParentIdentity(mapper, childIdentityFromIncluded.Identity);
                    }
                }
        }

        private IList<Table> ProcessReference(IReferenceMap reference, IClassMapper mapper, IClassMapper parent, IClassMapper virtualReferenceMap, IList<IReferenceMap> includedProperties)
        {
            IClassMapper getTopParentMap(IClassMapper virtualMap, IReferenceMap reference) =>
            (virtualMap != null
            && includedProperties.Any(i => i.ParentIdentity == virtualMap.Identity && i.PropertyInfo == reference.PropertyInfo))
            ? virtualMap : mapper;

            var tables = new List<Table>();

            var map = Configuration.GetMap(reference.EntityType);

            var isVirtual = TablesAdded.Any(a => a.Identity == map.Identity && a.ParentIdentity == parent.Identity);
            if (!isVirtual || (isVirtual && !TablesAdded.Any(a => a.ParentIdentity == map.Identity && a.IsVirtual)))
            {
                var topParentParam = getTopParentMap(virtualReferenceMap, reference);
                tables.AddRange(GetAllMappedTables(map, mapper, reference.PropertyInfo, isVirtual, includedProperties));
            }

            return tables;
        }

        private IList<Table> ProcessReferences(IClassMapper mapper, IClassMapper parent, IClassMapper virtualReferenceMap, IList<IReferenceMap> includedProperties)
        {
            IEnumerable<IReferenceMap> getReferences(IClassMapper mapper) =>
             mapper.References.Where(r =>
                        (r.ParentIdentity == mapper.Identity)
                        && includedProperties.Any(a => a.PropertyInfo == r.PropertyInfo && a.ParentIdentity == mapper.Identity));

            var tables = new List<Table>();

            if (includedProperties?.Count > 0)
                foreach (var reference in getReferences(mapper))
                    tables.AddRange(ProcessReference(reference, mapper, parent, virtualReferenceMap, includedProperties));

            return tables;
        }

        private IList<Table> GetAllMappedTables(IClassMapper parentClassMapper, IClassMapper topParentMap, PropertyInfo propertyInfo, bool isVirtualMap = false, IList<IReferenceMap> includedProperties = null)
        {
            var tables = new List<Table>();
            var _table = new Table();

            //Set new Identity and Parent Identity to most top map.
            ProcessRelationationalIdentities(ref parentClassMapper, ref topParentMap, propertyInfo, includedProperties);

            TableCount++;
            _table = new Table
            {
                Alias = "y_" + TableCount,
                EntityType = parentClassMapper.EntityType,
                Name = parentClassMapper.TableName,
                ReferenceName = "",
                Identity = parentClassMapper.Identity,
                ParentIdentity = topParentMap.Identity,
                IsVirtual = isVirtualMap,
                PropertyInfo = propertyInfo,
                ClassMapper = parentClassMapper
            };

            /** Creates a virtual mapping for nested references in the current ClassMapper **/
            IClassMapper virtualReferenceMap = GetVirtualReferenceMap(ref _table, isVirtualMap, topParentMap.Identity, parentClassMapper.EntityType, includedProperties);

            if (parentClassMapper.Identity == topParentMap.Identity || includedProperties.Any(a => a.Identity == _table.Identity))
            {
                tables.Add(_table);
                TablesAdded.Add(_table);

                tables.AddRange(ProcessReferences(parentClassMapper, topParentMap, virtualReferenceMap, includedProperties));
            }
            return tables;
        }
        public void MapTables(IClassMapper classMap, IList<IReferenceMap> includedProperties = null)
        {
            Tables = new List<Table>();
            TableCount = 0;
            TablesAdded.Clear();
            TableReferencesAdded.Clear();

            Tables = GetAllMappedTables(classMap, classMap, null, false, includedProperties).ToList();
        }

        public virtual string BuildSelectColumns(IClassMapper classMap, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null)
        {
            AllColumns = new List<IColumn>();
            MapTables(classMap, includedProperties);

            var i = 0;
            AllColumns = GetColumns().Where(p => !(p.Property.Ignored || p.Property.IsReadOnly)).ToList();

            AllColumns = AllColumns.Select(c => new Column
            {
                Alias = c.Alias,
                ClassMapper = c.ClassMapper,
                Property = c.Property,
                SimpleAlias = $"c_{i++}",
                TableIdentity = c.TableIdentity,
                Table = c.Table
            }).ToList<IColumn>();

            var columns = AllColumns
                .Where(col => !col.Property.Ignored && (colsToSelect == null || colsToSelect?.Any(c => c.PropertyName.Equals(col.Property.ColumnName, StringComparison.OrdinalIgnoreCase)) == true))
                .Select(col => GetColumnName(col, true));

            var result = columns.AppendStrings();
            return string.IsNullOrEmpty(result) ? throw new NotSupportedException("Query with empty ClassMapper is not supported.") : result;
        }

        private string GetAliasFromTableName(Guid identity)
        {
            return Tables
                .Where(x => x.Identity == identity)
                .Select(s => s.Alias)
                .FirstOrDefault();
        }

        public string GetReference(Table table, string parentReference = "")
        {
            var _reference = "";

            if (table.PropertyInfo != null && table.ParentIdentity != table.Identity)
            {
                _reference = (!string.IsNullOrEmpty(parentReference)) ? table.PropertyInfo.Name + "_" + parentReference : table.PropertyInfo.Name;
                var parentTable = Tables.Single(t => t.Identity == table.ParentIdentity);
                var refResult = GetReference(parentTable, parentReference);
                return !string.IsNullOrEmpty(refResult) ? refResult + "_" + _reference : _reference;
            }
            return _reference;
        }

        private IEnumerable<IColumn> GetColumns(Table table)
        {
            var reference = GetReference(table);

            string getParentReference(IMemberMap map) => map.ParentProperty != null ? map.ParentProperty.Name : string.Empty;

            var map = table.ClassMapper ?? Configuration.GetMap(table.EntityType);
            var columnIndex = 0;

            return map?.Properties?
                .Where(p => (map?.References == null || map?.References?.Any(r => r.PropertyInfo?.Name == p.Name) == false) && map?.Properties?.Any(mp => mp.ParentProperty == p) == false)
                .Select(m =>
                {
                    var alias = !string.IsNullOrEmpty(reference) ? reference + getParentReference(m) + "_" + m.Name : getParentReference(m) + m.Name;
                    return new Column(string.IsNullOrEmpty(alias) ? $"Column{++columnIndex}" : alias, m, m.ClassMapper, table);
                })
                .ToList() ?? new List<Column>();
        }

        public IEnumerable<IColumn> GetColumns()
        {
            var columns = new List<IColumn>();

            foreach (var table in Tables)
                columns.AddRange(GetColumns(table));

            return columns;
        }

        public static IList<IReferenceProperty> GetReferenceProperties(IClassMapper map)
        {
            return
                map.References
                   .SelectMany(c => c.ReferenceProperties)
                   .Select(r => r)
                   .Distinct()
                   .ToList();
        }
    }
}