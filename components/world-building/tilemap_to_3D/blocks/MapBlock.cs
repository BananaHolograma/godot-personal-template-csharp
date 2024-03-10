using Godot;
using Godot.Collections;
using System.Linq;

namespace GameRoot;

[Tool]
public partial class MapBlock : Node3D
{
	[Export] public string GroupName = "mapblock_2x2";
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

	public Array<MeshInstance3D> VisibleMeshes() =>
		(Array<MeshInstance3D>)GetChildren().Where(child => child is MeshInstance3D mesh && mesh.Visible);

}
