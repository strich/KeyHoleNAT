namespace KeyHole {
    public class KeyHole {
        public delegate void ProgressUpdateHandler(KeyHole sender, ProgressUpdateEventArgs e);

        public GlobalOptions GlobalOptions;
        public STUNOptions STUNOptions;
        public UPNPOptions UPnPOptions;

        public KeyHole() : this(new UPNPOptions(), new STUNOptions(), new GlobalOptions(), null, null) {}

        public KeyHole(UPNPOptions upnpOptions, STUNOptions stunOptions, GlobalOptions globalOptions,
            ProgressUpdateHandler onProgressUpdate, ProgressUpdateHandler onProgressFinished) {
            UPnPOptions = upnpOptions;
            STUNOptions = stunOptions;
            GlobalOptions = globalOptions;

            OnProgressUpdate += onProgressUpdate;
            OnProgressFinished += onProgressFinished;

            OnProgressUpdate(this, new ProgressUpdateEventArgs() {ProgressUpdate = "Test Progress Update."});
        }

        public void BindPort() {
            // TODO
        }

        public void BindSocket() {
            // TODO
        }

        public event ProgressUpdateHandler OnProgressUpdate;
        public event ProgressUpdateHandler OnProgressFinished;

        ~KeyHole() {
            // TODO
        }
    }

    public enum ProgressUpdateVerbosity {
        Minimal,
        Normal,
        Verbose
    }
}