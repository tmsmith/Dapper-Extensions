using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dapper;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Sql;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Sqlite
{
    public class SqliteBaseFixture
    {
        protected IDatabase Db;

        [SetUp]
        public virtual void Setup()
        {
            string connectionString = string.Format("Data Source=.\\dapperTest_{0}.sqlite", Guid.NewGuid());
            string[] connectionParts = connectionString.Split(';');
            string file = connectionParts
                .ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1])
                .Where(d => d.Key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                .Select(k => k.Value).Single();

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            SQLiteConnection connection = new SQLiteConnection(connectionString);
            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new SqliteDialect());
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new Database(connection, sqlGenerator);

            var files = new List<string>
                                {
                                    ReadScriptFile("CreateAnimalTable"),
                                    ReadScriptFile("CreateFooTable"),
                                    ReadScriptFile("CreateMultikeyTable"),
                                    ReadScriptFile("CreatePersonTable"),
                                    ReadScriptFile("CreateCarTable")
                                };

            foreach (var setupFile in files)
            {
                connection.Execute(setupFile);
            }
        }

        [TearDown]
        public void TearDown()
        {
            string databaseName = Db.Connection.Database;
            if (!File.Exists(databaseName))
            {
                return;
            }

            int i = 10;
            while (IsDatabaseInUse(databaseName) && i > 0)
            {
                i--;
                Thread.Sleep(1000);
            }

            if (i > 0)
            {
                File.Delete(databaseName);
            }
        }

        public static bool IsDatabaseInUse(string databaseName)
        {
            FileStream fs = null;
            try
            {
                FileInfo fi = new FileInfo(databaseName);
                fs = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public string ReadScriptFile(string name)
        {
            string fileName = GetType().Namespace + ".Sql." + name + ".sql";
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}