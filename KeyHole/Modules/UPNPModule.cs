using System;
using System.Collections.Generic;
using System.Linq;
using OpenSource.UPnP;

namespace KeyHoleNAT {
    public class UPNPModule : Module {
        // Devices listed here have an active port map on them
        private readonly List<DeviceServices> _activeDevices = new List<DeviceServices>();
        private readonly SafeTimer _discoveryTimeoutTimer;
        private readonly SafeTimer _portmapTimeoutTimer;
        private UPnPSmartControlPoint _scp;
        private UPNPOptions _upnpOptions;
	    private GlobalOptions _globalOptions;
	    private bool _isUdpBound = false;
		private bool _isTcpBound = false;
	    private bool _discoveryPhaseEnded = false;

        public UPNPModule(UPNPOptions upnpOptions, GlobalOptions globalOptions, ProgressUpdateHandler onProgressUpdate,
            ProgressUpdateHandler onProgressFinish) {
            _upnpOptions = upnpOptions;
	        _globalOptions = globalOptions;
            ProgressUpdate += onProgressUpdate;
            ProgressFinish += onProgressFinish;

            // Setup the discovery phase timer:
            _discoveryTimeoutTimer = new SafeTimer(upnpOptions.DiscoveryTimeout, false);
            _discoveryTimeoutTimer.OnElapsed += OnUPNPDiscoveryPhaseEnd;

            // Setup the port map timer:
			_portmapTimeoutTimer = new SafeTimer(upnpOptions.PortmapTimeout, false);
			_portmapTimeoutTimer.OnElapsed += OnUPNPPortMapFail;
        }

        private void OnUPNPPortMapFail() {
            OnProgressFinish(new KeyHoleEventMessage(
                messageDescription: "Port Mapping failed with an unknown timeout error.",
                messageCode: MessageCode.ErrUnknown,
                loggingLevel: EventLoggingLevel.Informational));
        }

		public bool BindPort() {
			
		}

        private void OnUPNPDiscoveryPhaseEnd() {
            // Set the instance of UPNPSmartControlPoint to null for garbage collection:
            _scp = null;
	        _discoveryPhaseEnded = true;
            OnProgressUpdate("Finished discovery scan.");
            OnProgressUpdate("Found " + _activeDevices.Count + " UPnP enabled devices.");

            // If there are no devices capable of UPnP Port Mapping then exit as failed:
            if (_activeDevices.Count == 0) {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "Error: " + MessageCode.ErrNoUPnPDevice + ": No UPnP capable devices were found.",
                    messageCode: MessageCode.ErrNoUPnPDevice,
                    loggingLevel: EventLoggingLevel.Informational));

	            return;
            }

