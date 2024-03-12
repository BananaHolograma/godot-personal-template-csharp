using System;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExtensions;

namespace GameRoot;

[Tool]
public partial class MapBlock : Node3D
{

	[Export] public MapBlockMode CurrentMapBlockMode = MapBlockMode.PLANE;
	[Export] public string GroupName = "mapblock";
	[Export]
	public Vector3 Size
	{
		get => _size;
		set
		{
			if (!_size.IsEqualApprox(value))
				ChangeSize(value);

			_size = value;
		}
	}


	public enum MapBlockMode
	{
		PLANE,
		BOX
	}

	public MeshInstance3D Floor;
	public MeshInstance3D Ceil;
	public MeshInstance3D NorthWall;
	public MeshInstance3D SouthWall;
	public MeshInstance3D EastWall;
	public MeshInstance3D WestWall;

	private Vector3 _size = new(1, 3, .1f);


	public override void _EnterTree()
	{
		AddToGroup(GetGroupName());
		Name = $"MapBlock{GetTree().GetNodesInGroup(GetGroupName()).Count}";

		if (CurrentMapBlockMode.Equals(MapBlockMode.PLANE))
			GeneratePlaneMeshes();


		if (CurrentMapBlockMode.Equals(MapBlockMode.BOX))
			GenerateBoxMeshes();

		foreach (MeshInstance3D mesh in AvailableMeshes())
		{
			AddChild(mesh);
			mesh.SetOwnerToEditedSceneRoot();
		}

	}

	private StringName GetGroupName()
	{
		return $"{GroupName}_{(CurrentMapBlockMode.Equals(MapBlockMode.PLANE) ? "plane" : "box")}";
	}

	public void UpdateFaces(Dictionary<Vector2, bool> neighbours)
	{
		if (neighbours.Count == 0)
			return;

		Dictionary<Vector2, MeshInstance3D> vectorToWall = new() {
			{Vector2.Up, NorthWall},
			{Vector2.Down, SouthWall},
			{Vector2.Left, WestWall},
			{Vector2.Right, EastWall},
		};

		foreach (Vector2 neighbourDirection in neighbours.Keys)
		{
			if (neighbours[neighbourDirection])
			{
				MeshInstance3D mesh = vectorToWall[neighbourDirection];
				mesh?.Hide();
			}
		}
	}

	public void ChangeSize(Vector3 newSize, bool onlyOnVisibleMeshes = true)
	{
		if (newSize.IsZeroApprox() || newSize.Y == 0 || newSize.IsEqualApprox(Size))
			return;

		foreach (MeshInstance3D mesh in onlyOnVisibleMeshes ? VisibleMeshes() : AvailableMeshes())
		{
			if (CurrentMapBlockMode.Equals(MapBlockMode.PLANE))
				ChangeSizeOnPlaneMeshes(mesh, newSize);

			if (CurrentMapBlockMode.Equals(MapBlockMode.BOX))
				ChangeSizeOnBoxMeshes(mesh, newSize);
		}
	}

	private void ChangeSizeOnBoxMeshes(MeshInstance3D mesh, Vector3 newSize)
	{
		if (newSize.Y > 0 && mesh.Name == "Ceil")
			mesh.Position = mesh.Position with { Y = newSize.Y };

		if (new[] { "Ceil", "Floor" }.Any(name => name == mesh.Name))
		{
			PlaneMesh meshPlane = mesh.Mesh as PlaneMesh;
			meshPlane.Size = new Vector2(Size.X, Size.X);
			return;
		}

		BoxMesh meshBox = mesh.Mesh as BoxMesh;

	}

