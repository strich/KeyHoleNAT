using System;
using KeyHoleNAT;

namespace KeyHoleTests {
	class Program {
		static void Main(string[] args) {
            NATController nc = new NATController(
                upnpOptions: new UPNPOptions(
                    enabled: true,
                    discoveryTimeout: 5000,
					portmapTimeout: 2000),
                globalOptions: new GlobalOptions(
                    loggingLevel: EventLoggingLevel.Debug),
                onProgressUpdate: HandleProgressUpdate,
                onProgressFinished: HandleProgressFinish);

            nc.BindPort(
                portToBind: 17562,
                portDescription: "",
                ipProtocol: IPProtocol.Both);

			while(true) { }
		}

        private static void HandleProgressFinish(NATController sender, KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine(keyHoleEventMessage.MessageDescription);
	    }

        private static void HandleProgressUpdate(NATController sender, KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine(keyHoleEventMessage.MessageDescription + " | Code: " + keyHoleEventMessage.MessageCode);
	    }
	}
}
