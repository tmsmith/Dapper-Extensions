using DapperExtensions.Sql;
using System.Collections.Generic;

namespace DapperExtensions.Predicate
{
    public interface IPropertyPredicate : IComparePredicate
    {
        string PropertyName2 { get; set; }
        bool UseRightTableAlias { get; set; }
        DatabaseFunction LeftDatabaseFunction { get; set; }
        string LeftDatabaseFunctionParameters { get; set; }
        DatabaseFunction RigthDatabaseFunction { get; set; }
        string RigthDatabaseFunctionParameters { get; set; }
    }

    public class PropertyPredicate<T, T2> : ComparePredicate, IPropertyPredicate
    {
        public string PropertyName2 { get; set; }
        public bool UseRightTableAlias { get; set; }
        public DatabaseFunction LeftDatabaseFunction { get; set; }
        public string LeftDatabaseFunctionParameters { get; set; }
        public DatabaseFunction RigthDatabaseFunction { get; set; }
        public string RigthDatabaseFunctionParameters { get; set; }

        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName, false, UseTableAlias);
            var columnName2 = GetColumnName(typeof(T2), sqlGenerator, PropertyName2, false, UseRightTableAlias);

            if (!LeftDatabaseFunction.Equals(DatabaseFunction.None))
            {
                columnName = sqlGenerator.Configuration.Dialect.GetDatabaseFunctionString(LeftDatabaseFunction, columnName, LeftDatabaseFunctionParameters);
            }

            if (!RigthDatabaseFunction.Equals(DatabaseFunction.None))
            {
                columnName2 = sqlGenerator.Configuration.Dialect.GetDatabaseFunctionString(RigthDatabaseFunction, columnName2, RigthDatabaseFunctionParameters);
            }

            return string.Format("({0} {1} {2})", columnName, GetOperatorString(), columnName2);
        }
    }
}
