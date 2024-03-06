using Godot;

namespace GameRoot;
public partial class InteractionPointer : Control
{
	[Export] public Vector2 MinimumSize = new(64, 64);
	[Export] public Texture2D DefaultPointerTexture;
	public TextureRect CurrentPointer;
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Pass;
		CurrentPointer = GetNode<TextureRect>("%Pointer");

		CurrentPointer.Texture = DefaultPointerTexture;
		CurrentPointer.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		CurrentPointer.CustomMinimumSize = MinimumSize;
	}


}
