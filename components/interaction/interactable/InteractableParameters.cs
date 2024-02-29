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
	public string title;
	public string description;
	public CATEGORY Category;

	[ExportGroup("Scan")]
	public bool Scannable = false;

	[ExportGroup("Pickup")]
	public bool Pickable = false;
	public string PickupMessage;
	public float PullPower = 20f;
	public float ThrowPower = 10f;

	[ExportGroup("Usable")]
	public bool Usable = false;
	public string UsableMessage;
	[ExportGroup("Inventory")]
	public bool CanBeSaved = false;
	public string InventorySaveMessage;

	[ExportGroup("Player")]
	public bool LockPlayer = false;
}
