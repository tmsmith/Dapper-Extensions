using System;
using System.Linq.Expressions;

namespace Dapper.Extensions.Linq.Extensions
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }

        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, expr2.Body), expr1.Parameters[0]);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, expr2.Body), expr1.Parameters[0]);
        }
    }
}
