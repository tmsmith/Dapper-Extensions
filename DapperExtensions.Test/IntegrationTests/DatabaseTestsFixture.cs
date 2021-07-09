using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DapperExtensions.Test.IntegrationTests
{
    [NonParallelizable]
    public abstract class DatabaseTestsFixture : IDisposable
    {
        private readonly Dictionary<string, string> _connectionStrings = new Dictionary<string, string>();
        private readonly string projectName = Assembly.GetCallingAssembly().GetName().Name;
        protected readonly string[] CreateTableScripts = { "CreateAnimalTable", "CreateFooTable", "CreateMultikeyTable", "CreatePersonTable", "CreateCarTable" };

        protected DatabaseTestsFixture(string configPath = null)
        {
            ProjectPath = configPath ?? GetBasePath();
        }

        public virtual IDatabase Db { get; private set; }

        public string ProjectPath { get; }

        protected SqlDialectBase Dialect { get; private set; }

        protected virtual string ConnectionString(string connectionName)
        {
            lock (_connectionStrings)
            {
                if (_connectionStrings.Count == 0)
                {
                    var fileContent = ReadFile(Path.Combine(ProjectPath, "connectionstrings.json"));

                    foreach (var item in JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent))
                    {
                        _connectionStrings.Add(item.Key, item.Value);
                    }
                }
            }

            var result = _connectionStrings.FirstOrDefault(c => c.Key.Equals(connectionName));

            if (!result.Equals(default) && !string.IsNullOrEmpty(result.Value))
            {
                return result.Value;
            }
            else
            {
                throw new KeyNotFoundException($"ConnectionString not found in file '{ProjectPath}'");
            }
        }

        protected virtual void CommonSetup(DbConnection connection, SqlDialectBase sqlDialect)
        {
            Dialect = sqlDialect;
            var config = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), Dialect);
            var sqlGenerator = new SqlGeneratorImpl(config);
            Db = new Database(connection, sqlGenerator);
        }

        private string GetNamespacePath()
        {
            var namespacePath = $"{GetType().Namespace.Replace($"{projectName}.", "")}";

            //TODO: Understand why projectName comes as "nunit.framework" using release configuration and need to adjust here
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                foreach (var aux in namespacePath.Split('.'))
                {
                    if (namespacePath.Contains(aux) && ProjectPath.Contains(aux))
                    {
                        //Using substrings to avoid removing partial strings
                        var indexOf = namespacePath.IndexOf(aux);

                        namespacePath = indexOf > 0 ?
                            $"{namespacePath.Substring(0, indexOf - 1)}{namespacePath.Substring(indexOf + aux.Length)}" :
                            indexOf + aux.Length >= namespacePath.Length ? "" : namespacePath.Substring(indexOf + aux.Length);
                    }
                }
            }

            while (namespacePath.Contains("."))
            {
                namespacePath = namespacePath.Substring(namespacePath.IndexOf(".") + 1);
            }

            return $"DDL/{namespacePath}";
        }

        public virtual void ExecuteScripts(IDbConnection connection, bool abortOnError, params string[] scripts)
        {
            var files = new List<string>();
            var namespacePath = GetNamespacePath();

            foreach (var script in scripts)
            {
                var fileName = $"{ProjectPath}/{namespacePath}/{script}";

                fileName += !Path.HasExtension(fileName) ? ".sql" : "";

                files.Add(ReadFile(fileName));
            };

            foreach (var setupFile in files)
            {
                try
                {
                    connection.Execute(setupFile);
                }
                catch (Exception)
                {
                    if (abortOnError)
                        throw;
                }
            }
        }

        private static string ReadFile(string fileName)
        {
            using StreamReader sr = new StreamReader(fileName);
            return sr.ReadToEnd();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                Db.Dispose();
            }
        }

        private string GetBasePath()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (directory != null && !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            return directory.FullName;
        }
    }
}