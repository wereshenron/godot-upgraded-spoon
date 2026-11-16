using Godot;
using Godot.Collections;
using upgradedspoon.globals;
using upgradedspoon.player.interaction;
using upgradedspoon.traits;
using Interactable = upgradedspoon.traits.interactable.Interactable;

namespace upgradedspoon.interactables;

public partial class Door : Interactable
{
    [Export] public Node3D SwingPivot;
    [Export] public float OpenAngleDegrees = 90f;
    [Export] public float Duration = 0.5f;

    private bool _opened;
    private float _closedY;
    private Tween _tween;
    private bool _transitioning;

    public override void _Ready()
    {
        base._Ready();
        _closedY = SwingPivot.Rotation.Y;
    }

    public override void Interact(Interactor interactor, Dictionary msg = null)
    {
        if (_transitioning) return;
        _transitioning = true;
        
        _opened = !_opened;
        Action = _opened ? "Shut" : "Open";
        // SignalBus.Instance.RaiseInteractableSeen(this);

        var target = _closedY + (_opened ? Mathf.DegToRad(OpenAngleDegrees) : 0f);

        _tween?.Kill(); // interrupt mid-swing so spamming interact doesn't stack tweens
        _tween = CreateTween();
        _tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(SwingPivot, "rotation:y", target, Duration);
        _tween.TweenCallback(Callable.From(() => _transitioning = false));
    }
}