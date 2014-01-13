using System;

namespace KeyHole {
    public class GlobalOptions {
        public UInt16 PortToBind;
        public EventLoggingLevel LoggingLevel;

        public GlobalOptions(UInt16 portToBind, EventLoggingLevel loggingLevel = EventLoggingLevel.Informational) {
            PortToBind = portToBind;
            LoggingLevel = loggingLevel;
        }
    }
}