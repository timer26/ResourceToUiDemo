using Godot;
using ResourceShowing.Scripts.Models;

namespace ResourceShowing.Scripts.ViewModels;

// Snapshot of a Character for display. Only [Export] properties are visible to the UI.
[GlobalClass]
public partial class CharacterViewModel : Resource
{
	[Export] public string Name { get; set; }
	[Export] public int Level { get; set; }
	// Range hint "min,max": the UI shows it as a bar instead of a number.
	[Export(PropertyHint.Range, "0,100")] public float HealthPercent { get; set; }
	[Export] public bool IsAlive { get; set; }
	// Enums arrive as ints; the hint string carries their names.
	[Export] public CharacterClass Class { get; set; }
	[Export] public Godot.Collections.Array<string> Inventory { get; set; } = [];

	public static CharacterViewModel From(Character character) => new()
	{
		ResourceName = $"Character: {character.Name}",
		Name = character.Name,
		Level = character.Level,
		HealthPercent = character.MaxHealth > 0 ? character.Health / character.MaxHealth * 100f : 0f,
		IsAlive = character.IsAlive,
		Class = character.Class,
		Inventory = new Godot.Collections.Array<string>(character.Inventory),
	};
}
