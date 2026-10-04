using System.Collections.Generic;
using Godot;

namespace ResourceShowing.Scripts.Ui;

// Shows any view model Resource. Knows no view model type: in _Ready it reads the Resource's property list
// and makes one row (name | control) per exported property. The control is picked from the property's
// type and hint, so a Character, a Weapon and a Planet all get a fitting UI from this one class.
// Read-only: no control changes anything.
public partial class ViewModelView : PanelContainer
{
	// Set before the view enters the tree (or in the inspector); _Ready builds the UI from it.
	[Export] public Resource ViewModel { get; set; }

	public override void _Ready()
	{
		var rows = new VBoxContainer();
		AddChild(rows);
		if (ViewModel == null)
		{
			rows.AddChild(new Label { Text = "Nothing selected" });
			return;
		}

		rows.AddChild(new Label { Text = ViewModel.ResourceName, ThemeTypeVariation = "HeaderLarge" });
		var grid = new GridContainer { Columns = 2 };
		rows.AddChild(grid);

		foreach (Godot.Collections.Dictionary property in ViewModel.GetPropertyList())
		{
			// The list also holds Resource's own properties (resource_name, …) and script/category entries;
			// [Export] properties of the script are the ones with both flags.
			var usage = (PropertyUsageFlags)(int)property["usage"];
			if ((usage & PropertyUsageFlags.ScriptVariable) == 0 || (usage & PropertyUsageFlags.Editor) == 0) continue;

			string name = (string)property["name"];
			grid.AddChild(new Label { Text = name, SizeFlagsVertical = SizeFlags.ShrinkBegin });
			Control control = CreateControl(ViewModel.Get(name), property);
			control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			grid.AddChild(control);
		}
	}

	private static Control CreateControl(Variant value, Godot.Collections.Dictionary property)
	{
		var hint = (PropertyHint)(int)property["hint"];
		string hintString = (string)property["hint_string"];

		switch (value.VariantType)
		{
			case Variant.Type.Bool:
				return new CheckBox { ButtonPressed = value.AsBool(), Disabled = true };

			case Variant.Type.Int when hint == PropertyHint.Enum:
				return EnumControl(value.AsInt64(), hintString);

			case Variant.Type.Int or Variant.Type.Float when hint == PropertyHint.Range:
				string[] range = hintString.Split(',');
				return new ProgressBar
				{
					MinValue = double.Parse(range[0]),
					MaxValue = double.Parse(range[1]),
					Value = value.AsDouble(),
					CustomMinimumSize = new Vector2(0, 24),
				};

			// SpinBox clamps even values set from code to its range (default 0..100), so open it wide first.
			case Variant.Type.Int:
				return new SpinBox { MinValue = long.MinValue, MaxValue = long.MaxValue, Value = value.AsInt64(), Editable = false };

			case Variant.Type.Float:
				return new SpinBox
				{
					MinValue = double.MinValue, MaxValue = double.MaxValue, Step = 0.01, Value = value.AsDouble(), Editable = false,
				};

			case Variant.Type.String when hint == PropertyHint.MultilineText:
				return new TextEdit
				{
					Text = value.AsString(), Editable = false, WrapMode = TextEdit.LineWrappingMode.Boundary,
					CustomMinimumSize = new Vector2(0, 80),
				};

			case Variant.Type.String:
				return new LineEdit { Text = value.AsString(), Editable = false };

			case Variant.Type.Color:
				return new ColorPickerButton { Color = value.AsColor(), Disabled = true, CustomMinimumSize = new Vector2(0, 24) };

			case Variant.Type.Array:
				return ArrayControl(value.AsGodotArray());

			case Variant.Type.Object when value.Obj is Resource nested:
				return new ViewModelView { ViewModel = nested };

			default:
				// Vector2I and anything else without a dedicated control: shown as text.
				return new Label { Text = value.ToString() };
		}
	}

	// The hint string has the enum's names: "Warrior:0,Mage:1,…" (or "Warrior,Mage,…").
	private static Control EnumControl(long selected, string hintString)
	{
		var options = new OptionButton { Disabled = true };
		var indexByValue = new Dictionary<long, int>();
		string[] names = hintString.Split(',');
		for (int i = 0; i < names.Length; i++)
		{
			string[] parts = names[i].Split(':');
			long value = parts.Length > 1 && long.TryParse(parts[1], out long v) ? v : i;
			options.AddItem(parts[0], (int)value);
			indexByValue[value] = i;
		}
		options.Selected = indexByValue.GetValueOrDefault(selected, -1);
		return options;
	}

	// Resources in the array get a nested view each (built by their own _Ready); other items a label.
	private static Control ArrayControl(Godot.Collections.Array items)
	{
		var list = new VBoxContainer();
		if (items.Count == 0) list.AddChild(new Label { Text = "(empty)" });
		foreach (Variant item in items)
			list.AddChild(item.Obj is Resource resource ? new ViewModelView { ViewModel = resource } : new Label { Text = $"• {item}" });
		return list;
	}
}
