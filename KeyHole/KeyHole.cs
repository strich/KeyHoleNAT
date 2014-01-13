using System;
using System.Collections.Generic;

namespace KeyHole {
    public class KeyHole {
        public delegate void ProgressUpdateHandler(KeyHole sender, KeyHoleEventMessage e);

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
            OnProgressFinish += onProgressFinished;
        }

        public void BindPort() {
            // Attempt to bind a port via UPNP:
            upnpModule = new UPNPModule(UPNPOptions, HandleProgressUpdate, HandleProgressFinish);
            upnpModule.Start();
        }

        

        public void BindSocket() {
            // TODO
        }

        public event ProgressUpdateHandler OnProgressUpdate;
        public event ProgressUpdateHandler OnProgressFinish;

        private void HandleProgressUpdate(object sender, KeyHoleEventMessage keyHoleEventMessage) {
            OnProgressUpdate(this, keyHoleEventMessage);
        }

        private void HandleProgressFinish(object sender, KeyHoleEventMessage keyHoleEventMessage) {
            OnProgressFinish(this, keyHoleEventMessage);
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