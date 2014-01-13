using System;
using System.Collections.Generic;
using System.Linq;
using OpenSource.UPnP;

namespace KeyHole {
    public class UPNPModule : Module {

        // Devices listed here have an active port map on them
        private readonly List<DeviceServices> activeDevices = new List<DeviceServices>();
        private SafeTimer timeoutTimer;
        private UPnPSmartControlPoint scp;

        private UPNPOptions UPNPOptions;

        public UPNPModule(UPNPOptions upnpOptions, ProgressUpdateHandler onProgressUpdate,
            ProgressUpdateHandler onProgressFinish) {
            UPNPOptions = upnpOptions;
            ProgressUpdate += onProgressUpdate;
            ProgressFinish += onProgressFinish;

            // Setup the timer:
            timeoutTimer = new SafeTimer(upnpOptions.Timeout, false);
            timeoutTimer.OnElapsed += OnUPNPDiscoveryPhaseEnd;
        }

        private void OnUPNPDiscoveryPhaseEnd() {
            // Set the instance of UPNPSmartControlPoint to null for garbage collection:
            scp = null;
            OnProgressUpdate("[UPNP] Finished discovery scan.");
            OnProgressUpdate("[UPNP] Found " + activeDevices.Count + " UPnP enabled devices.");

            // If there are no devices capable of UPnP Port Mapping then exit as failed:
            if (activeDevices.Count == 0) {
                
            }

            // Attempt to port map on all devices found:
            // TODO: Figure out which device is the gateway
            foreach (DeviceServices device in activeDevices) {
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
                    service.InvokeAsync(portMappingAction.Name,
                                        upnpArgs.ToArray(), null,
                                        HandleInvoke,
                                        HandleInvokeError);
                } catch (UPnPInvokeException ie) {
                    OnProgressFinish(new KeyHoleEventMessage(
                        messageDescription: "[UPNP] UPnPInvokeException: " + ie.Message,
                        messageCode: MessageCode.ErrUnknown,
                        loggingLevel: EventLoggingLevel.Informational));
                }
            }
        }

        private void HandleInvokeError(UPnPService sender, string methodname, UPnPArgument[] args, UPnPInvokeException e,
                                       object tag) {
            if (e.UPNP == null) {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "[UPNP] UPnPInvokeException: " + e,
                    messageCode: MessageCode.ErrUnknown,
                    loggingLevel: EventLoggingLevel.Informational));
            }
            else {
                OnProgressFinish(new KeyHoleEventMessage(
                    messageDescription: "[UPNP] UPnPInvokeException: " + e.UPNP.ErrorCode + " : " +
                                  e.UPNP.ErrorDescription,
                    messageCode: MessageCode.ErrUnknown,
                    loggingLevel: EventLoggingLevel.Informational));
            }
        }

        private void HandleInvoke(UPnPService sender, string methodname, UPnPArgument[] args, object returnvalue, object tag) {
            OnProgressFinish(new KeyHoleEventMessage(
                messageDescription: "[UPNP] UPnP Successfully mapped a port.",
                messageCode: MessageCode.Success,
                loggingLevel: EventLoggingLevel.Informational));
        }

        private void OnDeviceDiscovery(UPnPSmartControlPoint sender, UPnPDevice device) {
            OnProgressUpdate("[UPNP] Discovered new device: " + device.FriendlyName + " of type " + device.StandardDeviceType);

            var services = new List<UPnPService>();

            // Services can be within embedded devices within the root parent devices.
            //  Discover all services within the device and list as a flat tree:
            DiscoverDeviceServices(device, services);

            // Check if it contains the port mapping services and save it for later if it does:
            if (services.Count(service => service.ServiceID == "urn:upnp-org:serviceId:WANIPConn1" ||
                                          service.ServiceID == "urn:upnp-org:serviceId:WANIPConn2") != 0) {
                var ds = new DeviceServices { Device = device, Services = services };
                activeDevices.Add(ds);
            }

            OnProgressUpdate("[UPNP] Finished discovering the services of " + device.FriendlyName);
        }

        private void DiscoverDeviceServices(UPnPDevice device, List<UPnPService> services) {
            // Discover services:
            services.AddRange(device.Services);

            foreach (UPnPDevice childDevice in device.EmbeddedDevices) {
                DiscoverDeviceServices(childDevice, services);
            }
        }

        public void Start() {
            OnProgressUpdate("[UPNP] Beginning discovery scan.");

            // Start timer:
            timeoutTimer.Start();

            // Setup the UPnP Smart Control Point:
            scp = new UPnPSmartControlPoint(OnDeviceDiscovery);

        }
    }
}
