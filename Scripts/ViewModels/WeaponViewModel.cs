using Godot;
using ResourceShowing.Scripts.Models;

namespace ResourceShowing.Scripts.ViewModels;

// Snapshot of a Weapon for display.
[GlobalClass]
public partial class WeaponViewModel : Resource
{
	[Export] public string Title { get; set; }
	[Export] public int Damage { get; set; }
	[Export] public float AttackSpeed { get; set; }
	[Export] public float DamagePerSecond { get; set; }
	[Export] public Element Element { get; set; }
	[Export] public Color Glow { get; set; }
	[Export] public bool TwoHanded { get; set; }
	// Multiline hint: the UI shows a text box instead of a single line.
	[Export(PropertyHint.MultilineText)] public string Description { get; set; }

	public static WeaponViewModel From(Weapon weapon) => new()
	{
		ResourceName = $"Weapon: {weapon.Title}",
		Title = weapon.Title,
		Damage = weapon.Damage,
		AttackSpeed = weapon.AttackSpeed,
		DamagePerSecond = weapon.Damage * weapon.AttackSpeed,
		Element = weapon.Element,
		Glow = weapon.Glow,
		TwoHanded = weapon.TwoHanded,
		Description = weapon.Description,
	};
}
