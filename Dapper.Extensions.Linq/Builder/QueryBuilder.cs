using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper.Extensions.Linq.Core;
using Dapper.Extensions.Linq.Core.Builder;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Predicates;

namespace Dapper.Extensions.Linq.Builder
{
    public class QueryBuilder<T> : IQueryBuilder<T> where T : class, IEntity
    {
        private readonly PredicateGroup _predicateGroup;

        public QueryBuilder(GroupOperator groupOperator)
        {
            _predicateGroup = new PredicateGroup
            {
                Operator = groupOperator,
                Predicates = new List<IPredicate>()
            };
        }

        private IQueryBuilder<T> Find<TValue>(Expression<Func<T, TValue>> expression, object value, Operator @operator, bool not)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null || memberExpression.Expression.NodeType != ExpressionType.Parameter)
                throw new InvalidOperationException("Doing something fancy?");

            _predicateGroup.Predicates.Add(new FieldPredicate<T> { Not = not, Operator = @operator, PropertyName = memberExpression.Member.Name, Value = value });
            return this;
        }

        public IQueryBuilder<T> Equals<TValue>(Expression<Func<T, TValue>> expression, TValue value) { return Find(expression, value, Operator.Eq, false); }
        public IQueryBuilder<T> NotEquals<TValue>(Expression<Func<T, TValue>> expression, TValue value) { return Find(expression, value, Operator.Eq, true); }
        public IQueryBuilder<T> LessThan<TValue>(Expression<Func<T, TValue>> expression, TValue value) { return Find(expression, value, Operator.Lt, false); }
        public IQueryBuilder<T> GreaterThan<TValue>(Expression<Func<T, TValue>> expression, TValue value) { return Find(expression, value, Operator.Gt, false); }
        public IQueryBuilder<T> In<TValue>(Expression<Func<T, TValue>> expression, params TValue[] values)
        {
            return Find(expression, values, Operator.Eq, false);
        }

        public IQueryBuilder<T> Like<TValue>(Expression<Func<T, TValue>> expression, TValue likeValue) { return Find(expression, likeValue, Operator.Like, false); }

        public IQueryBuilder<T> SubQuery(IQueryBuilder<T> queryBuilder)
        {
            if (queryBuilder == null)
                return this;

            var predicateGroup = queryBuilder.GetPredicate();
            if (predicateGroup == null)
                return this;

            if (_predicateGroup.Predicates == null)
                _predicateGroup.Predicates = new List<IPredicate>();

            _predicateGroup.Predicates.Add(predicateGroup);

            return this;
        }

        public IPredicateGroup GetPredicate() { return _predicateGroup; }

        public static IPredicateGroup FromExpression(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) return null;
            return PredicateConverter.Convert(predicate);
        }

        private sealed class PredicateConverter
        {
            private PredicateConverter()
            {
            }

            public static PredicateGroup Convert(Expression expression)
            {
                if (expression.NodeType == ExpressionType.Lambda)
                    expression = ((LambdaExpression)expression).Body;

                var converter = new PredicateConverter();
                var result = converter.Parse(expression);

                var group = result as PredicateGroup;
                if (group == null)
                    group = new PredicateGroup { Predicates = new List<IPredicate> { result } };

                return group;
            }

            private IPredicate ParseGroup(BinaryExpression expression)
            {
                var group = new PredicateGroup { Predicates = new List<IPredicate>(2) };
                switch (expression.NodeType)
                {
                    case ExpressionType.AndAlso:
                        group.Operator = GroupOperator.And;
                        break;
                    case ExpressionType.OrElse:
                        group.Operator = GroupOperator.Or;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                PredicateGroup g;
                var left = this.Parse(expression.Left);
                var right = this.Parse(expression.Right);

                g = left as PredicateGroup;
                if (g != null && g.Operator == group.Operator && g.Predicates != null)
                {
                    foreach (var predicate in g.Predicates)
                        group.Predicates.Add(predicate);
                }
                else if (g == null || g.Predicates != null)
                    group.Predicates.Add(left);

                g = right as PredicateGroup;
                if (g != null && g.Operator == group.Operator && g.Predicates != null)
                {
                    foreach (var predicate in g.Predicates)
                        group.Predicates.Add(predicate);
                }
                else if (g == null || g.Predicates != null)
                    group.Predicates.Add(right);

                return group;
            }

            private IPredicate ParseField(BinaryExpression expression)
            {
                string propertyName;
                Expression valueExpression;

                if (TryGetField(expression.Left, out propertyName))
                    valueExpression = expression.Right;
                else if (TryGetField(expression.Right, out propertyName))
                    valueExpression = expression.Left;
                else
                    throw new NotImplementedException("Doing something fancy?");

                object value;
                if (valueExpression.NodeType == ExpressionType.Constant)
                {
                    value = ((ConstantExpression)valueExpression).Value;
                }
                else
                {
                    value = InvokeExpression(valueExpression);
                }

                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                        return new FieldPredicate<T> { Not = false, Operator = Operator.Eq, PropertyName = propertyName, Value = value };
                    case ExpressionType.NotEqual:
                        return new FieldPredicate<T> { Not = true, Operator = Operator.Eq, PropertyName = propertyName, Value = value };
                    case ExpressionType.LessThan:
                        return new FieldPredicate<T> { Not = false, Operator = Operator.Lt, PropertyName = propertyName, Value = value };
                    case ExpressionType.LessThanOrEqual:
                        return new FieldPredicate<T> { Not = false, Operator = Operator.Le, PropertyName = propertyName, Value = value };
                    case ExpressionType.GreaterThan:
                        return new FieldPredicate<T> { Not = false, Operator = Operator.Gt, PropertyName = propertyName, Value = value };
                    case ExpressionType.GreaterThanOrEqual:
                        return new FieldPredicate<T> { Not = false, Operator = Operator.Ge, PropertyName = propertyName, Value = value };
                    default:
                        throw new NotImplementedException();
                }
            }

            private IPredicate ParseUnaryNot(UnaryExpression expression)
            {
                if (expression.NodeType != ExpressionType.Not)
                    throw new InvalidOperationException();

                var predicate = this.Parse(expression.Operand);
                return VisitPredicateTree(predicate, p =>
                {
                    var g = p as PredicateGroup;
                    if (g != null)
                    {
                        g.Operator = g.Operator == GroupOperator.And ? GroupOperator.Or : GroupOperator.And;
                        return true;
                    }

                    var f = p as IFieldPredicate;
                    if (f != null)
                    {
                        f.Not = !f.Not;
                        return false;
                    }

                    throw new NotImplementedException();
                });
            }

            private IPredicate Parse(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Not:
                        return this.ParseUnaryNot((UnaryExpression)expression);

                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Equal:
                        return this.ParseField((BinaryExpression)expression);

                    case ExpressionType.OrElse:
                    case ExpressionType.AndAlso:
                        return this.ParseGroup((BinaryExpression)expression);

                    case ExpressionType.Call:
                        return this.ParseCall((MethodCallExpression)expression);

                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                    case ExpressionType.AddAssignChecked:
                    case ExpressionType.AddChecked:
                    case ExpressionType.And:
                    case ExpressionType.AndAssign:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Assign:
                    case ExpressionType.Block:
                    case ExpressionType.Coalesce:
                    case ExpressionType.Conditional:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.DebugInfo:
                    case ExpressionType.Decrement:
                    case ExpressionType.Default:
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                    case ExpressionType.Dynamic:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.ExclusiveOrAssign:
                    case ExpressionType.Extension:
                    case ExpressionType.Goto:
                    case ExpressionType.Increment:
                    case ExpressionType.Index:
                    case ExpressionType.Invoke:
                    case ExpressionType.IsFalse:
                    case ExpressionType.IsTrue:
                    case ExpressionType.Label:
                    case ExpressionType.Lambda:
                    case ExpressionType.LeftShift:
                    case ExpressionType.LeftShiftAssign:
                    case ExpressionType.ListInit:
                    case ExpressionType.Loop:
                    case ExpressionType.MemberAccess:
                    case ExpressionType.MemberInit:
                    case ExpressionType.Modulo:
                    case ExpressionType.ModuloAssign:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                    case ExpressionType.MultiplyAssignChecked:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.New:
                    case ExpressionType.NewArrayBounds:
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.OnesComplement:
                    case ExpressionType.Or:
                    case ExpressionType.OrAssign:
                    case ExpressionType.Parameter:
                    case ExpressionType.PostDecrementAssign:
                    case ExpressionType.PostIncrementAssign:
                    case ExpressionType.Power:
                    case ExpressionType.PowerAssign:
                    case ExpressionType.PreDecrementAssign:
                    case ExpressionType.PreIncrementAssign:
                    case ExpressionType.Quote:
                    case ExpressionType.RightShift:
                    case ExpressionType.RightShiftAssign:
                    case ExpressionType.RuntimeVariables:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                    case ExpressionType.SubtractAssignChecked:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Switch:
                    case ExpressionType.Throw:
                    case ExpressionType.Try:
                    case ExpressionType.TypeAs:
                    case ExpressionType.TypeEqual:
                    case ExpressionType.TypeIs:
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.Unbox:
                    default:
                        throw new NotImplementedException();

                    // currently ignored, sometime later should think about fixing...
                    case ExpressionType.Constant:
                        return new PredicateGroup();
                }
            }

            private IPredicate ParseCall(MethodCallExpression expression)
            {
                if (expression.Method.DeclaringType == typeof(QueryFunctions))
                    return ParseCallQueryFunction(expression);

                if (expression.Method.DeclaringType == typeof(System.Linq.Enumerable))
                    return ParseCallEnumerableFunction(expression);

                if (expression.Method.DeclaringType != null &&
                    expression.Method.DeclaringType.IsGenericType &&
                    expression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
                    return ParseCallListFunction(expression);

                if (expression.Method.Name == "Contains")
                    return ParseCallContains(expression);

                throw new NotImplementedException();
            }

            private IPredicate ParseCallQueryFunction(MethodCallExpression expression)
            {
                if (expression.Method.Name == "Like")
                {
                    var patternExpression = expression.Arguments[0] as ConstantExpression;
                    var memberExpression = SimplifyExpression(expression.Arguments[1]) as MemberExpression;

                    if (patternExpression == null)
                        throw new NotImplementedException();
                    if (memberExpression == null || memberExpression.Expression.NodeType != ExpressionType.Parameter)
                        throw new NotImplementedException();

                    return new FieldPredicate<T> { Not = false, Operator = Operator.Like, PropertyName = memberExpression.Member.Name, Value = patternExpression.Value };
                }

                throw new NotImplementedException();
            }

            private IPredicate ParseCallContains(MethodCallExpression expression)
            {
                var patternExpression = expression.Arguments[0] as ConstantExpression;
                var memberExpression = expression.Object as MemberExpression;

                if (patternExpression == null)
                    throw new NotImplementedException();

                if (memberExpression == null || memberExpression.Expression.NodeType != ExpressionType.Parameter)
                    throw new NotImplementedException();

                return new FieldPredicate<T>
                {
                    Not = false,
                    Operator = Operator.Like,
                    PropertyName = memberExpression.Member.Name,
                    Value = string.Format("%{0}%", patternExpression.Value)
                };
            }

            private IPredicate ParseCallEnumerableFunction(MethodCallExpression expression)
            {
                if (expression.Method.Name == "Contains")
                {
                    var valueExpression = expression.Arguments[0] as MemberExpression;
                    var memberExpression = SimplifyExpression(expression.Arguments[1]) as MemberExpression;

                    if (valueExpression == null)
                        throw new NotImplementedException();

                    if (memberExpression == null)
                        throw new NotImplementedException();

                    string propertyName = Nullable.GetUnderlyingType(memberExpression.Expression.Type) == null ?
                        memberExpression.Member.Name :
                        ((MemberExpression)memberExpression.Expression).Member.Name;

                    return new FieldPredicate<T> { Not = false, Operator = Operator.Eq, PropertyName = propertyName, Value = InvokeExpression(valueExpression) };
                }

                throw new NotImplementedException();
            }

            private IPredicate ParseCallListFunction(MethodCallExpression expression)
            {
                if (expression.Method.Name == "Contains")
                {
                    var valueExpression = expression.Arguments[0] as MemberExpression;
                    var memberExpression = SimplifyExpression(expression.Object) as MemberExpression;

                    if (valueExpression == null)
                        throw new NotImplementedException();

                    if (memberExpression == null)
                        throw new NotImplementedException();

                    string propertyName = Nullable.GetUnderlyingType(valueExpression.Expression.Type) == null ?
                        valueExpression.Member.Name :
                        ((MemberExpression)valueExpression.Expression).Member.Name;

                    return new FieldPredicate<T>
                    {
                        Operator = Operator.Eq,
                        PropertyName = propertyName,
                        Value = InvokeExpression(memberExpression)
                    };
                }

                throw new NotImplementedException();
            }
        }

        private static IPredicate VisitPredicateTree(IPredicate predicate, Func<IPredicate, bool> callback)
        {
            if (!callback(predicate))
                return predicate;

            var group = (IPredicateGroup)predicate;
            foreach (var p in group.Predicates)
                VisitPredicateTree(p, callback);

            return predicate;
        }

        private static Expression SimplifyExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                    return ((UnaryExpression)expression).Operand;
                default:
                    return expression;
            }
        }

        private static object InvokeExpression(Expression expression)
        {
            return Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke();
        }

        private static bool TryGetField(Expression expression, out string name)
        {
            expression = SimplifyExpression(expression);
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = expression as MemberExpression;
                if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                {
                    name = memberExpression.Member.Name;
                    return true;
                }
            }

            name = null;
            return false;
        }
    }
}