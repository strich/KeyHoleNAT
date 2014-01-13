﻿using System;
using KeyHoleNAT;

namespace KeyHoleTests {
	class Program {
		static void Main(string[] args) {
            NATController nc = new NATController(
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

            nc.BindPort();

			while(true) { }
		}

        private static void HandleProgressFinish(NATController sender, KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine("[FINISHED " + keyHoleEventMessage.MessageCode + "] " + keyHoleEventMessage.MessageDescription);
	    }

        private static void HandleProgressUpdate(NATController sender, KeyHoleEventMessage keyHoleEventMessage) {
            Console.WriteLine(keyHoleEventMessage.MessageDescription);
	    }
	}
}
