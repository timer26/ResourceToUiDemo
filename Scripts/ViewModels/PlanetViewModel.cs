using System.Linq;
using Godot;
using ResourceShowing.Scripts.Models;

namespace ResourceShowing.Scripts.ViewModels;

// Snapshot of a Planet for display. Moons are nested view models: the UI builds them recursively.
[GlobalClass]
public partial class PlanetViewModel : Resource
{
	[Export] public string Name { get; set; }
	[Export] public Vector2I SystemPosition { get; set; }
	[Export] public double RadiusKm { get; set; }
	[Export] public Color Atmosphere { get; set; }
	[Export] public bool Habitable { get; set; }
	[Export] public Godot.Collections.Array<MoonViewModel> Moons { get; set; } = [];

	public static PlanetViewModel From(Planet planet) => new()
	{
		ResourceName = $"Planet: {planet.Name}",
		Name = planet.Name,
		SystemPosition = planet.SystemPosition,
		RadiusKm = planet.RadiusKm,
		Atmosphere = planet.Atmosphere,
		Habitable = planet.Habitable,
		Moons = new Godot.Collections.Array<MoonViewModel>(planet.Moons.Select(MoonViewModel.From)),
	};
}
