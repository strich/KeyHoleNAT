using System;
using KeyHole;

namespace KeyHoleTests {
	class Program {
		static void Main(string[] args) {
			//ConnectionManager cm = new ConnectionManager();
			//cm.CreateServer();

		    KeyHole.KeyHole kh = new KeyHole.KeyHole(
                upnpOptions: new UPNPOptions(
                    enabled: true,
                    timeout: 5000),
                stunOptions: new STUNOptions(
                    enabled: true,
                    timeout: 5000),
                globalOptions: new GlobalOptions(
                    loggingLevel: EventLoggingLevel.Debug,
                    preferredPort: 11112),
                onProgressUpdate: HandleProgressUpdate,
                onProgressFinished: HandleProgressFinish);

            kh.BindPort();

			while(true) { }
		}

	    private static void HandleProgressFinish(KeyHole.KeyHole sender, KeyHoleEventMessage progressUpdateEventArgs) {
            Console.WriteLine("[FINISHED] " + progressUpdateEventArgs.MessageDescription);
	    }

	    private static void HandleProgressUpdate(KeyHole.KeyHole sender, KeyHoleEventMessage progressUpdateEventArgs) {
	        Console.WriteLine(progressUpdateEventArgs.MessageDescription);
	    }
	}
}
