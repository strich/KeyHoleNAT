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

        private UPNPOptions UPNPOptions;

        public UPNPModule(UPNPOptions upnpOptions, ProgressUpdateHandler onProgressUpdate,
            ProgressUpdateHandler onProgressFinish) {
            UPNPOptions = upnpOptions;
            ProgressUpdate += onProgressUpdate;
            ProgressFinish += onProgressFinish;

            // Setup the discovery phase timer:
            _discoveryTimeoutTimer = new SafeTimer(upnpOptions.Timeout, false);
            _discoveryTimeoutTimer.OnElapsed += OnUPNPDiscoveryPhaseEnd;

            // Setup the port map timer:
            _discoveryTimeoutTimer = new SafeTimer(2000, false);
            _discoveryTimeoutTimer.OnElapsed += OnUPNPPortMapFail;
        }

        private void OnUPNPPortMapFail() {
            OnProgressFinish(new KeyHoleEventMessage(
                messageDescription: "Port Mapping failed with an unknown timeout error.",
                messageCode: MessageCode.ErrUnknown,
                loggingLevel: EventLoggingLevel.Informational));
        }

        private void OnUPNPDiscoveryPhaseEnd() {
            // Set the instance of UPNPSmartControlPoint to null for garbage collection:
            _scp = null;
            OnProgressUpdate("Finished discovery scan.");
            OnProgressUpdate("Found " + _activeDevices.Count + " UPnP enabled devices.");

            // If there are no devices capable of UPnP Port Mapping then exit as failed:
            if (_activeDevices.Count == 0) {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "No UPnP capable devices were found.",
                    messageCode: MessageCode.ErrNoUPnPDevice,
                    loggingLevel: EventLoggingLevel.Informational));
            }

            // Attempt to port map on all devices found:
            // TODO: Figure out which device is the gateway
            foreach (DeviceServices device in _activeDevices) {
                // TODO remove this:
                //var localIP = device.Device.InterfaceToHost;
                //var remoteIP = device.Device.RemoteEndPoint.Address;
                UPnPService service = device.Services.First(s => s.ServiceID == "urn:upnp-org:serviceId:WANIPConn1" ||
                                                                 s.ServiceID == "urn:upnp-org:serviceId:WANIPConn2");

                UPnPAction portMappingAction = service.GetAction("AddPortMapping");
                var upnpArgs = new List<UPnPArgument> {
                    new UPnPArgument("NewRemoteHost", ""),
                    new UPnPArgument("NewExternalPort", (UInt16) 11112),
                    new UPnPArgument("NewProtocol", "TCP"),
                    new UPnPArgument("NewInternalPort", (UInt16) 11112),
                    new UPnPArgument("NewInternalClient", device.Device.InterfaceToHost),
                    new UPnPArgument("NewEnabled", true),
                    new UPnPArgument("NewPortMappingDescription", "War for the Overworld"),
                    new UPnPArgument("NewLeaseDuration", (UInt32) 3600)
                };

                try {
                    _portmapTimeoutTimer.Start();

                    service.InvokeAsync(portMappingAction.Name,
                        upnpArgs.ToArray(), null,
                        HandleInvoke,
                        HandleInvokeError);
                }
                catch (UPnPInvokeException ie) {
                    _portmapTimeoutTimer.dispose(); // Discard the timer.

                    OnProgressFinish(new KeyHoleEventMessage(
                        messageDescription: "UPnPInvokeException: " + ie.Message,
                        messageCode: MessageCode.ErrUnknown,
                        loggingLevel: EventLoggingLevel.Informational));
                }
            }
        }

        private void HandleInvokeError(UPnPService sender, string methodname, UPnPArgument[] args, UPnPInvokeException e,
            object tag) {
            if (e.UPNP == null) {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "UPnPInvokeException: " + e,
                    messageCode: MessageCode.ErrUnknown,
                    loggingLevel: EventLoggingLevel.Informational));
            }
            else {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "UPnPInvokeException: " + e.UPNP.ErrorCode + " : " +
                                        e.UPNP.ErrorDescription,
                    messageCode: MessageCode.ErrUnknown,
                    loggingLevel: EventLoggingLevel.Informational));
            }
        }

        private void HandleInvoke(UPnPService sender, string methodname, UPnPArgument[] args, object returnvalue,
            object tag) {
            OnProgressFinish(new KeyHoleEventMessage(
                messageDescription: "UPnP Successfully mapped a port.",
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