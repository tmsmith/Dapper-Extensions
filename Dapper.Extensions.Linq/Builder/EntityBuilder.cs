using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Extensions.Linq.Core;
using Dapper.Extensions.Linq.Core.Builder;

namespace Dapper.Extensions.Linq.Builder
{
    sealed class EntityBuilder<T> : IEntityBuilder<T> where T : class, IEntity
    {
        private readonly Func<IEnumerable<T>> _entityResolver;
        private IEnumerable<T> _items;

        public EntityBuilder(Func<IEnumerable<T>> entityResolver)
        {
            _entityResolver = entityResolver;
        }

        private IEnumerable<T> ResolveEnities()
        {
            return _items ?? (_items = _entityResolver());
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
            return AsEnumerable().ToList();
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
    }
}