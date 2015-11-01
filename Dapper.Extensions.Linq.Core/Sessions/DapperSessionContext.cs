using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Attributes;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public class DapperSessionContext : IDapperSessionContext
    {
        private IDictionary<string, IDapperSession> _sessionDictionary;

        private readonly IDapperSessionFactory _factory;

        public DapperSessionContext(IDapperSessionFactory factory)
        {
            _factory = factory;
            Bind();
        }

        public IDapperSession GetSession<TEntity>() where TEntity : class, IEntity
        {
            return GetSession(typeof(TEntity));
        }

        public IDapperSession GetSession(Type entityType)
        {
            if (_sessionDictionary == null)
                throw new InvalidOperationException("No sessions bound");

            var attribute = entityType.GetCustomAttribute<DataContextAttribute>();
            string sessionName;
            if (attribute == null)
            {
                sessionName = DapperSessionFactory.DefaultConnectionStringProviderName;
            }
            else sessionName = attribute.ConnectionStringName;

            IDapperSession session;
            if (_sessionDictionary.TryGetValue(sessionName, out session))
                return session;

            throw new Exception(string.Format("Session for entity {0} not found.", entityType.Name));
        }

        public void Dispose()
        {
            if (_sessionDictionary != null)
                Unbind();
        }

        public void Bind()
        {
            if (_sessionDictionary != null)
                return;

            _sessionDictionary = _factory.OpenAndBind();
        }

        public void Unbind()
        {
            if (_sessionDictionary == null)
                return;

            List<string> keys = _sessionDictionary.Keys.ToList();

            foreach (var conString in keys)
            {
                IDapperSession session = _sessionDictionary[conString];
                if (session.Transaction != null)
                {
                    session.Transaction.Rollback();
                    session.Transaction.Dispose();
                    session.Transaction = null;
                }
                session.Close();
                session.Dispose();
            }

            _sessionDictionary = null;
        }

        public IEnumerable<IDapperSession> BoundedSessions => _sessionDictionary?.Values.ToList() ?? new List<IDapperSession>();
    }
}
