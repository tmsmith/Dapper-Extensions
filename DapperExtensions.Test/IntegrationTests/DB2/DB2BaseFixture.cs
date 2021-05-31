#if NETCORE
using Dapper;
using DapperExtensions.Sql;
using IBM.Data.DB2.Core;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.BC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace DapperExtensions.Test.IntegrationTests.DB2
{
    [NonParallelizable]
    public class DB2BaseFixture : DatabaseTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            var connection = new DB2Connection(ConnectionString("DB2"));

            CommonSetup(connection, new DB2Dialect());

            ExecuteScripts(Db.Connection, false, "DropAnimalTable", "DropFooTable", "DropMultikeyTable", "DropPersonTable", "DropCarTable");
            ExecuteScripts(Db.Connection, true, CreateTableScripts);
        }
    }
}
#endif