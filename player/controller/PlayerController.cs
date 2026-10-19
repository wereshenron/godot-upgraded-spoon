using Godot;
using upgradedspoon.player.interaction;
using upgradedspoon.player.stats;
using Camera = upgradedspoon.player.camera.Camera;

namespace upgradedspoon.player.controller;

public partial class PlayerController : CharacterBody3D
{
	private Node3D _pivot;
	private Interactor _interactor;
	
	[Export] public Camera Camera { get; set; }
	[Export] public float JumpForce = 8.0f;
	[Export] public float SprintMultiplier = 1.6f;
	[Export(PropertyHint.Range, "1,3")] public float FallGravityMultiplier = 1.4f;
	[Export] public float TerminalVelocity = 50.0f;
	[Export(PropertyHint.Range, "0.01,1")] public float Acceleration = 0.12f;

	[Export] public PlayerStats PlayerStats { get; set; }
	[Export] public float TiltLimit = Mathf.DegToRad(75f);
	[Export] public float TurnSpeed = 4.8f;
	[Export] public float FallAcceleration = 75.0f;
	[Export] public float PushForce = 5.0f;
	[Export] public float RecoilRecoverSpeed = 4.0f;
	[Export] public float RecoilSnap = 12.0f;

	private float _speed;
	private Vector3 _targetVelocity = Vector3.Zero;
	private bool _isSprinting;

	public override void _Ready()
	{
		_pivot = GetNode<Node3D>("Pivot");
		_interactor = GetNode<Interactor>("Interactor");

		if (PlayerStats is PlayerStats playerStats)
		{
			_speed = playerStats.Speed;
			_interactor.PlayerStats = playerStats;
		}

		Camera.AimRaycast.AddException(this);
		Camera.PickupRaycast.AddException(this);
	}

	public override void _PhysicsProcess(double deltaD)
	{
		var delta = (float)deltaD;

		var direction = Vector3.Zero;
		var runningSpeed = _speed;

		if (Input.IsActionPressed("move_right")) direction.X += 1;
		if (Input.IsActionPressed("move_left")) direction.X -= 1;
		if (Input.IsActionPressed("move_back")) direction.Z -= 1;
		if (Input.IsActionPressed("move_forward")) direction.Z += 1;

		if (Input.IsActionPressed("sprint"))
		{
			runningSpeed *= SprintMultiplier;
			_isSprinting = true;
		}
		else
		{
			_isSprinting = false;
		}

		if (direction != Vector3.Zero)
		{
			Basis cameraBasis = new Basis(Vector3.Up, Camera.GlobalTransform.Basis.GetEuler().Y);

			Vector3 forward = -cameraBasis.Z.Normalized();
			Vector3 right = cameraBasis.X.Normalized();
			direction = (right * direction.X + forward * direction.Z).Normalized();

			Vector3 lookDirection = new Vector3(direction.X, 0, direction.Z);
			Basis current = _pivot.Basis.Orthonormalized();
			Basis target = Basis.LookingAt(lookDirection).Orthonormalized();
			target = new Basis(target.X.Normalized(), Vector3.Up, target.Z.Normalized());
			_pivot.Basis = current.Slerp(target, delta * TurnSpeed);
		}

		// Ground Velocity
		float groundT = 1.0f - Mathf.Pow(1.0f - Acceleration, delta * 60.0f);
		_targetVelocity.X = Mathf.Lerp(_targetVelocity.X, direction.X * runningSpeed, groundT);
		_targetVelocity.Z = Mathf.Lerp(_targetVelocity.Z, direction.Z * runningSpeed, groundT);


		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			_targetVelocity.Y = JumpForce;
		}

		// Vertical Velocity
		if (!IsOnFloor())
		{
			_targetVelocity.Y -= FallAcceleration * delta;
		}
		else if (_targetVelocity.Y < 0)
		{
			_targetVelocity.Y = 0; // reset on landing so stale fall speed doesn't carry over
		}

		Velocity = _targetVelocity;
		MoveAndSlide();
	}
}
