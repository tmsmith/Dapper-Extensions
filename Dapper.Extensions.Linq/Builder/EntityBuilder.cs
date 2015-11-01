using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Extensions.Linq.Core;
using Dapper.Extensions.Linq.Core.Builder;

namespace Dapper.Extensions.Linq.Builder
{
    class EntityBuilder<T> : IEntityBuilder<T> where T : class, IEntity
    {
        private readonly Func<IEnumerable<T>> _entityResolver;
        private IEnumerable<T> _items;

        public EntityBuilder(Func<IEnumerable<T>> entityResolver)
        {
            _entityResolver = entityResolver;
        }

        private IEnumerable<T> ResolveEnities() { return _items ?? (_items = _entityResolver()); }

        public virtual IEnumerable<T> AsEnumerable()
        {
            return ResolveEnities();
        }

        public virtual bool Any()
        {
            return ResolveEnities().Any();
        }

        public virtual IList<T> ToList() { return AsEnumerable().ToList(); }
        public int Count()
        {
            return ResolveEnities().Count();
        }

        public virtual T Single()
        {
            return ResolveEnities().Single();
        }

        public virtual T SingleOrDefault()
        {
            return ResolveEnities().SingleOrDefault();
        }
        public virtual T FirstOrDefault()
        {
            return ResolveEnities().FirstOrDefault();
        }
    }
}