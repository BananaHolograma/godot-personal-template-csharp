using System.Linq;
using Godot;
using Godot.Collections;
using GodotExtensions;

namespace GameRoot;

[Tool]
public partial class MapBlock : Node3D
{
	[Export] public string GroupName = "mapblock";
	[Export]
	public Vector2 Size
	{
		get => _size;
		set
		{
			if (!_size.IsEqualApprox(value))
				ChangeSize(value, Height);

			_size = value;
		}
	}
	[Export]
	public float Height
	{
		get => _height;
		set
		{
			if (_height != value)
				ChangeSize(Size, value);

			_height = value;
		}
	}

	private Vector2 _size = new(2, 2);
	private float _height = 3;

	public MeshInstance3D Floor;
	public MeshInstance3D Ceil;
	public MeshInstance3D NorthWall;
	public MeshInstance3D SouthWall;
	public MeshInstance3D EastWall;
	public MeshInstance3D WestWall;


	public override void _EnterTree()
	{
		this.QueueFreeChildren();

		AddToGroup(GroupName);
		Name = $"MapBlock{GetTree().GetNodesInGroup(GroupName).Count}";

		Floor = new() { Name = "Floor", Position = Vector3.Zero, Mesh = new PlaneMesh() { Size = Size, Orientation = PlaneMesh.OrientationEnum.Y } };
		Ceil = new() { Name = "Ceil", Position = new Vector3(0, Height, 0), Mesh = new PlaneMesh() { Size = Size, FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.Y } };
		NorthWall = new() { Name = "NorthWall", Position = new Vector3(0, Height / 2f, -Size.X / 2f), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Height), Orientation = PlaneMesh.OrientationEnum.Z } };
		SouthWall = new() { Name = "SouthWall", Position = new Vector3(0, Height / 2f, Size.X / 2f), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Height), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.Z }, };
		EastWall = new() { Name = "EastWall", Position = new Vector3(Size.X / 2f, Height / 2f, 0), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Height), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.X } };
		WestWall = new() { Name = "WestWall", Position = new Vector3(-Size.X / 2f, Height / 2f, 0), Mesh = new PlaneMesh() { Size = new Vector2(Size.X, Height), Orientation = PlaneMesh.OrientationEnum.X }, };

		AddChild(Floor);
		AddChild(Ceil);
		AddChild(NorthWall);
		AddChild(SouthWall);
		AddChild(EastWall);
		AddChild(WestWall);

		Floor.SetOwnerToEditedSceneRoot();
		Ceil.SetOwnerToEditedSceneRoot();
		NorthWall.SetOwnerToEditedSceneRoot();
		SouthWall.SetOwnerToEditedSceneRoot();
		EastWall.SetOwnerToEditedSceneRoot();
		WestWall.SetOwnerToEditedSceneRoot();

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

	public void ChangeSize(Vector2 newSize, float newHeight = 0, bool onlyOnVisibleMeshes = true)
	{
		newHeight = newHeight == 0 ? Height : newHeight;

		if (newSize.IsZeroApprox() || newHeight == 0 || (newSize.IsEqualApprox(Size) && Height == newHeight))
			return;

		foreach (MeshInstance3D mesh in onlyOnVisibleMeshes ? VisibleMeshes() : AvailableMeshes())
		{
			PlaneMesh meshPlane = mesh.Mesh as PlaneMesh;

			if (newHeight > 0 && mesh.Name == "Ceil")
				mesh.Position = mesh.Position with { Y = newHeight };

			if (new[] { "Ceil", "Floor" }.Any(name => name == mesh.Name))
				meshPlane.Size = newSize;

			if (new[] { "NorthWall", "SouthWall" }.Any(name => name == mesh.Name))
			{
				meshPlane.Size = new Vector2(newSize.X, newHeight);
				mesh.Position = mesh.Position with { Y = newHeight / 2f, Z = Mathf.Sign(mesh.Position.Z) * (newSize.Y / 2f) };
			}

			if (new[] { "EastWall", "WestWall" }.Any(name => name == mesh.Name))
			{
				meshPlane.Size = new Vector2((newSize.Y > newSize.X || newSize.X > newSize.Y) ? newSize.Y : newSize.X, newHeight);
				mesh.Position = mesh.Position with { X = Mathf.Sign(mesh.Position.X) * (newSize.X / 2f), Y = newHeight / 2f };
			}
		}
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
