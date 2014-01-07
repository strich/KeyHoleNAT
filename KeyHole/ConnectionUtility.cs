namespace KeyHole {
    public static class ConnectionUtility {

        public static void CreateServer() { }

        public static void CreateServer(int port, string protocol) { }

        public static void CreateServer(int port, string protocol, string IPAddress) { }

        private static int GetPortViaUpnp()
        {
            // TODO - Use UPnP lib to AddPortMapping()

            return 0;
        }
    }
}
