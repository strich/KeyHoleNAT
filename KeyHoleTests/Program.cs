using System;
using KeyHoleNAT;

namespace KeyHoleNATTests {
    internal class Program {
        private static void Main(string[] args) {
            // Initialize the controller:
            // All paramaters displayed here are optional and shown only for example.
            var nc = new NATController(
                // Options specific to UPnP:
                upnpOptions: new UPNPOptions(
                    // Enable or disable the UPnP Module:
                    enabled: true,
                    aggressivePortMap: true),
                // STUN method options
                stunOptions: new STUNOptions(
                    enabled: true, // Enable or disable the STUN method of UPnP port punching.
                    timeout: 5000),
                // Global options
                globalOptions: new GlobalOptions(
                    loggingLevel: EventLoggingLevel.Debug), // Minimum logging level
                // UPnP logging and progress handlers.
                onProgressUpdate: HandleProgressUpdate,
                onProgressFinished: HandleProgressFinish);

            // The BindPort method is async will report any success or failure via the progress handlers:
            nc.BindPort(
                portToBind: 19800, // Port to attempt to bind to
                portDescription: "", // An optional description that can be seen in a UPnP devices GUI.
                ipProtocol: IPProtocol.Both); // Specify TCP, UDP or both protocols to bind to.

            Console.WriteLine("Finished!");
            while (true) {}
        }

        private static void HandleProgressFinish(KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine(keyHoleEventMessage.MessageDescription + " | Code: " + keyHoleEventMessage.MessageCode);
        }

        private static void HandleProgressUpdate(KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine(keyHoleEventMessage.MessageDescription + " | Code: " + keyHoleEventMessage.MessageCode);
        }
    }
}