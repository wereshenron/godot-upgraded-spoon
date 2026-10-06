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
    [Export] public double AimTransitionTime { get; set; } = 0.15;
    [Export] public GunSettings GunSettings { get; set; }

    /// Latest aim from the interactor; states read this when firing.
    public AimContext CurrentAim { get; private set; }
    
    public int Ammo { get; private set; }
    public bool CanReload => Ammo < GunSettings.MagazineSize;

    private Node3D _bulletPivot;

    /// 0 = hold_offset, 1 = aim_offset
    private float _aimBlend;

    private uint _initialLayer;
    private uint _initialMask;
    private Vector3 _targetRecoilOffset;

    private ShootableState CurrentState => StateMachine.CurrentState as ShootableState;

    public override void _Ready()
    {
        base._Ready();

        _bulletPivot = Body.GetNode<Node3D>("BulletPivot");
        _initialLayer = Body.CollisionLayer;
        _initialMask = Body.CollisionMask;

        StateMachine.AddState("Firing", new ShootableFiringState());
        StateMachine.AddState("Idle", new ShootableIdleState());
        StateMachine.Enter("Idle");

        Grabbed += (_, _) => OnGrabbed();
        Released += (_, _) => OnReleased();
    }

    public override void _PhysicsProcess(double delta)
    {
        var target = IsAiming ? 1f : 0f;
        var step = AimTransitionTime > 0 ? (float)(delta / AimTransitionTime) : 1f;
        _aimBlend = Mathf.MoveToward(_aimBlend, target, step);
    }

    public void Shoot()
    {
        Ammo--;
        ApplyRecoilKick();
        LaunchBullet(CurrentAim.Direction);
    }
    
    public void RefillMagazine() => Ammo = GunSettings.MagazineSize;


    private void ApplyRecoilKick()
    {
        var min = GunSettings.MinRecoilAmount;
        var max = GunSettings.RecoilAmount;

        var applied = new Vector3(
            (float)Random.Shared.NextDouble() * (max.X - min.X) + min.X,
            (float)Random.Shared.NextDouble() * (max.Y - min.Y) + min.Y,
            (float)Random.Shared.NextDouble() * (max.Z - min.Z) + min.Z);

        SignalBus.Instance.RaiseRecoilKicked(applied, GunSettings.MaxRecoilOffset);
        _targetRecoilOffset += applied;
    }

    private void LaunchBullet(Vector3 direction)
    {
        var bullet = GunSettings.BulletScene.Instantiate<RigidBody3D>();
        GetTree().Root.AddChild(bullet);
        bullet.GlobalPosition = _bulletPivot.GlobalPosition;
        bullet.LinearVelocity = direction * GunSettings.FireVelocity;
    }

    // Input from the interactor
    public override void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
    {
        CurrentAim = ctx;

        if (slot == UseSlot.Secondary)
            HandleAim(phase);

        CurrentState?.OnAction(slot, phase, delta, ctx);
    }

    private void HandleAim(InputPhase phase)
    {
        switch (phase)
        {
            case InputPhase.Pressed:
                IsAiming = true;
                SignalBus.Instance.RaiseShowReticle(false);
                break;
            case InputPhase.Released:
                IsAiming = false;
                SignalBus.Instance.RaiseShowReticle(true);
                break;
            case InputPhase.Held:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
        }
    }

    public override Vector3 GetHoldOffset() => HoldOffset.Lerp(AimOffset, _aimBlend);

    private void OnGrabbed()
    {
        Body.Freeze = true;
        Body.CollisionLayer = 0;
        Body.CollisionMask = 0;
        SignalBus.Instance.RaiseLookedAway();
    }

    private void OnReleased()
    {
        CurrentState?.Cancelled();
        IsAiming = false;
        SignalBus.Instance.RaiseShowReticle(true);

        Body.Freeze = false;
        Body.CollisionLayer = _initialLayer;
        Body.CollisionMask = _initialMask;
        SignalBus.Instance.RaiseLookedAway();
    }
}