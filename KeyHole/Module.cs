namespace KeyHoleNAT {
    public class Module {
        public delegate void ProgressUpdateHandler(KeyHoleEventMessage e);
        public event ProgressUpdateHandler ProgressUpdate;
        public static event ProgressUpdateHandler ProgressFinish;

        protected void OnProgressUpdate(KeyHoleEventMessage e) {
            if (ProgressUpdate != null) {
                ProgressUpdate(new KeyHoleEventMessage(
                    messageDescription: "[UPnP] " + e.MessageDescription,
                    messageCode: e.MessageCode,
                    loggingLevel: e.LoggingLevel));
            }
        }

        protected void OnProgressUpdate(string message) {
            OnProgressUpdate(new KeyHoleEventMessage(
                messageDescription: message,
                messageCode: MessageCode.None,
                loggingLevel: EventLoggingLevel.Informational));
        }

        protected static void OnProgressFinish(KeyHoleEventMessage e) {
            if (ProgressFinish != null) {
                ProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "[UPnP] " + e.MessageDescription,
                    messageCode: e.MessageCode,
                    loggingLevel: e.LoggingLevel));
            }
        }
    }
}
