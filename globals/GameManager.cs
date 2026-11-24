using Godot;
using upgradedspoon.chunks;
using upgradedspoon.player.controller;
using upgradedspoon.world;

namespace upgradedspoon.globals;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }
	public PlayerController Player;
	public ChunkRegistry ChunkRegistry = GD.Load<ChunkRegistry>("res://chunks/registry.tres");
	public RandomNumberGenerator RNG = new RandomNumberGenerator();
	
	private PackedScene _playerScene = GD.Load<PackedScene>("res://player/Player.tscn");

	public override void _Ready()
	{
		Instance = this;
	}

	public void SpawnPlayer(string spawnId)
	{
		Player = _playerScene.Instantiate<PlayerController>();
		AddChild(Player);

		var target = FindSpawnPoint(spawnId);
		if (target is null) return;
		GD.Print("Spawning player: " + target.GlobalPosition);

		Player.GlobalPosition = target.GlobalPosition;
		Player.Rotation = target.GlobalRotation;
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
}
