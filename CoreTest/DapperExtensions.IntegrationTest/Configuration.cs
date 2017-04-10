using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperExtensions.IntegrationTests
{
    public class Configuration
    {
        private static readonly IConfigurationRoot Configurtion;

        static Configuration()
        {
            Configurtion = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json.config", optional: true)
                .Build();

        }

        public static string GetConnectionString(string name)
        {
            return Configurtion[$"ConnectionStrings:{name}"];
        }
    }
}
