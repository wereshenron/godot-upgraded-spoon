using Godot;

namespace upgradedspoon.types;

public struct AimContext
{
    public AimContext(Vector3 direction, float strengthMultiplier)
    {
        Direction = direction;
        StrengthMultiplier = strengthMultiplier;
    }

    public float StrengthMultiplier { get; set; }

    public Vector3 Direction { get; set; }
}