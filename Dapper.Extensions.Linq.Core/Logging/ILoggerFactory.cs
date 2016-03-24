using System;

namespace Dapper.Extensions.Linq.Core.Logging
{
    /// <summary>
    /// Used by <see cref="LogManager" /> to facilitate redirecting logging to a different library.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Gets a <see cref="ILog" /> for a specific <paramref name="type" />.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> to get the <see cref="ILog" /> for.</param>
        /// <param name="config">The <see cref="LoggerConfig"/></param>
        /// <returns>An instance of a <see cref="ILog" /> specifically for <paramref name="type" />.</returns>
        ILog GetLogger(Type type, LoggerConfig config);
    }
}