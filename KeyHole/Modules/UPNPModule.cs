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
            ProgressUpdateHandler onProgressFinished) {
            UPNPOptions = upnpOptions;
            ProgressUpdate += onProgressUpdate;
            ProgressFinished += onProgressFinished;

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
                var c = device.Device.LocalIPEndPoints;
                var b = device.Device.InterfaceToHost;
                var a = device.Device.RemoteEndPoint.Address;
                UPnPService service = device.Services.First(s => s.ServiceID == "urn:upnp-org:serviceId:WANIPConn1" ||
                                                                 s.ServiceID == "urn:upnp-org:serviceId:WANIPConn2");

                UPnPAction portMappingAction = service.GetAction("AddPortMapping");
                var upnpArgs = new List<UPnPArgument>();

                upnpArgs.Add(new UPnPArgument("NewRemoteHost", ""));
                upnpArgs.Add(new UPnPArgument("NewExternalPort", (UInt16)11112));
                upnpArgs.Add(new UPnPArgument("NewProtocol", "TCP"));
                upnpArgs.Add(new UPnPArgument("NewInternalPort", (UInt16)11112));
                upnpArgs.Add(new UPnPArgument("NewInternalClient", "192.168.0.90"));
                upnpArgs.Add(new UPnPArgument("NewEnabled", true));
                upnpArgs.Add(new UPnPArgument("NewPortMappingDescription", "War for the Overworld"));
                upnpArgs.Add(new UPnPArgument("NewLeaseDuration", (UInt32)3600));

                try {
                    service.InvokeAsync(portMappingAction.Name,
                                        upnpArgs.ToArray(), null,
                                        HandleInvoke,
                                        HandleInvokeError);
                } catch (UPnPInvokeException ie) {
                    OnProgressUpdate("[UPNP] UPnPInvokeException: " + ie.Message);
                    // TODO: Do a OnProgressFinish
                }
            }
        }

        private void HandleInvokeError(UPnPService sender, string methodname, UPnPArgument[] args, UPnPInvokeException e,
                                       object tag) {
            if (e.UPNP == null) {
                OnProgressUpdate("[UPNP] UPnPInvokeException: " + e);
                // TODO: Do a OnProgressFinish
            }
            else {
                OnProgressUpdate("[UPNP] UPnPInvokeException: " + e.UPNP.ErrorCode.ToString() + " : " +
                                  e.UPNP.ErrorDescription);
                // TODO: Do a OnProgressFinish
            }
        }

        private void HandleInvoke(UPnPService sender, string methodname, UPnPArgument[] args, object returnvalue, object tag) {
            OnProgressUpdate("[UPNP] UPnP Successfully mapped a port.");
            // TODO: Do a OnProgressFinish
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
