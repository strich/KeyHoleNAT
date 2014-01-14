using System;

namespace KeyHoleNAT {
    public class GlobalOptions {
        public IPProtocol IPProtocol;
        public EventLoggingLevel LoggingLevel;
        public string PortDescription;
        public UInt16 PortToBind;

        public GlobalOptions(UInt16 portToBind, IPProtocol ipProtocol = IPProtocol.Both, string portDescription = "",
            EventLoggingLevel loggingLevel = EventLoggingLevel.Informational) {
            PortToBind = portToBind;
            IPProtocol = ipProtocol;
            LoggingLevel = loggingLevel;
            PortDescription = portDescription;
        }
    }

    public enum IPProtocol {
        UDP,
        TCP,
        Both
    }
}