using System;

namespace Dapper.Extensions.Linq.Core.Logging
{
    public class DefaultFactory : LoggingFactoryDefinition
    {
        readonly Lazy<LogLevel> _level;

        public DefaultFactory()
        {
            _level = new Lazy<LogLevel>(() => LogLevelReader.GetDefaultLogLevel());
        }

        protected internal override ILoggerFactory GetLoggingFactory()
        {
            return new DefaultLoggerFactory(_level.Value);
        }
    }
}