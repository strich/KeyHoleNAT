using System;

namespace KeyHoleNAT {
    public class GlobalOptions {
        public UInt16 PortToBind;
	    public string PortDescription;
        public EventLoggingLevel LoggingLevel;

        public GlobalOptions(UInt16 portToBind, string portDescription = "", EventLoggingLevel loggingLevel = EventLoggingLevel.Informational) {
            PortToBind = portToBind;
            LoggingLevel = loggingLevel;
	        PortDescription = portDescription;
        }
    }
}