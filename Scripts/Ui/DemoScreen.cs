using System;
using System.Collections.Generic;
using Godot;
using ResourceShowing.Scripts.Models;
using ResourceShowing.Scripts.ViewModels;

namespace ResourceShowing.Scripts.Ui;

// List of sample objects on the left; selecting one builds its view model and puts a new ViewModelView
// for it into the container on the right. The view builds its own UI in _Ready.
public partial class DemoScreen : Control
{
	[Export] public ItemList Samples { get; set; }
	[Export] public Container Content { get; set; }

	// Name in the list -> view model built from a sample model.
	private readonly List<(string Name, Func<Resource> Build)> _samples =
	[
		("Character", () => CharacterViewModel.From(new Character
		{
			Name = "Aria", Level = 7, Health = 34, MaxHealth = 50, IsAlive = true, Class = CharacterClass.Mage,
			Inventory = ["Staff", "Health potion", "Old map"],
		})),
		("Weapon", () => WeaponViewModel.From(new Weapon
		{
			Title = "Frostbite", Damage = 42, AttackSpeed = 1.25f, Element = Element.Ice,
			Glow = new Color(0.4f, 0.8f, 1f), TwoHanded = true,
			Description = "A greatsword forged in a glacier.\nSlows whatever it hits.",
		})),
		("Planet", () => PlanetViewModel.From(new Planet
		{
			Name = "Kepler-22b", SystemPosition = new Vector2I(12, -4), RadiusKm = 15_290.5,
			Atmosphere = new Color(0.9f, 0.6f, 0.3f), Habitable = false,
			Moons = [new Moon { Name = "Io-K", OrbitDays = 3.2f }, new Moon { Name = "Selene-K", OrbitDays = 27.4f }],
		})),
	];

	public override void _Ready()
	{
		foreach ((string name, _) in _samples) Samples.AddItem(name);
		Samples.ItemSelected += OnSampleSelected;
		Samples.Select(0);
		OnSampleSelected(0);
	}

	private void OnSampleSelected(long index)
	{
		foreach (Node child in Content.GetChildren()) child.QueueFree();
		Content.AddChild(new ViewModelView { ViewModel = _samples[(int)index].Build() });
	}
}
