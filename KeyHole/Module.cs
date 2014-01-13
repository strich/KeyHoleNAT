namespace KeyHole {
    public class Module {
        public delegate void ProgressUpdateHandler(object sender, KeyHoleEventMessage e);

        public event ProgressUpdateHandler ProgressUpdate;
        public event ProgressUpdateHandler ProgressFinish;

        protected void OnProgressUpdate(KeyHoleEventMessage e) {
            if (ProgressUpdate != null) {
                ProgressUpdate(this, e);
            }
        }

        protected void OnProgressUpdate(string message) {
            OnProgressUpdate(new KeyHoleEventMessage(
                messageDescription: message,
                messageCode: MessageCode.None,
                loggingLevel: EventLoggingLevel.Informational));
        }

        protected void OnProgressFinish(KeyHoleEventMessage e) {
            if (ProgressFinish != null) {
                ProgressFinish(this, e);
            }
        }
    }
}
