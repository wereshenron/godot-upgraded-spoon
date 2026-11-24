using Godot;
using upgradedspoon.globals;

namespace upgradedspoon.chunks.generation;

public partial class Level : Node3D
{
    public override void _Ready()
    {
        var gameManager = GameManager.Instance;
        GetNode<LevelGenerator>("LevelGenerator")
        .Generate(gameManager.ChunkRegistry.FixedStart, gameManager.ChunkRegistry.Chunks, gameManager.RNG.Seed);
    }
}