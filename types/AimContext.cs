using Godot;

namespace upgradedspoon.types;

public struct AimContext
{
    public AimContext(Vector3 direction, double strengthMultiplier)
    {
        Direction = direction;
        StrengthMultiplier = strengthMultiplier;
    }

    public double StrengthMultiplier { get; set; }

    public Vector3 Direction { get; set; }
}