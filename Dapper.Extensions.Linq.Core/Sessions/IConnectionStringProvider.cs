namespace Dapper.Extensions.Linq.Core.Sessions
{
    public interface IConnectionStringProvider
    {
        string ConnectionString(string connectionStringName);
    }
}
