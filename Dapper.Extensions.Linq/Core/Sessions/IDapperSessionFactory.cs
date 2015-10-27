using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public interface IDapperSessionFactory
    {
        /// <summary>
        /// Open sessions for data contexts of all loaded entties. It just resolves connection string and returns the session.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, IDapperSession> OpenAndBind();
    }
}
