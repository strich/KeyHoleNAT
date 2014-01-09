namespace KeyHole {
    public class UPNPOptions {
        public bool Enabled;
        public int TimeOut;

        public UPNPOptions(bool enabled = true, int timeOut = 5000) {
            Enabled = enabled;
            TimeOut = timeOut;
        }
    }
}