using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Predicate
{
    public interface IExistsPredicate : IPredicate
    {
        IPredicate Predicate { get; set; }
        bool Not { get; set; }
    }

    public class ExistsPredicate<TSub> : IExistsPredicate
        where TSub : class
    {
        public IPredicate Predicate { get; set; }
        public bool Not { get; set; }

        public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var mapSub = GetClassMapper(typeof(TSub), sqlGenerator.Configuration);
            var sql = string.Format("({0}EXISTS (SELECT 1 FROM {1} WHERE {2}))",
                Not ? "NOT " : string.Empty,
                sqlGenerator.GetTableName(mapSub),
                Predicate.GetSql(sqlGenerator, parameters, isDml));
            return sql;
        }

        protected virtual IClassMapper GetClassMapper(Type type, IDapperExtensionsConfiguration configuration)
        {
            var map = configuration.GetMap(type);
            if (map == null)
            {
                throw new NullReferenceException(string.Format("Map was not found for {0}", type));
            }

            return map;
        }
    }
}
