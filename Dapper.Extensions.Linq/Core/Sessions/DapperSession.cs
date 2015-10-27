using System.Data;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public class DapperSession : IDapperSession
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        public IDbConnection Connection
        {
            get { return _connection; }
        }

        public IDbTransaction Transaction
        {
            get { return _transaction; } set { _transaction = value; } }

        public DapperSession(IDbConnection connection)
        {
            this._connection = connection;
        }

        #region IDbConnection Members

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            _transaction = _connection.BeginTransaction(il);
            return _transaction;
        }

        public IDbTransaction BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        public void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            _connection.Close();
        }

        public string ConnectionString
        {
            get
            {
                return _connection.ConnectionString;
            }
            set
            {
                _connection.ConnectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get { return _connection.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        public string Database
        {
            get { return _connection.Database; }
        }

        public void Open()
        {
            _connection.Open();
        }

        public ConnectionState State
        {
            get { return _connection.State; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _connection.Dispose();
        }

        #endregion
    }
}
