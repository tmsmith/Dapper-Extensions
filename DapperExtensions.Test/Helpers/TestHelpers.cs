using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Threading;
using Dapper;
using DapperExtensions.Sql;
using DapperExtensions.Test.IntegrationTests;
using MySql.Data.MySqlClient;

namespace DapperExtensions.Test.Helpers
{
    public static class TestHelpers
    {
        public static SqlGeneratorImpl GetGenerator()
        {
            return new SqlGeneratorImpl(new SqlCeDialect());
        }

        public static SqlCeConnection GetConnection(string databaseName)
        {
            string connectionString = "Data Source=" + databaseName;
            SqlCeConnection connection = new SqlCeConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static Database GetSqlConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return new Database
                       {
                           Connection = connection,
                           Dialect = new SqlServerDialect()
                       };
        }

        public static Database GetSqlCeConnection(string connectionString)
        {
            string[] connectionParts = connectionString.Split(';');
            string file = connectionParts
                .ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1])
                .Where(d => d.Key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                .Select(k => k.Value).Single();

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (SqlCeEngine ce = new SqlCeEngine(connectionString))
            {
                ce.CreateDatabase();
            }

            SqlCeConnection connection = new SqlCeConnection(connectionString);
            connection.Open();
            return new Database
                       {
                           Connection = connection,
                           Dialect = new SqlCeDialect()
                       };
        }

        public static Database GetMySqlConnection(string connectionString)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            return new Database
                       {
                           Connection = connection,
                           Dialect = new MySqlDialect()
                       };
        }

        public static Database GetSqliteConnecton(string connectionString)
        {
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
            connection.Open();
            return new Database
                       {
                           Connection = connection,
                           Dialect = new SqliteDialect()
                       };
        }

        public static void LoadDatabases(IList<Database> databases)
        {
            foreach (var database in databases)
            {
                string prefix = database.Dialect.GetType().Name.Replace("Dialect", string.Empty);
                var files = new List<string>()
                                {
                                    ReadScriptFile(prefix + ".CreateAnimalTable"),
                                    ReadScriptFile(prefix + ".CreateFooTable"),
                                    ReadScriptFile(prefix + ".CreateMultikeyTable"),
                                    ReadScriptFile(prefix + ".CreatePersonTable"),
                                    ReadScriptFile(prefix + ".CreateCarTable")
                                };

                foreach (var setupFile in files)
                {
                    database.Connection.Execute(setupFile);
                }
            }
        }

        public static void DeleteDatabase(string databaseName)
        {
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

        public static Protected Protected(this object obj)
        {
            return new Protected(obj);
        }

        public static string ReadScriptFile(string name)
        {
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DapperExtensions.Test.IntegrationTests.SqlScripts." + name + ".sql"))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}