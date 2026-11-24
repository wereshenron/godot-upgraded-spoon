using Godot;

namespace upgradedspoon.chunks;

[GlobalClass]
public partial class ChunkRegistry : Resource
{
    [Export] public ChunkData[] Chunks { get; set; }
    [Export] public ChunkData[] FixedStart { get; set; } = [];
    
    public ChunkRegistry() {}
}