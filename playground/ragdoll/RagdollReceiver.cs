using Godot;

namespace upgradedspoon.playground.ragdoll;

public partial class RagdollReceiver : Node
{
    [Export] public PhysicalBoneSimulator3D Sim { get; set; }
    [Export] public float FullRagdollDelay { get; set; } = 0.1f;

    private bool _fullRagdoll;
    private bool _fullRagdollPending;
    private Skeleton3D _skeleton;
    
    public override void _Ready()
    {
        _skeleton = Sim.GetSkeleton();
        foreach (var child in Sim.GetChildren())
        {
            if (child is not PhysicalBone3D pb) continue;
            var id = pb.GetBoneId();
            var bone = _skeleton.GetBoneName(id);
            pb.Mass = GetBoneMass(bone);
        }
    }

    private static float GetBoneMass(string bone) => bone switch
    {
        var b when b.Contains("spine") || b.Contains("hips") => 15f,
        var b when b.Contains("thigh") => 8f,
        var b when b.Contains("calf") || b.Contains("shin") => 4f,
        var b when b.Contains("upperarm") => 3f,
        var b when b.Contains("forearm") || b.Contains("hand") => 1.5f,
        var b when b.Contains("head") => 5f,
        _ => 2f,
    };

    public async void Hit(PhysicalBone3D pb, Vector3 impulse, Vector3 worldPoint)
    {
        // Already fully ragdolled: just shove the bone
        if (_fullRagdoll)
        {
            pb.ApplyImpulse(impulse, worldPoint - pb.GlobalPosition);
            return;
        }
        
        var id = pb.GetBoneId();
        var bone = _skeleton.GetBoneName(id);

        // 1. Simulate just the hit bone (and its children) and hit it
        Sim.PhysicalBonesStartSimulation([bone]);
        pb.ApplyImpulse(impulse, worldPoint - pb.GlobalPosition);

        // 2. Only one pending full-ragdoll per enemy, no matter how many hits land
        if (_fullRagdollPending) return;
        _fullRagdollPending = true;

        await ToSignal(GetTree().CreateTimer(FullRagdollDelay), SceneTreeTimer.SignalName.Timeout);

        // The enemy may have been freed while we waited
        if (!IsInstanceValid(this) || !IsInstanceValid(Sim)) return;

        // 3. Start everything else; already-simulating bones keep their velocity
        Sim.PhysicalBonesStartSimulation();
        _fullRagdoll = true;
        _fullRagdollPending = false;
    }
}