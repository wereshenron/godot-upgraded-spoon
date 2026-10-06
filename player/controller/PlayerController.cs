using Godot;
using upgradedspoon.player.interaction;
using upgradedspoon.player.stats;

namespace upgradedspoon.player.controller;

public partial class PlayerController : CharacterBody3D
{
	private Camera3D _camera;
	private Node3D _cameraPivot;
	private Node3D _pivot;
	private Interactor _interactor;

	[Export] public float MouseSensitivity = 0.003f;
	[Export] public float JumpForce = 8.0f;
	[Export] public float SprintMultiplier = 1.6f;
	[Export] public float CameraSmoothing = 0.12f;

	[Export] public Resource PlayerStats = GD.Load<Resource>("res://player/stats/PlayerStats.tres");
	[Export] public float TiltLimit = Mathf.DegToRad(75f);
	[Export] public float TurnSpeed = 4.8f;
	[Export] public float FallAcceleration = 75.0f;
	[Export] public float PushForce = 5.0f;
	[Export] public float RecoilRecoverSpeed = 4.0f;
	[Export] public float RecoilSnap = 12.0f;

	private float _speed;
	private Vector3 _targetVelocity = Vector3.Zero;
	private float _targetCamX = 0.0f;
	private float _targetCamY = 0.0f;
	private bool _isSprinting = false;
	private Vector3 _cameraRecoilOffset = Vector3.Zero;
	private Vector3 _cameraRecoilTarget = Vector3.Zero;

	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("CameraPivot/Camera3D");
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		_pivot = GetNode<Node3D>("Pivot");
		_interactor = GetNode<Interactor>("Interactor");

		if (PlayerStats is PlayerStats playerStats)
		{
			_speed = playerStats.Speed;
			_interactor.PlayerStats = playerStats;
		}

		// GetNode<SignalBus>("/root/SignalBus").RecoilKicked += OnRecoilKicked;
		GetNode<RayCast3D>("CameraPivot/Camera3D/AimRaycast").AddException(this);
	}

	private void OnRecoilKicked(Vector3 amount, Vector3 maxOffset)
	{
		_cameraRecoilTarget += amount;
		_cameraRecoilTarget.X = Mathf.Clamp(_cameraRecoilTarget.X, -maxOffset.X, maxOffset.X);
		_cameraRecoilTarget.Y = Mathf.Clamp(_cameraRecoilTarget.Y, -maxOffset.Y, maxOffset.Y);
		_cameraRecoilTarget.Z = Mathf.Clamp(_cameraRecoilTarget.Z, -maxOffset.Y, maxOffset.Z);
	}

	// Commented out because this is how you can "run through" objects - keeping for testing
	// public override void _Process(double delta)
	// {
	// 	SetCollisionMaskValue(16, !_isSprinting);
	// }

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			_targetCamX = Mathf.Clamp(
				_targetCamX - motion.Relative.Y * MouseSensitivity,
				-TiltLimit,
				TiltLimit
			);
			_targetCamY += -motion.Relative.X * MouseSensitivity;
		}
	}

	public override void _PhysicsProcess(double deltaD)
	{
		float delta = (float)deltaD;

		// Aim camera by lerp
		float t = 1.0f - Mathf.Pow(1.0f - CameraSmoothing, delta * 60.0f);
		Vector3 camRot = _cameraPivot.Rotation;
		camRot.X = Mathf.Lerp(camRot.X, _targetCamX, t);
		camRot.Y = Mathf.Lerp(camRot.Y, _targetCamY, t);

		_cameraRecoilTarget = _cameraRecoilTarget.MoveToward(Vector3.Zero, RecoilRecoverSpeed * delta);
		_cameraRecoilOffset = _cameraRecoilOffset.Lerp(_cameraRecoilTarget, RecoilSnap * delta);

		camRot.X += _cameraRecoilOffset.X;
		camRot.Y -= _cameraRecoilOffset.Y;
		_cameraPivot.Rotation = camRot;

		Vector3 direction = Vector3.Zero;
		float runningSpeed = _speed;

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
			Basis cameraBasis = new Basis(Vector3.Up, _camera.GlobalTransform.Basis.GetEuler().Y);

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
		_targetVelocity.X = Mathf.Lerp(_targetVelocity.X, direction.X * runningSpeed, t);
		_targetVelocity.Z = Mathf.Lerp(_targetVelocity.Z, direction.Z * runningSpeed, t);

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			_targetVelocity.Y = JumpForce;
		}

		// Vertical Velocity
		if (!IsOnFloor())
		{
			_targetVelocity.Y = Mathf.Lerp(_targetVelocity.Y, _targetVelocity.Y - (FallAcceleration * delta), t);
		}

		Velocity = _targetVelocity;
		MoveAndSlide();
	}
}
