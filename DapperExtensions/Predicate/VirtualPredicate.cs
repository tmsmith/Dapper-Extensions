using DapperExtensions.Extensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Predicate
{
    public interface IVirtualPredicate : IPredicate
    {
        Operator Operator { get; set; }
        bool Not { get; set; }
        string Comparable { get; set; }
        object Value { get; set; }
    }

    public class VirtualPredicate : IVirtualPredicate
    {
        public Operator Operator { get; set; }
        public bool Not { get; set; }
        public string Comparable { get; set; }
        public object Value { get; set; }

        public VirtualPredicate() : base()
        {
        }

        public VirtualPredicate(string comparable, Operator op, object value, bool not = false) : base()
        {
            Comparable = comparable;
            Operator = op;
            Value = value;
            Not = not;
        }

        public virtual string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var param = new Parameter
            {
                ColumnName = Comparable,
                Value = Value is Func<object> ? (Value as Func<object>).Invoke() : Value,
                Name = Comparable
            };

            var valueParameter = parameters.SetParameterName(param, sqlGenerator.Configuration.Dialect.ParameterPrefix);
            return $"({Comparable} {Operator.GetString(Not)} {valueParameter})";
        }
    }
}
