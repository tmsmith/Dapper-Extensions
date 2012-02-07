using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Threading;

namespace DapperExtensions.Test.Helpers
{
    public static class TestHelpers
    {
        public static SqlCeConnection GetConnection(string databaseName)
        {
            string connectionString = "Data Source=" + databaseName;
            SqlCeConnection connection = new SqlCeConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static void LoadDatabase(string databaseName)
        {
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName);
            }

            string connectionString = "Data Source=" + databaseName;
            using (SqlCeEngine ce = new SqlCeEngine(connectionString))
            {
                ce.CreateDatabase();
            }

            using (SqlCeConnection connection = GetConnection(databaseName))
            {
                using (SqlCeCommand cmd = new SqlCeCommand(ReadScriptFile("CreatePersonTable"), connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (SqlCeCommand cmd = new SqlCeCommand(ReadScriptFile("CreateMultikeyTable"), connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (SqlCeCommand cmd = new SqlCeCommand(ReadScriptFile("CreateAnimalTable"), connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (SqlCeCommand cmd = new SqlCeCommand(ReadScriptFile("CreateFooTable"), connection))
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
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DapperExtensions.Test.SqlScripts." + name + ".sql"))
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        public static Protected Protected(this object obj)
        {
            return new Protected(obj);
        }
    }
}