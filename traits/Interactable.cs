using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using upgradedspoon.player.interaction;

namespace upgradedspoon.traits;

public abstract partial class Interactable : Node3D
{
    [Export] public GeometryInstance3D[] GeometryList;
    [Export] public string Action;
    [Export] public string Title;

    private static readonly StringName BlendParam = "blend";
    private ShaderMaterial _outlineShaderMaterial;
    private Tween _tween;
    private bool _highlighted;
    private Callable _setBlend;

#nullable enable
    public virtual void Interact(Interactor interactor, Dictionary? msg = null)
    {
    }

    public override void _Ready()
    {
        GetParentNode3D().AddToGroup("Interactable");
        _outlineShaderMaterial = GD.Load<ShaderMaterial>("res://textures/materials/outline.tres");
        _setBlend = Callable.From<double>(SetBlend);
        globals.SignalBus.Instance.LookedAway += (_, _) => { SetHighlight(false); };
    }

    public void SetHighlight(bool highlight)
    {
        if (highlight == _highlighted) return;
        _highlighted = highlight;
        
        foreach (var t in GeometryList)
        {
            if (t == null) return;
            if (highlight)
            {
                SetOverlay(_outlineShaderMaterial);
                StartTween(0.0, 1.0);
            }
            else
            {
                var blendVal = _outlineShaderMaterial.GetShaderParameter("blend").AsDouble();
                StartTween(blendVal, 0.0, () => SetOverlay(null));
            }
        }
    }

    private void StartTween(double from, double to, Action? onComplete = null)
    {
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenMethod(_setBlend, from, to, 0.3f);
        if (onComplete != null) _tween.TweenCallback(Callable.From(onComplete));
    }

    private void SetBlend(double value)
    {
        _outlineShaderMaterial.SetShaderParameter(BlendParam, value);
    }

    private void SetOverlay(Material? m)
    {
        foreach (var g in GeometryList)
        {
            if (g == null) continue;
            g.MaterialOverlay = m;
        }
    }
}