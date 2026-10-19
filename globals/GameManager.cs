using Godot;
using upgradedspoon.player.controller;
using upgradedspoon.world;

namespace upgradedspoon.globals;

public partial class GameManager : Node
{
	private PackedScene _playerScene = GD.Load<PackedScene>("res://player/Player.tscn");
	private PackedScene[] _globalScenes;
	public PlayerController Player;
	
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
		Player = _playerScene.Instantiate<PlayerController>();
		AddChild(Player);

		var target = FindSpawnPoint(spawnId);
		if (target is null) return;

		Player.GlobalPosition = target.GlobalPosition;
		Player.Rotation = target.Rotation;
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
