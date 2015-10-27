using System.Data;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public interface IDapperSession : IDbConnection
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; set; }
    }
}
