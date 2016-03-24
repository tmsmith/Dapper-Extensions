using System;
using System.Configuration;

namespace Dapper.Extensions.Linq.Core.Logging
{
    static class LogLevelReader
    {
        public static LogLevel GetDefaultLogLevel(LogLevel fallback = LogLevel.Info)
        {
            var logging = ConfigurationManager.GetSection(typeof(Config.Logging).Name) as Config.Logging;
            if (logging == null) return fallback;

            var threshold = logging.Threshold;
            LogLevel logLevel;
            if (Enum.TryParse(threshold, true, out logLevel)) return logLevel;

            string logLevels = string.Join(", ", Enum.GetNames(typeof(LogLevel)));
            throw new Exception(string.Format("The value of '{0}' is invalid as a loglevel. Must be one of {1}", threshold, logLevels));
        }
    }
}