using System;

namespace KeyHole {
    public class GlobalOptions {
        public UInt16 PreferredPort;
        public EventLoggingLevel LoggingLevel;

        public GlobalOptions(UInt16 preferredPort = 0, EventLoggingLevel loggingLevel = EventLoggingLevel.Informational) {
            PreferredPort = preferredPort;
            LoggingLevel = loggingLevel;
        }
    }
}