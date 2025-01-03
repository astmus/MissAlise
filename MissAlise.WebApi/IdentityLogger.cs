﻿using Microsoft.IdentityModel.Abstractions;
using System.Diagnostics;

namespace MissAlise.WebApi
{
    public class IdentityLogger : IIdentityLogger
    {
        private EventLogLevel _minLogLevel = EventLogLevel.LogAlways;

        /// <summary>
        /// Create instance of IIdentityLogger implementer and set a logging level for this instance
        /// </summary>
        /// <param name="minLogLevel">Default: LogAlways</param>
        public IdentityLogger(EventLogLevel minLogLevel = EventLogLevel.LogAlways)
        {
            _minLogLevel = minLogLevel;
        }

        public bool IsEnabled(EventLogLevel eventLogLevel)
        {
            return eventLogLevel >= _minLogLevel;
        }

        public void Log(LogEntry entry)
        {
            Debug.WriteLine($"MSAL: EventLogLevel: {entry.EventLogLevel}, Message: {entry.Message} ");
        }
    }
}
