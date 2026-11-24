using System.Collections.Generic;
using System.Linq;
using Godot;
using upgradedspoon.interactables;

namespace upgradedspoon.chunks.generation;

public partial class LevelGenerator : Node3D
{
    [Export] public PackedScene DoorScene { get; set; }

    private readonly Dictionary<ChunkBase, Aabb> _live = new();
    private readonly RandomNumberGenerator _rng = new();
    private IReadOnlyList<ChunkData> _pool;
    private (Door door, ChunkBase old)? _pendingSwap;
    private readonly Queue<ChunkData> _fixed = new();
    
    public void Generate(IReadOnlyList<ChunkData> fixedStart, IReadOnlyList<ChunkData> pool, ulong seed)
    {
        _rng.Seed = seed;
        _pool = pool;

        _fixed.Clear();
        foreach (var f in fixedStart.Skip(1)) _fixed.Enqueue(f);

        var first = fixedStart[0].ChunkScene.Instantiate<ChunkBase>();
        AddChild(first);
        _live[first] = first.LocalBounds;
        SpawnDoors(first, except: null);
    }
    
     private void SpawnDoors(ChunkBase room, Connector except)
    {
        foreach (var c in room.GetConnectors().Where(c => c != except))
        {
            var door = DoorScene.Instantiate<Door>();
            room.AddChild(door);
            door.GlobalTransform = c.GlobalTransform;
            door.Room[0] = room; door.Conn[0] = c;
            door.Opening += OnDoorOpening;
            door.Crossed += OnDoorCrossed;
        }
    }

    private void OnDoorOpening(Door door)
    {
        bool live0 = IsLive(door.Room[0]), live1 = IsLive(door.Room[1]);
        if (live0 && live1) return;                       // both sides already exist
        if (!Attach(door, side: live0 ? 1 : 0, ignore: null))
            door.Lock();                                  // nothing fit: becomes a prop door
    }

    private void OnDoorCrossed(Door door, ChunkBase from, ChunkBase to)
    {
        door.Reparent(to); // door now belongs to the room being kept

        // anything that isn't the room you left or entered is unreachable now
        foreach (var r in _live.Keys.Where(r => r != from && r != to).ToList())
        {
            _live.Remove(r);
            r.QueueFree();
        }
        _pendingSwap = (door, from);
    }

    public override void _Process(double delta)
    {
        if (_pendingSwap is not { } p || !IsHidden(p.door)) return;
        _pendingSwap = null;

        var side = p.door.Room[0] == p.old ? 0 : 1;
        if (!Attach(p.door, side, ignore: p.old)) return; // if it fails, just keep the old room
        _live.Remove(p.old);
        p.old.GetParent().RemoveChild(p.old);
        p.old.QueueFree();
    }

    // Place a random room on one side of a door, anchored to the connector on the other side.
    private bool Attach(Door door, int side, ChunkBase ignore)
    {
        var anchor = door.Conn[1 - side];
        for (var i = 0; i < 8; i++)
        {
            var chunk = _pool[_rng.RandiRange(0, _pool.Count - 1)].ChunkScene.Instantiate<ChunkBase>();
            var fits = chunk.GetConnectors().Where(c => c.Size == anchor.Size).ToList();
            if (fits.Count > 0)
            {
                var mine = fits[_rng.RandiRange(0, fits.Count - 1)];
                var flip = new Transform3D(new Basis(Vector3.Up, Mathf.Pi), Vector3.Zero);
                chunk.Transform = anchor.GlobalTransform * flip * mine.Transform.AffineInverse();
                var bounds = chunk.LocalBounds.Grow(-0.1f);

                if (!_live.Any(kv => kv.Key != ignore && kv.Value.Intersects(bounds)))
                {
                    AddChild(chunk);
                    _live[chunk] = bounds;
                    door.Room[side] = chunk;
                    door.Conn[side] = mine;
                    SpawnDoors(chunk, except: mine);
                    return true;
                }
            }
            chunk.QueueFree();
        }
        return false;
    }

    private bool IsLive(ChunkBase room) => room != null && _live.ContainsKey(room);

    // Assumes the door's origin is at the doorway's floor center
    private bool IsHidden(Door door)
    {
        var cam = GetViewport().GetCamera3D();
        var mid = door.GlobalPosition + Vector3.Up;
        var half = door.GlobalBasis.X * 0.5f;
        return !(cam.IsPositionInFrustum(mid)
              || cam.IsPositionInFrustum(mid + half)
              || cam.IsPositionInFrustum(mid - half));
    }
}