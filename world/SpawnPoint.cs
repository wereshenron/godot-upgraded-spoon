using Godot;
using upgradedspoon.globals;

namespace upgradedspoon.world;

public partial class SpawnPoint : Marker3D
{
	[Export] public string SpawnId = "default";

	public override void _Ready()
	{
		GameManager.Instance.SpawnPlayer(SpawnId);
	}
}
