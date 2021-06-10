using DapperExtensions.Predicate;
using System;
using System.ComponentModel;

namespace DapperExtensions.Extensions
{
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var memInfo = value.GetType().GetMember(value.ToString());

            if (memInfo?.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs?.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return value.ToString();
        }

        public static string GetString(this Operator op, bool not)
        {
            return op switch
            {
                Operator.Gt => not ? "<=" : ">",
                Operator.Ge => not ? "<" : ">=",
                Operator.Lt => not ? ">=" : "<",
                Operator.Le => not ? ">" : "<=",
                Operator.Like => not ? "NOT LIKE" : "LIKE",
                Operator.Contains => not ? "NOT IN" : "IN",
                _ => not ? "<>" : "=",
            };
        }
    }
}
