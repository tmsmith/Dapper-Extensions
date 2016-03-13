using System;
using System.Diagnostics;

namespace Dapper.Extensions.Linq.Core.Logging
{
    class DefaultLoggerFactory : ILoggerFactory
    {
        readonly LogLevel _filterLevel;
        readonly bool _isDebugEnabled;
        readonly bool _isErrorEnabled;
        readonly bool _isFatalEnabled;
        readonly bool _isInfoEnabled;
        readonly bool _isWarnEnabled;
        private LoggerConfig _config;

        readonly object _locker = new object();

        public DefaultLoggerFactory(LogLevel filterLevel)
        {
            _filterLevel = filterLevel;
            _isDebugEnabled = LogLevel.Debug >= filterLevel;
            _isInfoEnabled = LogLevel.Info >= filterLevel;
            _isWarnEnabled = LogLevel.Warn >= filterLevel;
            _isErrorEnabled = LogLevel.Error >= filterLevel;
            _isFatalEnabled = LogLevel.Fatal >= filterLevel;
        }

        public ILog GetLogger(Type type, LoggerConfig config)
        {
            _config = config;

            return new DefaultLogger(type.FullName, this)
            {
                IsDebugEnabled = _isDebugEnabled && config.Enabled,
                IsInfoEnabled = _isInfoEnabled && config.Enabled,
                IsWarnEnabled = _isWarnEnabled && config.Enabled,
                IsErrorEnabled = _isErrorEnabled && config.Enabled,
                IsFatalEnabled = _isFatalEnabled && config.Enabled
            };
        }

        public void Write(string name, LogLevel messageLevel, string message)
        {
            if (!_config.Enabled || messageLevel < _filterLevel)
                return;

            var datePart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var paddedLevel = messageLevel.ToString().ToUpper();
            var fullMessage = string.Format("{0} {1} {2} {3}", datePart, paddedLevel, name, message);

            lock (_locker)
            {
                Trace.WriteLine(fullMessage);
            }
        }
    }
}