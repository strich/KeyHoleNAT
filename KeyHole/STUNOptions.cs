namespace KeyHoleNAT {
    public class STUNOptions {
        public bool Enabled;
        public int Timeout;

        public STUNOptions(bool enabled = true, int timeout = 5000) {
            Enabled = enabled;
            Timeout = timeout;
        }
    }
}