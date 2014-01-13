using System;
using KeyHole;

namespace KeyHoleTests {
	class Program {
		static void Main(string[] args) {
		    KeyHole.KeyHole kh = new KeyHole.KeyHole(
                upnpOptions: new UPNPOptions(
                    enabled: true,
                    timeout: 5000),
                stunOptions: new STUNOptions(
                    enabled: true,
                    timeout: 5000),
                globalOptions: new GlobalOptions(
                    portToBind: 21899,
                    loggingLevel: EventLoggingLevel.Debug),
                onProgressUpdate: HandleProgressUpdate,
                onProgressFinished: HandleProgressFinish);

            kh.BindPort();

			while(true) { }
		}

	    private static void HandleProgressFinish(KeyHole.KeyHole sender, KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine("[FINISHED " + keyHoleEventMessage.MessageCode + "] " + keyHoleEventMessage.MessageDescription);
	    }

        private static void HandleProgressUpdate(KeyHole.KeyHole sender, KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine(keyHoleEventMessage.MessageDescription);
	    }
	}
}
