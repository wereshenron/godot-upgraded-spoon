using System.Collections.Generic;
using System.Linq;
using Godot;

namespace upgradedspoon.chunks;

[Tool]
[GlobalClass]
public partial class ChunkBase : Node3D
{
	[Export]
	public float GridUnit
	{
		get => _gridUnit;
		set
		{
			_gridUnit = value;
			UpdateDebugGrid();
			UpdateConfigurationWarnings();
		}
	}

	[Export]
	public bool ShowGrid
	{
		get => _showGrid;
		set
		{
			_showGrid = value;
			UpdateDebugGrid();
		}
	}

	[Export]
	public Aabb LocalBounds
	{
		get => _localBounds;
		set
		{
			_localBounds = value;
			UpdateDebugBox();
			UpdateDebugGrid();
			UpdateConfigurationWarnings();
		}
	}

	// Acts like a button: tick it in the inspector and it fits the bounds, then unticks itself
	[Export]
	public bool FitBoundsToGeometry
	{
		get => false;
		set
		{
			if (value && IsInsideTree()) FitBounds();
		}
	}

	private void FitBounds()
	{
		var first = true;
		Aabb result = default;
		var toLocal = GlobalTransform.AffineInverse();

		foreach (var g in FindChildren("*", "GeometryInstance3D", true, false).OfType<GeometryInstance3D>())
		{
			if (IsDebugNode(g)) continue;
			var local = toLocal * (g.GlobalTransform * g.GetAabb());
			result = first ? local : result.Merge(local);
			first = false;
		}

		if (!first) LocalBounds = result;
	}

	private static bool IsDebugNode(Node n) => n.Name == DebugName || n.Name == GridName;

	// Finds or creates an editor-only unshaded line mesh child
	private ImmediateMesh GetDebugMesh(string nodeName, bool useVertexColors)
	{
		var mi = GetNodeOrNull<MeshInstance3D>(nodeName);
		if (mi == null)
		{
			mi = new MeshInstance3D { Name = nodeName, Mesh = new ImmediateMesh() };
			mi.MaterialOverride = new StandardMaterial3D
			{
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				AlbedoColor = new Color(0f, 1f, 0.4f),
				VertexColorUseAsAlbedo = useVertexColors,
				NoDepthTest = useVertexColors,
				Transparency = useVertexColors
					? BaseMaterial3D.TransparencyEnum.Alpha
					: BaseMaterial3D.TransparencyEnum.Disabled
			};
			mi.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
			AddChild(mi); // no Owner set, so it never gets saved into the scene
		}

		return (ImmediateMesh)mi.Mesh;
	}

	private void UpdateDebugBox()
	{
		if (!Engine.IsEditorHint() || !IsInsideTree()) return;

		var mesh = GetDebugMesh(DebugName, false);
		mesh.ClearSurfaces();
		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		int[,] edges = { {0,1},{2,3},{4,5},{6,7},{0,2},{1,3},{4,6},{5,7},{0,4},{1,5},{2,6},{3,7} };
		for (var i = 0; i < edges.GetLength(0); i++)
		{
			mesh.SurfaceAddVertex(LocalBounds.GetEndpoint(edges[i, 0]));
			mesh.SurfaceAddVertex(LocalBounds.GetEndpoint(edges[i, 1]));
		}
		mesh.SurfaceEnd();
	}

