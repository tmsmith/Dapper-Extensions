using System;

namespace Dapper.Extensions.Linq.Core.Logging
{
    /// <summary>
    /// Responsible for the creation of <see cref="ILog" /> instances and used as an extension point to redirect log event to
    /// an external library.
    /// </summary>
    /// <remarks>
    /// The default logging will use trace
    /// </remarks>
    public class LogManager : ILogManager
    {
        static Lazy<ILoggerFactory> _loggerFactory = new Lazy<ILoggerFactory>(new DefaultFactory().GetLoggingFactory);
        static readonly LoggerConfig LoggerConfig = new LoggerConfig();

        /// <summary>
        /// Used to inject an instance of <see cref="ILoggerFactory" /> into <see cref="LogManager" />.
        /// </summary>
        public static ILogManager Use<T>() where T : LoggingFactoryDefinition, new()
        {
            var loggingDefinition = new T();
            _loggerFactory = new Lazy<ILoggerFactory>(loggingDefinition.GetLoggingFactory);

            return new LogManager();
        }

        public ILogManager Enable(bool value = true)
        {
            LoggerConfig.Enabled = value;
            return this;
        }

        /// <summary>
        /// Construct a <see cref="ILog" /> using <typeparamref name="T" /> as the name.
        /// </summary>
        public static ILog GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        /// <summary>
        /// Construct a <see cref="ILog" /> using <paramref name="type" /> as the name.
        /// </summary>
        public static ILog GetLogger(Type type)
        {
            return _loggerFactory.Value.GetLogger(type, LoggerConfig);
        }
    }
}
