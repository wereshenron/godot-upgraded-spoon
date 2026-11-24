using Godot;

namespace upgradedspoon.chunks;

[Tool]
[GlobalClass]
public partial class Connector : Marker3D
{
    [Export] public StringName Size { get; set; } = "standard";
    public bool Used { get; set; } = false;
}