	// Draws the same lattice OnGrid() accepts: multiples of GridUnit/2 in local X and Z,
	// laid on the floor of LocalBounds. Full-unit lines are brighter than half-unit lines.
	private void UpdateDebugGrid()
	{
		if (!Engine.IsEditorHint() || !IsInsideTree()) return;

		var mesh = GetDebugMesh(GridName, true);
		mesh.ClearSurfaces();

		var half = GridUnit * 0.5f;
		if (!ShowGrid || half <= 0.001f || LocalBounds.Size.X <= 0f || LocalBounds.Size.Z <= 0f) return;

		var min = LocalBounds.Position;
		var max = LocalBounds.End;
		var y = min.Y + 0.01f; // nudge up to avoid z-fighting with the floor

		var minor = new Color(1f, 1f, 1f, 0.15f);
		var major = new Color(1f, 0.8f, 0.2f, 0.6f);

		var x0 = Mathf.CeilToInt(min.X / half - 0.001f);
		var x1 = Mathf.FloorToInt(max.X / half + 0.001f);
		var z0 = Mathf.CeilToInt(min.Z / half - 0.001f);
		var z1 = Mathf.FloorToInt(max.Z / half + 0.001f);

		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

		// Lines running along Z (one per X step)
		for (var i = x0; i <= x1; i++)
		{
			var x = i * half;
			var c = i % 2 == 0 ? major : minor;
			mesh.SurfaceSetColor(c);
			mesh.SurfaceAddVertex(new Vector3(x, y, min.Z));
			mesh.SurfaceSetColor(c);
			mesh.SurfaceAddVertex(new Vector3(x, y, max.Z));
		}

		// Lines running along X (one per Z step)
		for (var k = z0; k <= z1; k++)
		{
			var z = k * half;
			var c = k % 2 == 0 ? major : minor;
			mesh.SurfaceSetColor(c);
			mesh.SurfaceAddVertex(new Vector3(min.X, y, z));
			mesh.SurfaceSetColor(c);
			mesh.SurfaceAddVertex(new Vector3(max.X, y, z));
		}

		mesh.SurfaceEnd();
	}

	public IEnumerable<Connector> GetConnectors() =>
		FindChildren("*", "Marker3D", true, false).OfType<Connector>();

	private Aabb _localBounds;
	private float _gridUnit = 4f;
	private bool _showGrid = true;

	private const string DebugName = "__BoundsDebug";
	private const string GridName = "__GridDebug";

	public override void _Ready()
	{
		UpdateDebugBox();
		UpdateDebugGrid();
	}

	public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();
		var connectors = GetConnectors().ToList();

		if (LocalBounds.Size == Vector3.Zero)
			warnings.Add("LocalBounds is empty.");
		if (connectors.Count == 0)
			warnings.Add("Chunk has no connectors.");

		var center = LocalBounds.GetCenter();

		foreach (var c in connectors)
		{
			// Position and direction relative to the chunk root
			var pos = ToLocal(c.GlobalPosition);
			var outward = (GlobalTransform.Basis.Inverse() * -c.GlobalTransform.Basis.Z).Normalized();

			if (c.Size.IsEmpty)
				warnings.Add($"{c.Name}: Size not set.");

			// Facing must be axis-aligned
			if (Mathf.Max(Mathf.Abs(outward.X), Mathf.Max(Mathf.Abs(outward.Y), Mathf.Abs(outward.Z))) < 0.99f)
				warnings.Add($"{c.Name}: -Z is not axis-aligned.");

			// Must point away from the room's interior
			if (outward.Dot(pos - center) <= 0f)
				warnings.Add($"{c.Name}: faces into the room (flip it so -Z points out).");

			// Should sit on the bounds' outer face
			var distToFace = Mathf.Abs((pos - center).Dot(outward) -
									   Mathf.Abs((LocalBounds.Size * 0.5f).Dot(outward.Abs())));
			if (distToFace > 0.05f)
				warnings.Add($"{c.Name}: not flush with the outer wall plane.");

			// Grid check (horizontal axes only; adjust to taste)
			if (!OnGrid(pos.X) || !OnGrid(pos.Z))
				warnings.Add($"{c.Name}: off the {GridUnit} m grid.");
		}

		// Geometry should stay inside the bounds
		warnings
			.AddRange(
				from mesh in FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>()
				where !IsDebugNode(mesh)
				let worldAabb = mesh.GlobalTransform * mesh.GetAabb()
				let localAabb = GlobalTransform.AffineInverse() * worldAabb
				where !LocalBounds.Grow(0.05f)
					.Encloses(localAabb)
				select $"{mesh.Name} sticks out past LocalBounds."
			);

		return [.. warnings];
	}

	private bool OnGrid(float v)
	{
		float half = GridUnit * 0.5f; // allows door centers on half-grid points
		return Mathf.Abs(v - Mathf.Round(v / half) * half) < 0.01f;
	}
}
