using Godot;
using upgradedspoon.world;

namespace upgradedspoon.globals;

public partial class GameManager : Node
{
	private PackedScene _playerScene = GD.Load<PackedScene>("res://player/Player.tscn");
	private PackedScene[] _globalScenes;
	private Node3D _player;
	
	public static GameManager Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		
		// Initialize onReadies
		_globalScenes =
		[
			GD.Load<PackedScene>("res://hud/hud.tscn")
		];

		InstantiateGlobalScenes();
		// SpawnPlayer("default");
		
	}

	public void SpawnPlayer(string spawnId)
	{
		_player = _playerScene.Instantiate<Node3D>();
		AddChild(_player);

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

		GD.Print("Getting to the nasty point?");
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
