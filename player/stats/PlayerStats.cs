using Godot;

namespace upgradedspoon.player.stats;

[GlobalClass]
public partial class PlayerStats : Resource
{
	[Export] public float Speed;
	[Export] public float SprintMultiplier;
	[Export] public float StrengthMultiplier;

	public PlayerStats() : this(0.0f, 0.0f, 0.0f)
	{
	}

	public PlayerStats(float speed, float sprintMultiplier, float strengthMultiplier)
	{
		Speed = speed;
		SprintMultiplier = sprintMultiplier;
		StrengthMultiplier = strengthMultiplier;
	}
}