	private void ChangeSizeOnPlaneMeshes(MeshInstance3D mesh, Vector3 newSize)
	{
		PlaneMesh meshPlane = mesh.Mesh as PlaneMesh;

		if (newSize.Y > 0 && mesh.Name == "Ceil")
			mesh.Position = mesh.Position with { Y = newSize.Y };

		if (new[] { "Ceil", "Floor" }.Any(name => name == mesh.Name))
			meshPlane.Size = new Vector2(Size.X, Size.X);

		if (new[] { "NorthWall", "SouthWall" }.Any(name => name == mesh.Name))
		{
			meshPlane.Size = new Vector2(newSize.X, newSize.Y);
			mesh.Position = mesh.Position with { Y = newSize.Y / 2f, Z = Mathf.Sign(mesh.Position.Z) * (newSize.Y / 2f) };
		}

		if (new[] { "EastWall", "WestWall" }.Any(name => name == mesh.Name))
		{
			meshPlane.Size = new Vector2((newSize.Y > newSize.X || newSize.X > newSize.Y) ? newSize.Y : newSize.X, newSize.Y);
			mesh.Position = mesh.Position with { X = Mathf.Sign(mesh.Position.X) * (newSize.X / 2f), Y = newSize.Y / 2f };
		}
	}

	private void GenerateBoxMeshes()
	{
		this.QueueFreeChildren();

		Floor = new() { Name = "Floor", Position = Vector3.Zero, Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.X), Orientation = PlaneMesh.OrientationEnum.Y } };
		Ceil = new() { Name = "Ceil", Position = new Vector3(0, Size.Y, 0), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.X), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.Y } };
		NorthWall = new() { Name = "NorthWall", Position = new Vector3(0, Size.Y / 2f, -(Size.X / 2f + Size.Z / 2f)), Mesh = new BoxMesh() { Size = Size } };
		SouthWall = new() { Name = "SouthWall", Position = new Vector3(0, Size.Y / 2f, (Size.X / 2f) + Size.Z / 2f), Mesh = new BoxMesh() { Size = Size } };
		EastWall = new() { Name = "EastWall", Position = new Vector3(Size.X / 2f + Size.Z / 2f, Size.Y / 2f, 0), Rotation = new Vector3(0, Mathf.DegToRad(90), 0), Mesh = new BoxMesh() { Size = Size } };
		WestWall = new() { Name = "WestWall", Position = new Vector3(-(Size.X / 2f + Size.Z / 2f), Size.Y / 2f, 0), Rotation = new Vector3(0, Mathf.DegToRad(90), 0), Mesh = new BoxMesh() { Size = Size } };
	}
	private void GeneratePlaneMeshes()
	{
		this.QueueFreeChildren();

		Floor = new() { Name = "Floor", Position = Vector3.Zero, Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.X), Orientation = PlaneMesh.OrientationEnum.Y } };
		Ceil = new() { Name = "Ceil", Position = new Vector3(0, Size.Y, 0), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.X), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.Y } };
		NorthWall = new() { Name = "NorthWall", Position = new Vector3(0, Size.Y / 2f, -Size.X / 2f), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.Y), Orientation = PlaneMesh.OrientationEnum.Z } };
		SouthWall = new() { Name = "SouthWall", Position = new Vector3(0, Size.Y / 2f, Size.X / 2f), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.Y), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.Z }, };
		EastWall = new() { Name = "EastWall", Position = new Vector3(Size.X / 2f, Size.Y / 2f, 0), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.Y), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.X } };
		WestWall = new() { Name = "WestWall", Position = new Vector3(-Size.X / 2f, Size.Y / 2f, 0), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Size.Y), Orientation = PlaneMesh.OrientationEnum.X }, };
	}

	private Array<MeshInstance3D> AvailableMeshes()
	{
		return new Array<MeshInstance3D>() {
			Floor,
			Ceil,
			NorthWall,
			SouthWall,
			EastWall,
			WestWall
		};
	}

	public Array<MeshInstance3D> VisibleMeshes()
	{
		Array<MeshInstance3D> visibleMeshes = new();

		foreach (MeshInstance3D child in AvailableMeshes().Where(child => child != null && child.Visible))
			visibleMeshes.Add(child);

		return visibleMeshes;
	}
}
