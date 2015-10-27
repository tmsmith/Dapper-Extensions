using System;

namespace Dapper.Extensions.Linq.Core.Context
{
    public class DataContextAttribute : Attribute
    {
        public string ConnectionStringName { get; private set; }

        public DataContextAttribute(string connectionStringName) { ConnectionStringName = connectionStringName; }
    }
}
