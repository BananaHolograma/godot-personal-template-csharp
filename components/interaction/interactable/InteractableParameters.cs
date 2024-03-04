using Godot;

namespace GameRoot;
public partial class InteractableParameters : Resource
{
	public enum CATEGORY
	{
		DOOR,
		WINDOW,
		OBJECT,
		ITEM,
		WEAPON,
		KEY
	}

	[ExportGroup("Information")]
	[Export] public string Title;
	[Export] public string Description;
	[Export] public CATEGORY Category;

	[ExportGroup("Scan")]
	[Export] public bool Scannable = false;

	[ExportGroup("Pickup")]
	[Export] public bool Pickable = false;
	[Export] public string PickupMessage;
	[Export] public float PullPower = 20f;
	[Export] public float ThrowPower = 10f;

	[ExportGroup("Usable")]
	[Export] public bool Usable = false;
	[Export] public string UsableMessage;

	[ExportGroup("Inventory")]
	[Export] public bool CanBeSaved = false;
	[Export] public string InventorySaveMessage;

	[ExportGroup("Player")]
	[Export] public bool LockPlayer = false;

	// Make sure you provide a parameterless constructor.
	// In C#, a parameterless constructor is different from a
	// constructor with all default values.
	// Without a parameterless constructor, Godot will have problems
	// creating and editing your resource via the inspector.
	public InteractableParameters() : this("", "", CATEGORY.OBJECT) { }
	public InteractableParameters(string title, string description, CATEGORY category)
	{
		Title = title;
		Description = description;
		Category = category;
	}
}
