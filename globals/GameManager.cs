using Godot;
using upgradedspoon.world;

namespace upgradedspoon.globals;

public partial class GameManager : Node
{
    public PackedScene PlayerScene;

    private PackedScene[] _globalScenes;
    private Node3D _player;

    public override void _Ready()
    {
        // Initialize onReadies
        _globalScenes =
        [
            GD.Load<PackedScene>("res://hud/hud.tscn")
        ];

        InstantiateGlobalScenes();
        SpawnPlayer("default");
    }

    private void SpawnPlayer(string spawnId)
    {
        if (_player is null)
        {
            _player = PlayerScene.Instantiate<Node3D>();
            AddChild(_player);
        }

        var target = FindSpawnPoint(spawnId);
        if (target is null) return;

        _player.GlobalPosition = target.GlobalPosition;
        _player.Rotation = target.Rotation;
    }

    private Node3D FindSpawnPoint(string spawnId)
    {
        foreach (var spawnPoint in GetTree().GetNodesInGroup("SpawnPoint"))
        {
            if (spawnPoint is SpawnPoint point && point.SpawnId == spawnId)
            {
                return point;
            }
        }

        return new Node3D(); // Let's hope this never gets hit
    }

    private void InstantiateGlobalScenes()
    {
        foreach (var scene in _globalScenes)
        {
            var instance = scene.Instantiate();
            AddChild(instance);
        }
    }
}