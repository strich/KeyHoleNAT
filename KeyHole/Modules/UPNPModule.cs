using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Mono.Nat;

namespace KeyHoleNAT {
    public class UPNPModule : Module {
        // Devices listed here have an active port map on them
        private readonly List<INatDevice> _activeDevices = new List<INatDevice>();
        private static UPNPOptions _upnpOptions;
        private GlobalOptions _globalOptions;

        private static Dictionary<Mapping, PortMapDeviceState[]> _portBindRequests;

        public UPNPModule(UPNPOptions upnpOptions, GlobalOptions globalOptions, ProgressUpdateHandler onProgressUpdate,
                          ProgressUpdateHandler onProgressFinish) {
            _upnpOptions = upnpOptions;
            _globalOptions = globalOptions;
            ProgressUpdate += onProgressUpdate;
            ProgressFinish += onProgressFinish;

            _portBindRequests = new Dictionary<Mapping, PortMapDeviceState[]>();

            OnProgressUpdate("Beginning discovery scan.");

            // Setup UPnP:
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery();
        }

        private void DeviceFound(object sender, DeviceEventArgs e) {
            OnProgressUpdate("Discovered new device: " + e.Device.GetType().Name + " with external IP " +
                             e.Device.GetExternalIP());

            var pbRequestsTemp = new Dictionary<Mapping, PortMapDeviceState[]>(_portBindRequests);
            foreach(var pbRequest in pbRequestsTemp) {
                bool deviceFound = false;

                for (int i = 0; i < pbRequest.Value.Length; i++) {
                    if (pbRequest.Value[i].Device.Equals(e.Device))
                        deviceFound = true;
                }

                // Add the device:
                if (deviceFound != true) {
                    var pbList = pbRequest.Value.ToList();
                    pbList.Add(new PortMapDeviceState { Device = e.Device });
                    _portBindRequests[pbRequest.Key] = pbList.ToArray();
                }
            }

            UpdatePortMappingStateMachine();
        }

        private static void UpdatePortMappingStateMachine() {
            var pbRequestsTemp = new Dictionary<Mapping, PortMapDeviceState[]>(_portBindRequests);
            foreach (var pbRequestTemp in pbRequestsTemp) {
                for (int i = 0; i < pbRequestTemp.Value.Length; i++) {
                    var mapping = pbRequestTemp.Key;
                    var portMapDeviceState = pbRequestTemp.Value[i];

                    switch (portMapDeviceState.PortMapState) {
                        case PortMapState.None:
                            if (/*_upnpOptions.AggressivePortMap*/ false) { // This isn't working as expected right now
                                DeletePortMapping(mapping, portMapDeviceState.Device);
                                _portBindRequests[mapping][i].PortMapState = PortMapState.DeleteRequestSent;
                            } else {
                                _portBindRequests[mapping][i].PortMapState = PortMapState.DeleteRequestSuccessful;
                            }

                            break;
                        case PortMapState.DeleteRequestSent:
                            break;
                        case PortMapState.DeleteRequestSuccessful:
                            AddPortMapping(mapping, portMapDeviceState.Device);
                            _portBindRequests[mapping][i].PortMapState = PortMapState.RequestSent;
                            break;
                        case PortMapState.RequestSent:
                            break;
                        case PortMapState.RequestSuccessful:
                            OnProgressFinish(new KeyHoleEventMessage(
                                                 messageDescription: "Port map successful.",
                                                 messageCode: MessageCode.Success,
                                                 loggingLevel: EventLoggingLevel.Informational));
                            _portBindRequests[mapping][i].PortMapState = PortMapState.RequestSuccessfulDone;
                            break;
                        case PortMapState.RequestSuccessfulDone:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public void BindPort(UInt16 portToBind, IPProtocol ipProtocol, string portDescription) {
            List<PortMapDeviceState> portMapDevices = new List<PortMapDeviceState>();
            foreach (var activeDevice in _activeDevices) {
                portMapDevices.Add(new PortMapDeviceState{Device = activeDevice});
            }

            if (ipProtocol == IPProtocol.Both || ipProtocol == IPProtocol.TCP) {
                Mapping newPortMapping = new Mapping(Protocol.Tcp, portToBind, portToBind);
                try {
                    _portBindRequests.Add(newPortMapping, portMapDevices.ToArray());
                } catch {}
            }
            if (ipProtocol == IPProtocol.Both || ipProtocol == IPProtocol.UDP) {
                Mapping newPortMapping = new Mapping(Protocol.Udp, portToBind, portToBind);
                try {
                    _portBindRequests.Add(newPortMapping, portMapDevices.ToArray());
                } catch { }
            }

            UpdatePortMappingStateMachine();
        }

        private static void DeletePortMapping(Mapping mapping, INatDevice device) {
            PortMapAsyncResult pmar = new PortMapAsyncResult { Device = device, Mapping = mapping };
            device.BeginDeletePortMap(mapping, DeletePortMapCallback, pmar);
        }

        private static void DeletePortMapCallback(IAsyncResult ar) {
            INatDevice device = ((PortMapAsyncResult)ar.AsyncState).Device;
            Mapping mapping = ((PortMapAsyncResult)ar.AsyncState).Mapping;

            if (_portBindRequests.ContainsKey(mapping)) {
                var portMapDeviceStates = _portBindRequests[mapping];

                for (int i = 0; i < portMapDeviceStates.Count(); i++) {
                    if (portMapDeviceStates[i].Device.Equals(device)) {
                        portMapDeviceStates[i].PortMapState = PortMapState.DeleteRequestSuccessful;
                    }
                }

                _portBindRequests[mapping] = portMapDeviceStates;
            }

            UpdatePortMappingStateMachine();
        }

        private static void AddPortMapping(Mapping mapping, INatDevice device) {
            PortMapAsyncResult pmar = new PortMapAsyncResult { Device = device, Mapping = mapping };
            device.BeginCreatePortMap(mapping, CreatePortMapCallback, pmar);
        }


        private static void CreatePortMapCallback(IAsyncResult ar) {
            INatDevice device = ((PortMapAsyncResult)ar.AsyncState).Device;
            Mapping mapping = ((PortMapAsyncResult)ar.AsyncState).Mapping;

            if (_portBindRequests.ContainsKey(mapping)) {
                var portMapDeviceStates = _portBindRequests[mapping];

                for (int i = 0; i < portMapDeviceStates.Count(); i++) {
                    if (portMapDeviceStates[i].Device.Equals(device)) {
                        portMapDeviceStates[i].PortMapState = PortMapState.RequestSuccessful;
                    }
                }

                _portBindRequests[mapping] = portMapDeviceStates;
            }

            UpdatePortMappingStateMachine();
        }

        public override string ToString() {
            return "UPNP";
        }
    }

    struct PortMapAsyncResult {
        public Mapping Mapping;
        public INatDevice Device;
    }

    struct PortMapDeviceState {
        public INatDevice Device;
        public PortMapState PortMapState;
    }

    enum PortMapState {
        None = 0,
        DeleteRequestSent,
        DeleteRequestSuccessful,
        RequestSent,
        RequestSuccessful,
        RequestSuccessfulDone
    }
}