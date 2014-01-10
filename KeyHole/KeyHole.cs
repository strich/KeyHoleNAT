using System;
using System.Collections.Generic;

namespace KeyHole {
    public class KeyHole {
        public delegate void ProgressUpdateHandler(KeyHole sender, ProgressUpdateEventArgs e);

        public GlobalOptions GlobalOptions;
        public STUNOptions STUNOptions;
        public UPNPOptions UPNPOptions;

        private UPNPModule upnpModule;

        public KeyHole() : this(new UPNPOptions(), new STUNOptions(), new GlobalOptions(), null, null) {}

        public KeyHole(UPNPOptions upnpOptions, STUNOptions stunOptions, GlobalOptions globalOptions,
            ProgressUpdateHandler onProgressUpdate, ProgressUpdateHandler onProgressFinished) {
            UPNPOptions = upnpOptions;
            STUNOptions = stunOptions;
            GlobalOptions = globalOptions;

            OnProgressUpdate += onProgressUpdate;
            OnProgressFinished += onProgressFinished;

            if(OnProgressUpdate != null)
                OnProgressUpdate(this, new ProgressUpdateEventArgs() {MessageDescription = "Test Starting KeyHole."});
        }

        public void BindPort() {
            // Attempt to bind a port via UPNP:
            upnpModule = new UPNPModule(UPNPOptions, HandleProgressUpdate, HandleProgressFinished);
            upnpModule.Start();
        }

        private void HandleProgressFinished(object sender, ProgressUpdateEventArgs progressUpdateEventArgs) {
            throw new NotImplementedException();
        }

        public void BindSocket() {
            // TODO
        }

        public event ProgressUpdateHandler OnProgressUpdate;
        public event ProgressUpdateHandler OnProgressFinished;

        private void HandleProgressUpdate(object sender, ProgressUpdateEventArgs progressUpdateEventArgs) {
            OnProgressUpdate(this, progressUpdateEventArgs);
        }

        ~KeyHole() {
            // TODO
        }
    }

    public enum EventLoggingLevel {
        Informational,
        Warning,
        Error,
        Debug
    }
}