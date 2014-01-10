namespace KeyHole {
    public class UPNPOptions {
        public bool Enabled;
        public int Timeout;

        public UPNPOptions(bool enabled = true, int timeout = 5000) {
            Enabled = enabled;
            Timeout = timeout;
        }
    }
}