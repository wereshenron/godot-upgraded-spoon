using System.Collections.Generic;
using Godot;
using upgradedspoon.player.interaction;
using upgradedspoon.player.stats;
using Camera = upgradedspoon.player.camera.Camera;

namespace upgradedspoon.player.controller;

public partial class PlayerController : CharacterBody3D
{
	
	[Export] public float JumpForce = 8.0f;
	[Export] public float SprintMultiplier = 1.6f;
	[Export(PropertyHint.Range, "1,3")] public float FallGravityMultiplier = 1.4f;
	[Export(PropertyHint.Range, "0.01,1")] public float Acceleration = 0.12f;

	[ExportGroup("Node References")]
	[Export] public Camera Camera { get; set; }
	[Export] public Node3D Pivot { get; set; }
	[Export] public Interactor Interactor { get; set; }
	[Export] public PlayerStats PlayerStats { get; set; }
	[Export] public float TurnSpeed = 4.8f;
	[Export] public float FallAcceleration = 75.0f;

	private Vector3 _direction;
	private float _speed;
	private float _runningSpeed;
	private Vector3 _targetVelocity = Vector3.Zero;
	private bool _isSprinting;

	private static readonly StringName MoveLeft = new("move_left");
	private static readonly StringName MoveRight = new("move_right");
	private static readonly StringName MoveForward = new("move_forward");
	private static readonly StringName MoveBack = new("move_back");
	private static readonly StringName Sprint = new("sprint");
	private static readonly StringName Jump = new("jump");

	public override void _Ready()
	{
		_direction = Vector3.Zero;

		if (PlayerStats is { } playerStats)
		{
			_speed = playerStats.Speed;
			Interactor.PlayerStats = playerStats;
		}

		Camera.AimRaycast.AddException(this);
		Camera.PickupRaycast.AddException(this);
	}

	public override void _PhysicsProcess(double deltaD)
	{
		// using var _ = GcWatcher.Track("Player._PhysicsProcess");
		var delta = (float)deltaD;
		_runningSpeed = _speed;

		_direction = Vector3.Zero;

		_direction.X += Input.GetAxis(
			MoveLeft,
			MoveRight
		);
		
		_direction.Z += Input.GetAxis(
			MoveBack,
			MoveForward
		);

		if (Input.IsActionPressed(Sprint))
		{
			_runningSpeed *= SprintMultiplier;
			_isSprinting = true;
		}
		else
		{
			_runningSpeed = _speed;
			_isSprinting = false;
		}

		if (_direction != Vector3.Zero)
		{
			var cameraBasis = new Basis(Vector3.Up, Camera.GlobalTransform.Basis.GetEuler().Y);

			var forward = -cameraBasis.Z.Normalized();
			var right = cameraBasis.X.Normalized();
			_direction = (right * _direction.X + forward * _direction.Z).Normalized();

			var lookDirection = new Vector3(_direction.X, 0, _direction.Z);
			var current = Pivot.Basis.Orthonormalized();
			var target = Basis.LookingAt(lookDirection).Orthonormalized();
			target = new Basis(target.X.Normalized(), Vector3.Up, target.Z.Normalized());
			Pivot.Basis = current.Slerp(target, delta * TurnSpeed);
		}

		// Ground Velocity
		var groundT = 1.0f - Mathf.Pow(1.0f - Acceleration, delta * 60.0f);
		_targetVelocity.X = Mathf.Lerp(_targetVelocity.X, _direction.X * _runningSpeed, groundT);
		_targetVelocity.Z = Mathf.Lerp(_targetVelocity.Z, _direction.Z * _runningSpeed, groundT);

		if (Input.IsActionJustPressed(Jump) && IsOnFloor())
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
