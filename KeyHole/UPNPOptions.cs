namespace KeyHoleNAT {
    public class UPNPOptions {
        public bool Enabled;
		public  int DiscoveryTimeout;
		public  int PortmapTimeout;

		public UPNPOptions(bool enabled = true, int discoveryTimeout = 5000, int portmapTimeout = 2000) {
            Enabled = enabled;
			DiscoveryTimeout = discoveryTimeout;
			PortmapTimeout = portmapTimeout;
		}
    }
}