using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace DapperExtensions.Test.Helpers
{
    public class PortableDatabaseHelper
    {
        private static readonly object lockObj = new object();

        public static string ProcessConnectionString(string connectionString, string basePath)
        {
            lock (lockObj)
            {
                connectionString = string.Format(connectionString, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                var connectionParts = connectionString.Split(';');
                var file = connectionParts
                    .ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1])
                    .Where(d => d.Key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                    .Select(k => k.Value).Single();

                file = $"{basePath}\\{file}";
                var filePath = Path.GetDirectoryName(file);

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                return $"Data Source={file}";
            }
        }

        public static bool IsDatabaseInUse(string databaseName)
        {
            FileStream fs = null;
            try
            {
                var fi = new FileInfo(databaseName);
                fs = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                fs?.Close();
            }
        }

        public static void TearDown(string databaseName)
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
    }
}
