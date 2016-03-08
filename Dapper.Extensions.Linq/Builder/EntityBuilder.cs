using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Linq.Core;
using Dapper.Extensions.Linq.Core.Builder;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sessions;
using Dapper.Extensions.Linq.Extensions;
using Dapper.Extensions.Linq.Predicates;

namespace Dapper.Extensions.Linq.Builder
{
    sealed class EntityBuilder<T> : IEntityBuilder<T> where T : class, IEntity
    {
        private readonly IDapperSession _session;
        private readonly Expression<Func<T, bool>> _expression;
        private readonly IList<ISort> _sort;
        private int? _take;
        private int? _timeout;

        public EntityBuilder(IDapperSession session, Expression<Func<T, bool>> expression)
        {
            _session = session;
            _expression = expression;
            _sort = new List<ISort>();
        }

        private IEnumerable<T> ResolveEnities()
        {
            IPredicateGroup predicate = QueryBuilder<T>.FromExpression(_expression);

            IPredicateGroup p = predicate?.Predicates == null ? null : predicate;
            return _session.GetList<T>(p, _sort, _session.Transaction, _timeout, false, _take);
        }

        public IEnumerable<T> AsEnumerable()
        {
            return ResolveEnities();
        }

        public bool Any()
        {
            return ResolveEnities().Any();
        }

        public IList<T> ToList()
        {
            return ResolveEnities().ToList();
        }

        public int Count()
        {
            return ResolveEnities().Count();
        }

        public T Single()
        {
            return ResolveEnities().Single();
        }

        public T SingleOrDefault()
        {
            return ResolveEnities().SingleOrDefault();
        }

        public T FirstOrDefault()
        {
            return ResolveEnities().FirstOrDefault();
        }

        public IEntityBuilder<T> OrderBy(Expression<Func<T, object>> expression)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            if (propertyInfo == null) return this;

            var sort = new Sort
            {
                PropertyName = propertyInfo.Name,
                Ascending = true
            };
            _sort.Add(sort);

            return this;
        }

        public IEntityBuilder<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            if (propertyInfo == null) return this;

            var sort = new Sort
            {
                PropertyName = propertyInfo.Name,
                Ascending = false
            };
            _sort.Add(sort);

            return this;
        }

        public IEntityBuilder<T> Take(int number)
        {
            _take = number;
            return this;
        }

        /// <summary>
        /// Timeouts cannot be specified for SqlCe, these will remain zero.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IEntityBuilder<T> Timeout(int timeout)
        {
            _timeout = timeout;
            return this;
        }
    }
}