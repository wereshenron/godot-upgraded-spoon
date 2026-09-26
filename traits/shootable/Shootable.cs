using System;
using Godot;
using upgradedspoon.globals;
using upgradedspoon.state;
using upgradedspoon.traits.shootable.gun_settings;
using upgradedspoon.traits.shootable.states;
using upgradedspoon.types;

namespace upgradedspoon.traits.shootable;

public partial class Shootable : Holdable
{
    [Export] public ShootableStateMachine StateMachine { get; set; }
    [Export] public Material TracerMaterial { get; set; }
    [Export] public Vector3 AimOffset { get; set; }
    [Export] public double AimTransitionTime { get; set; }
    [Export] public GunSettings GunSettings { get; set; }

    private RayCast3D _aimRayCast; // TODO - add to _ready
    private PackedScene _bullet; // TODO - add to _ready
    private Node3D _bulletPivot; // TODO - add to _ready

    /// # 0 = hold_offset, 1 = aim_offset
    private float _aimBlend;

    private uint _initialLayer;
    private uint _initialMask;
    private Vector3 _currentRecoilOffset;
    private Vector3 _targetRecoilOffset;
    private Vector3 _direction = Vector3.Zero;
    private Vector3 _shootAimPoint = Vector3.Zero;

    public override void _Ready()
    {
        base._Ready();

        // On Readies
        _aimRayCast = GetViewport().GetCamera3D().GetNode<RayCast3D>("AimRaycast");
        _bullet = GunSettings.BulletScene;
        _bulletPivot = Body.GetNode<Node3D>("BulletPivot");

        // Initialize State
        StateMachine.AddState("Firing", new ShootableFiringState());

        // Event Handlers
        Grabbed += (_, _) => { OnGrabbed(); };
        Released += (_, _) => { OnReleased(); };

        _aimRayCast.AddException(Body);
    }

    public void Shoot()
    {
        var shootDirection = _shootAimPoint == Vector3.Zero ? _direction.Normalized() : (_shootAimPoint - _aimRayCast.GlobalPosition).Normalized();
        ApplyRecoilKick();
        LaunchBullet(shootDirection);
    }

    private void ApplyRecoilKick()
    {
        var minRecoil = GunSettings.MinRecoilAmount;
        var recoil = GunSettings.RecoilAmount;

        var recoilX = (float)Random.Shared.NextDouble() * (recoil.X - minRecoil.X) + minRecoil.X;
        var recoilY = (float)Random.Shared.NextDouble() * (recoil.Y - minRecoil.Y) + minRecoil.Y;
        var recoilZ = (float)Random.Shared.NextDouble() * (recoil.Z - minRecoil.Z) + minRecoil.Z;
        var recoilApplied = new Vector3(recoilX, recoilY, recoilZ);
        SignalBus.Instance.RaiseRecoilKicked(recoilApplied, GunSettings.MaxRecoilOffset);
        _targetRecoilOffset += recoilApplied;
    }

    private void LaunchBullet(Vector3 launchDirection)
    {
        var bulletScene = GD.Load<PackedScene>(GunSettings.BulletScene.ResourcePath);
        var bullet = bulletScene.Instantiate<RigidBody3D>();
        GetTree().Root.AddChild(bullet);
        
        bullet.GlobalPosition = _bulletPivot.GlobalPosition;
        bullet.LinearVelocity = launchDirection * GunSettings.FireVelocity;
        // bullet.Damage
    }

    // Input Event handlers
    public override void PrimaryPressed(AimContext? aimContext = null)
    {
        StateMachine.CurrentState.PrimaryPressed(aimContext);
    }

    public override void PrimaryHeld(double delta, AimContext? aimContext = null)
    {
        StateMachine.CurrentState.PrimaryHeld(delta, aimContext);
    }

    public override void PrimaryReleased(AimContext? aimContext = null)
    {
        StateMachine.CurrentState.PrimaryPressed(aimContext);
    }

    public override void SecondaryPressed(AimContext? aimContext = null)
    {
        IsAiming = true;
        SignalBus.Instance.RaiseShowReticle(false);
    }

    public override void SecondaryReleased(AimContext? aimContext = null)
    {
        IsAiming = false;
        SignalBus.Instance.RaiseShowReticle(true);
    }

    // Helpers
    public override Vector3 GetHoldOffset()
    {
        return HoldOffset.Lerp(AimOffset, _aimBlend);
    }


    // Event Handler Methods
    private void OnGrabbed()
    {
        Body.Freeze = true;
        Body.CollisionLayer = 0;
        Body.CollisionMask = 0;
        SignalBus.Instance.RaiseLookedAway();
    }

    private void OnReleased()
    {
        Body.Freeze = false;
        Body.CollisionLayer = _initialLayer;
        Body.CollisionMask = _initialMask;
        SignalBus.Instance.RaiseLookedAway();
    }
}