using Godot;
using upgradedspoon.globals;

namespace upgradedspoon.hud;

public partial class HudController : CanvasLayer
{
	public static HudController Instance { get; private set; }
	private static readonly StringName Pause = "Pause";
	private bool _isPaused;
	
	
	public override void _Ready()
	{
		base._Ready();
		Instance = this;

		SignalBus.Instance.PauseStarted += OnPauseStarted;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed(Pause))
		{
			OnPauseStarted(this, !_isPaused);
		}
		else if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left })
		{
			Input.SetMouseMode(Input.MouseModeEnum.Captured);
		}
	}

	private void OnPauseStarted(object sender, bool e)
	{
		GD.Print(e);
		switch (e)
		{
			case true:
				Input.SetMouseMode(Input.MouseModeEnum.Visible);
				_isPaused = true;
				break;
			case false: 
				Input.SetMouseMode(Input.MouseModeEnum.Captured);
				_isPaused = false;
				break;
		}
	}
}
