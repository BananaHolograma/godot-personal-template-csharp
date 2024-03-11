using System.Linq;
using Godot;
using Godot.Collections;

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
	public int Height
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
	private int _height = 3;

	public override void _EnterTree()
	{
		AddToGroup(GroupName);
		Name = $"MapBlock{GetTree().GetNodesInGroup(GroupName).Count}";
	}

	public void UpdateFaces(Dictionary<Vector2, bool> neighbours)
	{
		if (neighbours.Count == 0)
			return;

		Dictionary<Vector2, MeshInstance3D> vectorToWall = new() {
			{Vector2.Up, GetNode<MeshInstance3D>("NorthWall")},
			{Vector2.Down, GetNode<MeshInstance3D>("SouthWall")},
			{Vector2.Left, GetNode<MeshInstance3D>("WestWall")},
			{Vector2.Right, GetNode<MeshInstance3D>("EastWall")},
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

	public void ChangeSize(Vector2 newSize, int newHeight = 0)
	{
		newHeight = newHeight == 0 ? Height : newHeight;

		if (newSize.IsZeroApprox() || newHeight == 0)
			return;

		foreach (MeshInstance3D mesh in AvailableMeshes())
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
			GetNode<MeshInstance3D>("Floor"),
			GetNode<MeshInstance3D>("Ceil"),
			GetNode<MeshInstance3D>("NorthWall"),
			GetNode<MeshInstance3D>("SouthWall"),
			GetNode<MeshInstance3D>("EastWall"),
			GetNode<MeshInstance3D>("WestWall")
		};
	}

	public Array<MeshInstance3D> VisibleMeshes()
	{
		Array<MeshInstance3D> visibleMeshes = new();

		foreach (Node child in GetChildren())
		{
			if (child is MeshInstance3D mesh && mesh.Visible)
				visibleMeshes.Add((MeshInstance3D)child);
		}

		return visibleMeshes;
	}


}
