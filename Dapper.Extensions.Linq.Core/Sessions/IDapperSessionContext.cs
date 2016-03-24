using System;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public interface IDapperSessionContext : IDisposable
    {
        IDapperSession GetSession(Type entityType);
        IDapperSession GetSession<T>() where T : class, IEntity;
        /// <summary>
        /// Opens all session and binds them to this context
        /// </summary>
        void Bind();

        /// <summary>
        /// Unbinds and closes all associated sessions
        /// </summary>
        void Unbind();

        IEnumerable<IDapperSession> BoundedSessions { get; }
    }
}
