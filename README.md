#KeyHoleNAT

Lightweight implementation of the STUN method of port binding for NAT. Written in C# and cross-platform.
Most of the STUN implementation is being done by Mono.NAT as its a good lightweight and cross-platform UPnP library.
I have wrapped Mono.NAT up and abstracted it away from the user facing implementation so I can add more port punching functionality later such as TURN.

##Dependencies

The only dependency the KeyHoleNAT library currently has is Mono.NAT. The offical Mono.NAT and sadly outdated and currently abandoned. Please use the more updated version of Mono.NAT here: https://github.com/nterry/Mono.Nat
Both Mono.NAT and KeyHoleNAT can compile in Mono and .NET 2.0 up to v4.5.

##Quick Start

Quick and easy way to make a best effort attempt at binding a port:
```C#
  // Initialize the controller:
  NATController nc = new NATController();
  
  // The BindPort method is async and will not block the application:
  nc.BindPort(
    portToBind: 17562, // Port to attempt to bind to
    portDescription: "Port for MyApplication", // An optional description that can be seen in a UPnP devices GUI.
    ipProtocol: IPProtocol.Both); // Specify TCP, UDP or both protocols to bind to.
```

For a more advanced implementation exposing all options you can review the example application [here](https://github.com/strich/KeyHoleNAT/blob/master/KeyHoleTests/Program.cs).
