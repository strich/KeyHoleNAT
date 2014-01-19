using System;

namespace KeyHoleNAT {
    public class GlobalOptions {
        public EventLoggingLevel LoggingLevel;

        public GlobalOptions(EventLoggingLevel loggingLevel = EventLoggingLevel.Informational) {
            LoggingLevel = loggingLevel;
        }
    }

    public enum IPProtocol {
        UDP,
        TCP,
        Both
    }
}