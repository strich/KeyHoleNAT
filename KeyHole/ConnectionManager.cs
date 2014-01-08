using System;
using System.Collections.Generic;
using System.Linq;
using OpenSource.UPnP;

namespace KeyHole {
	public class ConnectionManager {
		private readonly List<DeviceServices> activeDevices = new List<DeviceServices>();
		                                      // Devices listed here have an active port map on them 

		private UPnPSmartControlPoint scp;

		public void CreateServer() {
			// Setup a timer to limit how long we should wait for UPnP to discover devices:
			var timer = new SafeTimer(5000, false);
			timer.OnElapsed += OnUPnPDiscoveryPhaseEnd;
			timer.Start();

			// Setup the UPnP Smart Control Point:
			scp = new UPnPSmartControlPoint(OnDeviceDiscovery);

			Console.WriteLine("######## Beginning UPnP Scan ########");
		}

		private void OnUPnPDiscoveryPhaseEnd() {
			// Attempt to add a port mapping to the devices found:
			Console.WriteLine("######## Beginning UPnP Port Mapping Phase ########");

			foreach (DeviceServices device in activeDevices) {
				UPnPService service = device.Services.First(s => s.ServiceID == "urn:upnp-org:serviceId:WANIPConn1" ||
				                                                 s.ServiceID == "urn:upnp-org:serviceId:WANIPConn2");

				UPnPAction portMappingAction = service.GetAction("AddPortMapping");
				var upnpArgs = new List<UPnPArgument>();

				var newRemoteHost = new UPnPArgument("NewRemoteHost", "");
				upnpArgs.Add(newRemoteHost);
				var NewExternalPort = new UPnPArgument("NewExternalPort", (UInt16)11112);
				upnpArgs.Add(NewExternalPort);
				var NewProtocol = new UPnPArgument("NewProtocol", "TCP");
				upnpArgs.Add(NewProtocol);
				var NewInternalPort = new UPnPArgument("NewInternalPort", (UInt16)11112);
				upnpArgs.Add(NewInternalPort);
				var NewInternalClient = new UPnPArgument("NewInternalClient", "192.168.0.90");
				upnpArgs.Add(NewInternalClient);
				var NewEnabled = new UPnPArgument("NewEnabled", true);
				upnpArgs.Add(NewEnabled);
				var NewPortMappingDescription = new UPnPArgument("NewPortMappingDescription", "WFTO Server Port Map");
				upnpArgs.Add(NewPortMappingDescription);
				var NewLeaseDuration = new UPnPArgument("NewLeaseDuration", (UInt32)3600);
				upnpArgs.Add(NewLeaseDuration);

				try {
					service.InvokeAsync(portMappingAction.Name, upnpArgs.ToArray(), null,
					                    HandleInvoke,
					                    HandleInvokeError);
				} catch (UPnPInvokeException ie) {
					Console.WriteLine("UPnPInvokeException: " + ie.Message);
				}
			}
		}

		private void HandleInvokeError(UPnPService sender, string methodname, UPnPArgument[] args, UPnPInvokeException e,
		                               object tag) {
			if (e.UPNP == null) Console.WriteLine("Invocation error: " + e);
			else Console.WriteLine("Invocation Error Code " + e.UPNP.ErrorCode.ToString() + " : " + e.UPNP.ErrorDescription);
		}

		private void HandleInvoke(UPnPService sender, string methodname, UPnPArgument[] args, object returnvalue, object tag) {
			Console.WriteLine("Invocation complete.");
		}

		private void OnDeviceDiscovery(UPnPSmartControlPoint sender, UPnPDevice device) {
			Console.WriteLine("New Device discovery: " + device.FriendlyName);
			Console.WriteLine("	Type: " + device.StandardDeviceType);

			var services = new List<UPnPService>();

			// Services can be within embedded devices within the root parent devices.
			//  Discover all services within the device and list as a flat tree:
			DiscoverDeviceServices(device, services);

			// Check if it contains the port mapping services and save it for later if it does:
			if (services.Count(service => service.ServiceID == "urn:upnp-org:serviceId:WANIPConn1" ||
			                              service.ServiceID == "urn:upnp-org:serviceId:WANIPConn2") != 0) {
				var ds = new DeviceServices {Device = device, Services = services};
				activeDevices.Add(ds);
			}

			Console.WriteLine("	Finished discovering device services.");
		}

		private void DiscoverDeviceServices(UPnPDevice device, List<UPnPService> services) {
			// Discover services:
			services.AddRange(device.Services);

			foreach (UPnPDevice childDevice in device.EmbeddedDevices) {
				DiscoverDeviceServices(childDevice, services);
			}
		}

		~ConnectionManager() {
			// TODO: Delete the UPnP port mapping.
		}
	}

	internal struct DeviceServices {
		public UPnPDevice Device;
		public List<UPnPService> Services;
	}
}