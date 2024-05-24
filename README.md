# Lattice Visual Scripting
Lattice is a visual gameplay programming system for Unity's ECS. It aims to be:
 - **Productive**: Lattice aims to minimize the time from design to implementation.
 - **Flexible**: Nodes are written with simple C# functions.
 - **Performant**: Lattice is compiled directly to .NET IL. 

## Status
Lattice is in preview and currently being used in production for Pontoco's upcoming projects. As improvement are made, new versions will be published to this public repository.

Follow along with development on the [Unity Forum Thread](https://forum.unity.com/threads/lattice-visual-scripting-for-ecs.1508402/), or on [Discord](https://discord.com/invite/Qx4aX6Xkxr).

## Installation
Add this git repository in the Unity Package Manager.

## Package Assemblies
 - Lattice.Runtime: This is the core Lattice compiler and executor. It also contains the serialized asset formats for graphs.
 - Lattice.StandardLibrary: A default set of nodes and extensions that projects can use. Most common nodes are exposed here.
 - Lattice.Editor: The Unity Editor UI and all workflows associated.
 