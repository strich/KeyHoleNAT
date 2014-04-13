using System;
using System.Collections.Generic;
using System.Timers;
using Mono.Nat;

namespace KeyHoleNAT {
    public class UPNPModule : Module {
        // Devices listed here have an active port map on them
		private readonly List<INatDevice> _activeDevices = new List<INatDevice>();
        private readonly Timer _discoveryTimeoutTimer;
		private readonly Timer _portmapTimeoutTimer;
        private UPNPOptions _upnpOptions;
	    private GlobalOptions _globalOptions;
	    private bool _discoveryPhaseEnded = false;

        public UPNPModule(UPNPOptions upnpOptions, GlobalOptions globalOptions, ProgressUpdateHandler onProgressUpdate,
            ProgressUpdateHandler onProgressFinish) {
            _upnpOptions = upnpOptions;
	        _globalOptions = globalOptions;
            ProgressUpdate += onProgressUpdate;
            ProgressFinish += onProgressFinish;

            // Setup and start the discovery phase timer:
	        _discoveryTimeoutTimer = new Timer(_upnpOptions.DiscoveryTimeout) {AutoReset = false};
            _discoveryTimeoutTimer.Elapsed += OnUPNPDiscoveryPhaseEnd;
            _discoveryTimeoutTimer.Start();

            // Setup the port map timer:
            //_portmapTimeoutTimer = new Timer(_upnpOptions.PortmapTimeout) {AutoReset = false};
            //_portmapTimeoutTimer.Elapsed += OnUPNPPortMapFail;

            OnProgressUpdate("Beginning discovery scan.");

            // Setup UPnP:
			NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery();
        }

	    private void DeviceFound(object sender, DeviceEventArgs e) {
            OnProgressUpdate("Discovered new device: " + e.Device.GetType().Name + " with external IP " + e.Device.GetExternalIP());

            _activeDevices.Add(e.Device);
	    }

	    private void OnUPNPPortMapFail(object state, ElapsedEventArgs elapsedEventArgs) {
            OnProgressFinish(new KeyHoleEventMessage(
                messageDescription: "Port Mapping failed with an unknown timeout error.",
                messageCode: MessageCode.ErrUnknown,
                loggingLevel: EventLoggingLevel.Informational));
        }

		public bool BindPort(UInt16 portToBind, IPProtocol ipProtocol, string portDescription) {
            // Block until discovery phase is over:
            while(!_discoveryPhaseEnded) {}

            // Attempt to port map on all devices found:
            // TODO: Figure out which device is the gateway
            foreach (var device in _activeDevices) {
                /* Before we try to map a port to ourself, we will delete it in case someone else has already taken it: */
                // TODO: Add an option to allow the user to enable or disable how aggressive we are being here.
                try {
                    // Start the port map timeout timer:
                    _portmapTimeoutTimer.Start();

                    // Send the Port Map request:
                    if (ipProtocol == IPProtocol.Both || ipProtocol == IPProtocol.TCP) {
                        DeletePortMapping(portToBind, Protocol.Tcp, device);
                        AddPortMapping(portToBind, Protocol.Tcp, portDescription, device);
                    }
                    if (ipProtocol == IPProtocol.Both || ipProtocol == IPProtocol.UDP) {
                        DeletePortMapping(portToBind, Protocol.Udp, device);
                        AddPortMapping(portToBind, Protocol.Udp, portDescription, device);
                    }
                } catch (Exception ex) {
                    //_portmapTimeoutTimer.Stop();

                    OnProgressFinish(new KeyHoleEventMessage(
                        messageDescription: "Port mapping error: " + ex.Message,
                        messageCode: MessageCode.ErrUnknown,
                        loggingLevel: EventLoggingLevel.Informational));
                }
            }
		    return true;
		}

        private void OnUPNPDiscoveryPhaseEnd(object state, ElapsedEventArgs elapsedEventArgs) {
	        _discoveryPhaseEnded = true;
            _discoveryTimeoutTimer.Stop();
            OnProgressUpdate("Finished initial discovery scan.");
            OnProgressUpdate("Found " + _activeDevices.Count + " UPnP enabled devices.");

            // If there are no devices capable of UPnP Port Mapping then exit as failed:
            if (_activeDevices.Count == 0) {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: MessageCode.ErrNoUPnPDevice + ": No UPnP capable devices were found.",
                    messageCode: MessageCode.ErrNoUPnPDevice,
                    loggingLevel: EventLoggingLevel.Informational));
            }
        }

		/// <summary>
		/// Will delete an existing Port Map, if it exists.
		/// </summary>
		private void DeletePortMapping(UInt16 portToBind, Protocol ipProtocol, INatDevice device) {
            Mapping args = new Mapping(ipProtocol, (int)portToBind, (int)portToBind);

			try {
				device.DeletePortMap(args);
			} catch {
				// We don't care if it doesn't succeed.
			}
		}

		/// <summary>
		/// Will create a new Port Map, if possible.
		/// </summary>
		private void AddPortMapping(UInt16 portToBind, Protocol ipProtocol, string portDescription, INatDevice device) {
            Mapping args = new Mapping(ipProtocol, (int)portToBind, (int)portToBind);
            device.CreatePortMap(args);
		}

        public override string ToString() {
            return "UPNP";
        }
    }
}