using System.Collections.Generic;
using Godot;

namespace ResourceShowing.Scripts.Models;

// Plain game data, no Godot Resource. Three classes with deliberately different property types,
// so the generated UI shows different controls for each.

public enum CharacterClass { Warrior, Mage, Rogue }

public sealed class Character
{
	public string Name;
	public int Level;
	public float Health;
	public float MaxHealth;
	public bool IsAlive;
	public CharacterClass Class;
	public List<string> Inventory = [];
}

public enum Element { Physical, Fire, Ice, Poison }

public sealed class Weapon
{
	public string Title;
	public int Damage;
	public float AttackSpeed;
	public Element Element;
	public Color Glow;
	public bool TwoHanded;
	public string Description;
}

public sealed class Moon
{
	public string Name;
	public float OrbitDays;
}

public sealed class Planet
{
	public string Name;
	public Vector2I SystemPosition;
	public double RadiusKm;
	public Color Atmosphere;
	public bool Habitable;
	public List<Moon> Moons = [];
}
