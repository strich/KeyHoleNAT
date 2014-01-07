namespace KeyHole {
    public static class ConnectionUtility {

        public static void CreateServer() { }

        public static void CreateServer(int requestedPort, string requestedProtocol) { }

        public static void CreateServer(int requestedPort, string requestedProtocol, string localIPAddress) { }

        private static void GetPort(int requestedPort, string requestedProtocol)
        {
            int port = 0;
            
            portGetPortViaUPNP();
        }

        private static int GetPortViaUPNP(int requestedPort, string requestedProtocol)
        {
            // TODO - Use UPnP lib to AddPortMapping()

            return 0;
        }

        private static int GetPortViaSTUN(int requestedPort, string requestedProtocol) {
            // TODO - Use UPnP lib to AddPortMapping()

            return 0;
        }
    }
}