            // Attempt to port map on all devices found:
            // TODO: Figure out which device is the gateway
            foreach (DeviceServices device in _activeDevices) {
				// Get the correct service type and action for the Port Mapping:
                UPnPService service = device.Services.First(s => s.ServiceID == "urn:upnp-org:serviceId:WANIPConn1" ||
                                                                 s.ServiceID == "urn:upnp-org:serviceId:WANIPConn2");
                UPnPAction addPortMappingAction = service.GetAction("AddPortMapping");
				UPnPAction deletePortMappingAction = service.GetAction("DeletePortMapping");

                /* Before we try to map a port to ourself, we will delete it in case someone else has already taken it: */
				// TODO: Add an option to allow the user to enable or disable how aggressive we are being here.
				try {
					// Start the port map timeout timer:
					_portmapTimeoutTimer.Start();

					// Send the Port Map request:
					if (_globalOptions.IPProtocol == IPProtocol.Both || _globalOptions.IPProtocol == IPProtocol.TCP) {
						DeletePortMapping(IPProtocol.TCP, deletePortMappingAction, service);
						AddPortMapping(IPProtocol.TCP, device, addPortMappingAction, service);
					}
					if (_globalOptions.IPProtocol == IPProtocol.Both || _globalOptions.IPProtocol == IPProtocol.UDP) {
						DeletePortMapping(IPProtocol.UDP, deletePortMappingAction, service);
						AddPortMapping(IPProtocol.UDP, device, addPortMappingAction, service);
					}
				} catch (UPnPInvokeException ie) {
					_portmapTimeoutTimer.Stop();

					OnProgressFinish(new KeyHoleEventMessage(
						messageDescription: "UPnPInvokeException: " + ie.Message,
						messageCode: MessageCode.ErrUnknown,
						loggingLevel: EventLoggingLevel.Informational));
				}
                
            }
        }

		/// <summary>
		/// Will delete an existing Port Map, if it exists.
		/// </summary>
		private void DeletePortMapping(IPProtocol ipProtocol, UPnPAction action, UPnPService service) {
			var args = new List<UPnPArgument> {
                new UPnPArgument("NewRemoteHost", ""),
                new UPnPArgument("NewExternalPort", _globalOptions.PortToBind),
                new UPnPArgument("NewProtocol", ipProtocol.ToString())
            };

			// TODO: Might want to properly handle return messages at some point.
			service.InvokeAsync(action.Name, args.ToArray(), null, null, null);
		}

		/// <summary>
		/// Will create a new Port Map, if possible.
		/// </summary>
		private void AddPortMapping(IPProtocol ipProtocol, DeviceServices device, UPnPAction action, UPnPService service) {
			var args = new List<UPnPArgument> {
                new UPnPArgument("NewRemoteHost", ""),
                new UPnPArgument("NewExternalPort", _globalOptions.PortToBind),
                new UPnPArgument("NewProtocol", ipProtocol.ToString()),
                new UPnPArgument("NewInternalPort", _globalOptions.PortToBind),
                new UPnPArgument("NewInternalClient", device.Device.InterfaceToHost.ToString()),
                new UPnPArgument("NewEnabled", true),
                new UPnPArgument("NewPortMappingDescription", _globalOptions.PortDescription),
                new UPnPArgument("NewLeaseDuration", (UInt32) 3600)
            };

			// Send the Port Map request:
			service.InvokeAsync(action.Name, args.ToArray(), null, HandleInvoke, HandleInvokeError);
		}

        private void HandleInvokeError(UPnPService sender, string methodname, UPnPArgument[] args, UPnPInvokeException e,
            object tag) {
            if (e.UPNP == null) {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "Received error trying to map port " + args[2].DataValue + args[1].DataValue + ". UPnPInvokeException: " + e,
                    messageCode: MessageCode.ErrUnknown,
                    loggingLevel: EventLoggingLevel.Informational));
            }
            else {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "Received error trying to map port " + args[2].DataValue + args[1].DataValue + ". UPnPInvokeException: [" + 
                        e.UPNP.ErrorCode + "] " + e.UPNP.ErrorDescription,
                    messageCode: MessageCode.ErrUnknown,
                    loggingLevel: EventLoggingLevel.Informational));
            }
        }

        private void HandleInvoke(UPnPService sender, string methodname, UPnPArgument[] args, object returnvalue,
            object tag) {
			// Stop the port map timer:
			_portmapTimeoutTimer.Stop();

            OnProgressUpdate(new KeyHoleEventMessage(
                messageDescription: "Successfully mapped port " + args[2].DataValue + args[1].DataValue + ".",
                messageCode: MessageCode.Success,
                loggingLevel: EventLoggingLevel.Informational));

			// Flag the port bind as successful:
			if (args[2].DataValue.ToString() == "UDP")
				_isUdpBound = true;
			if (args[2].DataValue.ToString() == "TCP")
				_isTcpBound = true;

			// If both ports are bound, return progress finished:
			if(_isTcpBound && _isUdpBound)
				OnProgressFinish(new KeyHoleEventMessage(
					messageDescription: "Successfully bound specified ports via UPnP.",
					messageCode: MessageCode.Success,
					loggingLevel: EventLoggingLevel.Informational));
        }

        private void OnDeviceDiscovery(UPnPSmartControlPoint sender, UPnPDevice device) {
            OnProgressUpdate("Discovered new device: " + device.FriendlyName + " of type " + device.StandardDeviceType);

            var services = new List<UPnPService>();

            // Services can be within embedded devices within the root parent devices.
            //  Discover all services within the device and list as a flat tree:
            DiscoverDeviceServices(device, services);

            // Check if it contains the port mapping services and save it for later if it does:
            if (services.Count(service => service.ServiceID == "urn:upnp-org:serviceId:WANIPConn1" ||
                                          service.ServiceID == "urn:upnp-org:serviceId:WANIPConn2") != 0) {
                var ds = new DeviceServices {Device = device, Services = services};
                _activeDevices.Add(ds);
            }

            OnProgressUpdate("Finished discovering the services of " + device.FriendlyName);
        }

        /// <summary>
        /// Discover all the services of the device and lay it out into a flat array.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="services"></param>
        private void DiscoverDeviceServices(UPnPDevice device, List<UPnPService> services) {
            // Discover services:
            services.AddRange(device.Services);

            // Recursive call into the device tree:
            foreach (UPnPDevice childDevice in device.EmbeddedDevices) {
                DiscoverDeviceServices(childDevice, services);
            }
        }


        public void Start() {
            OnProgressUpdate("Beginning discovery scan.");

            // Start timer:
            _discoveryTimeoutTimer.Start();

            // Setup the UPnP Smart Control Point:
            _scp = new UPnPSmartControlPoint(OnDeviceDiscovery);
        }

        public override string ToString() {
            return "UPNP";
        }
    }
}