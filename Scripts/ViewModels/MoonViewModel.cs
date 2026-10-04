using Godot;
using ResourceShowing.Scripts.Models;

namespace ResourceShowing.Scripts.ViewModels;

// One moon inside a PlanetViewModel. Its own file: Godot finds a C# Resource script by file name.
[GlobalClass]
public partial class MoonViewModel : Resource
{
	[Export] public string Name { get; set; }
	[Export] public float OrbitDays { get; set; }

	public static MoonViewModel From(Moon moon) => new()
	{
		ResourceName = $"Moon: {moon.Name}",
		Name = moon.Name,
		OrbitDays = moon.OrbitDays,
	};
}
