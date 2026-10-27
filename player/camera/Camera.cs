using Godot;
using upgradedspoon.globals;

namespace upgradedspoon.player.camera;

public partial class Camera : Camera3D
{
    [ExportGroup("Camera")]
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public float CameraSmoothing = 0.12f;
    [Export] public float TiltLimit = Mathf.DegToRad(75f);
    [Export] public float StickSensitivity = 3f;

    [ExportGroup("Recoil")]
    [Export] public float RecoilRecoverSpeed = 4.0f;
    [Export] public float RecoilSnap = 12.0f;
    
    [ExportGroup("Node References")]
    [Export] public RayCast3D PickupRaycast;
    [Export] public RayCast3D AimRaycast;

    // Where the player is aiming (raw input)
    private float _targetPitch;
    private float _targetYaw;

    // Smoothed aim, with no recoil mixed in
    private float _pitch;
    private float _yaw;

    private Vector3 _recoilOffset = Vector3.Zero;
    private Vector3 _recoilTarget = Vector3.Zero;
    private Vector2 _look = Vector2.Zero;

    /// <summary>Smoothed horizontal look angle (no recoil). Use this for movement direction.</summary>
    public float Yaw => _yaw;

    public override void _Ready()
    {
        _pitch = _targetPitch = Rotation.X;
        _yaw = _targetYaw = Rotation.Y;

        SignalBus.Instance.RecoilKicked += OnRecoilKicked;
    }

    public override void _ExitTree()
    {
        if (SignalBus.Instance != null)
            SignalBus.Instance.RecoilKicked -= OnRecoilKicked;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion motion || Input.MouseMode != Input.MouseModeEnum.Captured) return;

        _targetPitch = Mathf.Clamp(_targetPitch - motion.Relative.Y * MouseSensitivity, -TiltLimit, TiltLimit);
        _targetYaw -= motion.Relative.X * MouseSensitivity;
    }

    public override void _PhysicsProcess(double deltaD)
    {
        var delta = (float)deltaD;
        var t = 1.0f - Mathf.Exp(-CameraSmoothing * delta);

        _pitch = Mathf.Lerp(_pitch, _targetPitch, t);
        _yaw = Mathf.Lerp(_yaw, _targetYaw, t);
        
        _recoilTarget = _recoilTarget.MoveToward(Vector3.Zero, RecoilRecoverSpeed * delta);
        _recoilOffset = _recoilOffset.Lerp(_recoilTarget, RecoilSnap * delta);

        // write to the pivot (or to Rotation if you dropped the pivot)
        Rotation = new Vector3(
            _pitch + _recoilOffset.X,
            _yaw - _recoilOffset.Y,
            0f);
    }
    
    
    public override void _Process(double delta)
    {
        _look = Input.GetVector("look_left", "look_right", "look_up", "look_down");
        if (_look == Vector2.Zero) return;
        // GD.Print(look);

        // Optional: response curve for finer control near the center
        _look = _look.Normalized() * Mathf.Pow(_look.Length(), 2f);

        var dt = (float)delta;
        _targetPitch = Mathf.Clamp(_targetPitch - _look.Y * StickSensitivity * dt, -TiltLimit, TiltLimit);
        _targetYaw -= _look.X * StickSensitivity * dt;
    }

    private void OnRecoilKicked(object sender, RecoilKickedArgs args)
    {
        _recoilTarget += args.Amount;
        _recoilTarget.X = Mathf.Clamp(_recoilTarget.X, -args.MaxOffset.X, args.MaxOffset.X);
        _recoilTarget.Y = Mathf.Clamp(_recoilTarget.Y, -args.MaxOffset.Y, args.MaxOffset.Y);
    }
}