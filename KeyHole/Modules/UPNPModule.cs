using System.Collections.Generic;
using OpenSource.UPnP;

namespace KeyHole {
    public class UPNPModule {
        public delegate void ProgressUpdateHandler(KeyHole sender, ProgressUpdateEventArgs e);

        // Devices listed here have an active port map on them
        private readonly List<DeviceServices> activeDevices = new List<DeviceServices>();

        private UPnPSmartControlPoint scp;

        public UPNPModule(ProgressUpdateHandler onProgressUpdate, ProgressUpdateHandler onProgressFinished) {
            OnProgressUpdate += onProgressUpdate;
            OnProgressFinished += onProgressFinished;
        }

        public void Start() {
            
        }

        public event ProgressUpdateHandler OnProgressUpdate;
        public event ProgressUpdateHandler OnProgressFinished;
    }
}
