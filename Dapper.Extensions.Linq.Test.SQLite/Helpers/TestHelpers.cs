using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using DapperExtensions.Sql;

namespace DapperExtensions.Test.Helpers
{
    public static class TestHelpers
    {
        public static SqlGeneratorImpl GetGenerator()
        {
            return new SqlGeneratorImpl(new SqliteDialect());
        }

        public static SQLiteConnection GetConnection(string databaseName)
        {
            var connectionString = "Data Source=" + databaseName;
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static void LoadDatabase(string databaseName)
        {
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }
            
            SQLiteConnection.CreateFile(databaseName);
            
            using (var connection = GetConnection(databaseName))
            {
                using (var cmd = new SQLiteCommand(ReadScriptFile("CreatePersonTable"), connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SQLiteCommand(ReadScriptFile("CreateMultikeyTable"), connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SQLiteCommand(ReadScriptFile("CreateAnimalTable"), connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SQLiteCommand(ReadScriptFile("CreateFooTable"), connection))
                {
                    cmd.ExecuteNonQuery();
                }

                connection.Close();
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

        public static void DeleteAllDatabase()
        {
            foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "db_*.sdf"))
            {
                DeleteDatabase(file);
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

        public static string ReadScriptFile(string name)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var s = assembly.GetManifestResourceStream(string.Format("Dapper.Extensions.Linq.Test.SqlScripts.{0}.sql", name)))
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}