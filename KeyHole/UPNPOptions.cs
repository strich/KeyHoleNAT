namespace KeyHoleNAT {
    public class UPNPOptions {
        public bool Enabled;
        /// <summary>
        /// Aggressively maps a port by first deleting the exist port map if its assigned to another PC on the LAN.
        /// </summary>
        public bool AggressivePortMap;

		public UPNPOptions(bool enabled = true, bool aggressivePortMap = true) {
            Enabled = enabled;
		    AggressivePortMap = aggressivePortMap;

		}
    }
}