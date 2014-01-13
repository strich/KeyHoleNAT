namespace KeyHole {
    public class KeyHoleEventMessage : System.EventArgs {
        public EventLoggingLevel LoggingLevel;
        public string MessageDescription;
        public MessageCode MessageCode;

        public KeyHoleEventMessage(string messageDescription, MessageCode messageCode = MessageCode.None, EventLoggingLevel loggingLevel = EventLoggingLevel.Informational) {
            LoggingLevel = loggingLevel;
            MessageDescription = messageDescription;
            MessageCode = messageCode;
        }
    }

    public enum MessageCode {
        None = 0,
        Success,
        ErrUnknown,
        ErrNoUPnPDevice
    }
}
