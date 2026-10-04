# ResourceShowing

### Disclaimer: This is AI-extracted logic. I only wrote this line and corrected a few things.
A small Godot 4.7 (.NET / C#) demo of building UI from a **view model Resource**: the UI reads the
resource's exported properties at runtime and makes a control for each one. It has no game logic and nothing
can be edited; it only shows the pattern used by `ViewModelPanel` in ReanimatedLegacy.

Three sample classes (`Character`, `Weapon`, `Planet`) have different properties. Pick one in the list and
the right side builds a UI for it, without any per-class UI code.

## Running

| What | Command |
|---|---|
| Build (compile check) | `dotnet build` |
| Run the demo | `godot-mono` (main scene is `Scenes/Demo.tscn`) |
| Open the editor | `godot-mono -e` |
| First run / after a fresh clone | `godot-mono --headless --import --path .` |
| Check for errors without a window | `godot-mono --path . --quit-after 60` |

Requirements: Godot 4.7 .NET build (`godot-mono`), .NET 8 SDK.

## Layout

```
Scenes/
  Demo.tscn                 list on the left, container on the right
Scripts/
  Models/Models.cs          plain C# data: Character, Weapon, Planet, Moon + enums
  ViewModels/               one Resource per model, [Export] properties + static From(model)
    CharacterViewModel.cs
    WeaponViewModel.cs
    PlanetViewModel.cs
    MoonViewModel.cs
  Ui/
    ViewModelView.cs        builds a UI for ANY view model Resource in _Ready
    DemoScreen.cs           the sample list; creates a ViewModelView when one is selected
```

## How it works

```
Model (plain C#)  --From()-->  ViewModel (Resource, [Export]s)  --GetPropertyList()-->  ViewModelView (controls)
   Character                    CharacterViewModel                                         LineEdit, CheckBox, ...
```

1. **Model**: plain data (`Character`, `Weapon`, `Planet`). It knows nothing about Godot UI.
2. **View model**: a `Resource` with `[Export]` properties. `static From(model)` creates a **snapshot**: it is
   built once and never edited. It can also hold computed values the model doesn't store
   (`HealthPercent`, `DamagePerSecond`).
3. **View**: `ViewModelView` has an exported `ViewModel` property. In `_Ready()` it:
   - calls `ViewModel.GetPropertyList()`,
   - keeps only the entries whose `usage` has both `ScriptVariable` and `Editor` flags (these are the script's
     `[Export]` properties; this drops Resource's own properties like `resource_name` and category entries),
   - reads each value with `ViewModel.Get(name)`,
   - picks a control from the value's `VariantType` plus the property's `hint` / `hint_string`.
4. **Container**: `DemoScreen` frees the old view and adds `new ViewModelView { ViewModel = vm }`. Adding it to
   the tree runs its `_Ready`, which builds the UI.

### What one property entry looks like

`GetPropertyList()` returns one `Godot.Collections.Dictionary` per property:

| Key | Example (`HealthPercent`) | Use |
|---|---|---|
| `name` | `"HealthPercent"` | label text, and the key for `Get(name)` |
| `type` | `Variant.Type.Float` | declared type (the view uses the value's `VariantType`) |
| `hint` | `PropertyHint.Range` | how to show it |
| `hint_string` | `"0,100"` | details for the hint (range, enum names, …) |
| `usage` | `ScriptVariable \| Editor \| Storage …` | filter: is it an exported script property? |

Names are the C# property names (`HealthPercent`), not snake_case.

### Type to control

| Property | Control |
|---|---|
| `bool` | CheckBox |
| enum (int + `PropertyHint.Enum`) | OptionButton; names come from `hint_string` (`"Warrior:0,Mage:1,Rogue:2"`) |
| `int` / `float` / `double` + `PropertyHint.Range` | ProgressBar with the range from `hint_string` |
| `int` | SpinBox |
| `float` / `double` | SpinBox, step 0.01 |
| `string` + `PropertyHint.MultilineText` | TextEdit |
| `string` | LineEdit |
| `Color` | ColorPickerButton |
| `Array<…>` | list: a nested `ViewModelView` for each Resource item, a label for anything else |
| `Resource` | nested `ViewModelView` |
| anything else (`Vector2I`, …) | Label with `value.ToString()` |

All controls are disabled or non-editable.

## Cookbook

### Add a new demo class

1. Model in `Scripts/Models/Models.cs` (or its own file):
   ```csharp
   public sealed class Spell
   {
       public string Name;
       public int ManaCost;
       public float Cooldown;
   }
   ```
2. View model in `Scripts/ViewModels/SpellViewModel.cs` (the **file name must match the class name**):
   ```csharp
   [GlobalClass]
   public partial class SpellViewModel : Resource
   {
       [Export] public string Name { get; set; }
       [Export] public int ManaCost { get; set; }
       [Export] public float Cooldown { get; set; }

       public static SpellViewModel From(Spell spell) => new()
       {
           ResourceName = $"Spell: {spell.Name}",   // shown as the title
           Name = spell.Name,
           ManaCost = spell.ManaCost,
           Cooldown = spell.Cooldown,
       };
   }
   ```
3. One entry in `_samples` in `Scripts/Ui/DemoScreen.cs`:
   ```csharp
   ("Spell", () => SpellViewModel.From(new Spell { Name = "Fireball", ManaCost = 20, Cooldown = 3.5f })),
   ```

`ViewModelView` doesn't change.

### Add a property to an existing view model

Add an `[Export]` property and fill it in `From`. It shows up in the UI automatically, in the order it is
declared.

### Keep a property out of the UI

Don't put `[Export]` on it. A plain C# property isn't in the property list with the `ScriptVariable` + `Editor`
flags, so the view skips it.

### Show a computed value

Compute it in `From`; the model doesn't need to store it:
```csharp
[Export] public float DamagePerSecond { get; set; }
...
DamagePerSecond = weapon.Damage * weapon.AttackSpeed,
```

### Show a number as a bar

```csharp
[Export(PropertyHint.Range, "0,100")] public float HealthPercent { get; set; }
```

### Show an enum as a dropdown

Export the enum directly. Godot puts the names in `hint_string`, so the view can show `Mage` instead of `1`:
```csharp
[Export] public CharacterClass Class { get; set; }
```
Nullable enums can't be exported; use a `string` (`"None"`) or add a `None` value to the enum.

### Show long text

```csharp
[Export(PropertyHint.MultilineText)] public string Description { get; set; }
```

### A list of simple values

```csharp
[Export] public Godot.Collections.Array<string> Inventory { get; set; } = [];
...
Inventory = new Godot.Collections.Array<string>(character.Inventory),
```

### Nested objects and lists of objects

Make a view model for the child and export it (or an array of it). The view builds a nested `ViewModelView`
for each one, recursively:
```csharp
[Export] public Godot.Collections.Array<MoonViewModel> Moons { get; set; } = [];
...
Moons = new Godot.Collections.Array<MoonViewModel>(planet.Moons.Select(MoonViewModel.From)),
```
A single child works the same way: `[Export] public MoonViewModel BiggestMoon { get; set; }`.

### Support a new type or hint in the view

Add a `case` to `CreateControl` in `ViewModelView.cs`, **above** the more general case for the same type
(a `when hint == …` case must come before the plain type case). For example, a `Vector2` as two SpinBoxes:
```csharp
case Variant.Type.Vector2:
    Vector2 v = value.AsVector2();
    var box = new HBoxContainer();
    box.AddChild(new SpinBox { MinValue = float.MinValue, MaxValue = float.MaxValue, Step = 0.01, Value = v.X, Editable = false });
    box.AddChild(new SpinBox { MinValue = float.MinValue, MaxValue = float.MaxValue, Step = 0.01, Value = v.Y, Editable = false });
    return box;
```

### Show different data / refresh

The view builds once, in `_Ready`. To show something else or newer data, build a new view model and replace
the view (as `DemoScreen.OnSampleSelected` does). Don't change a view model that is already shown; it is a
snapshot.

### Use the view from the editor (no code)

1. `dotnet build` so the editor knows the `[GlobalClass]` types.
2. Add a `ViewModelView` node to a scene (or a `PanelContainer` with `Scripts/Ui/ViewModelView.cs`).
3. In the inspector, create a new resource in `View Model`, for example `WeaponViewModel`, and fill in its
   values (or assign a saved `.tres`).
4. Run the scene: `_Ready` builds the UI from it.

## Gotchas

- **One file per Resource class, named after the class.** Godot finds a C# script by its file name; a Resource
  class in another class's file has no script, so its exported properties aren't listed.
- Resource classes must be `partial` (Godot's source generators add the property list code).
- `[GlobalClass]` makes the type show up in the editor's "New Resource" menu. The runtime demo works without
  it, but the editor recipe needs it.
- **SpinBox clamps values set from code** to `MinValue..MaxValue` (default 0..100), even with
  `AllowGreater`. Set the range before `Value` (object initializers run in order).
- Enum hint strings come as `"Name:value,…"` or `"Name,…"`; `EnumControl` handles both.
- `[ExportGroup]` / `[ExportCategory]` entries don't have the `ScriptVariable` flag, so the view skips them.
  Showing them as headers would need a check for `PropertyUsageFlags.Group`.
- The view model is only for display. Changing it does nothing to the model.

## Relation to ReanimatedLegacy

| ReanimatedLegacy | This demo |
|---|---|
| `SubChunkView` / domain data | `Models/*` |
| `ChunkViewModel`, `SubChunkViewModel` (`Backend/Glue/ViewModels/`) | `ViewModels/*` |
| `ViewModelService.Build(selection)` | `DemoScreen._samples` |
| `ViewModelPanel` (a `Tree`, follows `ViewModelService.Current`) | `ViewModelView` (one control per property, builds in `_Ready`) |
