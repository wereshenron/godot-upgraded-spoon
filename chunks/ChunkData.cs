using Godot;

namespace upgradedspoon.chunks;

[GlobalClass]
public partial class ChunkData : Resource
{
    [Export] public PackedScene ChunkScene { get; set; }
    [Export] public StringName ChunkType { get; set; }
    [Export] public int TierMin { get; set; }
    [Export] public int TierMax { get; set; }

    public ChunkData() {}
}