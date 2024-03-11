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
	public Vector2 PlaneSize
	{
		get => _planeSize;
		set
		{
			if (!_planeSize.IsEqualApprox(value))
				ChangeSize(value, Height);

			_planeSize = value;
		}
	}
	[Export]
	public float Height
	{
		get => _height;
		set
		{
			if (_height != value)
				ChangeSize(PlaneSize, value);

			_height = value;
		}
	}

	public Vector3 BoxSize
	{
		get => _boxSize;
		set
		{
			/* if (!_boxSize.IsEqualApprox(value))
				ChangeSize(value, Height);
 */
			_boxSize = value;
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

	private Vector2 _planeSize = new(2, 2);
	private Vector3 _boxSize = new(1, 3, .1f);
	private float _height = 3;

	public override void _EnterTree()
	{
		AddToGroup(GroupName);
		Name = $"MapBlock{GetTree().GetNodesInGroup(GroupName).Count}";

		if (CurrentMapBlockMode.Equals(MapBlockMode.PLANE))
		{
			GeneratePlaneMeshes();
		}

		if (CurrentMapBlockMode.Equals(MapBlockMode.BOX))
		{
			GenerateBoxMeshes();
		}

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

		if (newSize.IsZeroApprox() || newHeight == 0 || (newSize.IsEqualApprox(PlaneSize) && Height == newHeight))
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

	private void GenerateBoxMeshes()
	{
		this.QueueFreeChildren();

		Floor = new() { Name = "Floor", Position = Vector3.Zero, Mesh = new PlaneMesh() { Size = new Vector2(BoxSize.X, BoxSize.Y) } };
		Ceil = new() { Name = "Ceil", Position = new Vector3(0, Height, 0), Mesh = new PlaneMesh() { Size = new Vector2(BoxSize.X, BoxSize.Y) } };
		NorthWall = new() { Name = "NorthWall", Position = new Vector3(0, Height / 2f, -(BoxSize.X / 2f - BoxSize.Z)), Mesh = new BoxMesh() { Size = BoxSize } };
		SouthWall = new() { Name = "SouthWall", Position = new Vector3(0, Height / 2f, (BoxSize.X / 2f) - BoxSize.Z), Mesh = new BoxMesh() { Size = BoxSize } };


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
	private void GeneratePlaneMeshes()
	{
		this.QueueFreeChildren();

		Floor = new() { Name = "Floor", Position = Vector3.Zero, Mesh = new PlaneMesh() { Size = PlaneSize, Orientation = PlaneMesh.OrientationEnum.Y } };
		Ceil = new() { Name = "Ceil", Position = new Vector3(0, Height, 0), Mesh = new PlaneMesh() { Size = PlaneSize, FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.Y } };
		NorthWall = new() { Name = "NorthWall", Position = new Vector3(0, Height / 2f, -PlaneSize.X / 2f), Mesh = new PlaneMesh() { Size = new Vector2(PlaneSize.X, Height), Orientation = PlaneMesh.OrientationEnum.Z } };
		SouthWall = new() { Name = "SouthWall", Position = new Vector3(0, Height / 2f, PlaneSize.X / 2f), Mesh = new PlaneMesh() { Size = new Vector2(PlaneSize.X, Height), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.Z }, };
		EastWall = new() { Name = "EastWall", Position = new Vector3(PlaneSize.X / 2f, Height / 2f, 0), Mesh = new PlaneMesh() { Size = new Vector2(PlaneSize.X, Height), FlipFaces = true, Orientation = PlaneMesh.OrientationEnum.X } };
		WestWall = new() { Name = "WestWall", Position = new Vector3(-PlaneSize.X / 2f, Height / 2f, 0), Mesh = new PlaneMesh() { Size = new Vector2(PlaneSize.X, Height), Orientation = PlaneMesh.OrientationEnum.X }, };

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
