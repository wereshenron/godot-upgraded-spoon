using Godot;

namespace upgradedspoon.types;

public struct AimContext
{
    public AimContext(Vector3 origin, Vector3 cameraForward, Vector3 aimPoint)
    {
        Origin = origin;
        CameraForward = cameraForward;
        AimPoint = aimPoint;
    }

    public Vector3 AimPoint { get; set; }

    public Vector3 Origin { get; set; } = Vector3.Forward;
    
    public Vector3 CameraForward { get; set; }
}