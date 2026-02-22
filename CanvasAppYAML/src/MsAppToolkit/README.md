# MsAppToolkit (C#)

Reverse-engineered toolkit for reading and manipulating Power Apps `.msapp` packages.

## What this library understands

From the provided samples (`Test Canvas.msapp` and `Test Canvas 2.msapp`) plus official schema references:

- `.msapp` is a ZIP container.
- `Controls/*.json` contains canonical control trees used at load/runtime-like layers.
- `Src/*.pa.yaml` is source-control oriented YAML projection for app/screen/component definitions.
- `References/Templates.json` stores used control templates (XML), including default property values.
- `References/Themes.json` stores style maps and palette entries that feed default style values.
- Newer apps may also include:
  - `Components/*.json` for component definition control trees
  - `ComponentsMetadata.json` for component metadata
  - `References/QualifiedValues.json` for qualified lookup/value maps

### Screen/control relationship

- Each file in `Controls/*.json` has a `TopParent` node.
- A `TopParent` with template `screen` is a screen root.
- Child controls are in `Children` arrays recursively.
- Parent-child relation uses:
  - structural nesting in `Children`
  - `Parent` name field
  - stable identity with `ControlUniqueId`

### Template/style relationship

Effective property value precedence implemented by this library:

1. Template defaults (`References/Templates.json` XML `defaultValue` on `property` / `includeProperty`)
2. Theme style defaults (`References/Themes.json` `styles[].propertyValuesMap`, with `%Palette.*%` token resolution)
3. Explicit control rules (`Rules[].InvariantScript`)

## Core API

- `MsAppDocument.Load(path)`
- `Save(path)`
- `GetTopParents()`
- `GetScreens()`
- `GetComponentDefinitions()`
- `FindControlByName(name)`
- `ResolveEffectiveProperties(control)`
- `ExportScreenYaml(screenName, includeHeader = true, includeDefaults = false)`
- `ImportScreenYaml(yaml)`

## YAML behavior

- Export emits schema-style `Screens:` mapping with nested `Children` items.
- Control type is emitted as `Control: Name@version`.
- Properties are emitted as Power Fx formulas prefixed with `=`.
- Import updates `Rules` and `ControlPropertyState`, and rehydrates/reorders child trees from YAML.

## Notes

- This is intentionally pragmatic reverse engineering based on real sample artifacts.
- `.pa.yaml` is modeled as editable here for automation workflows, despite official docs describing it as primarily source-control/review output.
- Unknown JSON fields are preserved unless affected by targeted manipulations.
- Template resolution for YAML-added controls uses templates from the app first, then a bundled embedded `All controls.msapp` reference, and finally an external nearby `All controls.msapp` if present.
- During YAML-first pack, formulas are scanned for `Collect` / `ClearCollect` targets and packing fails if any referenced collection is missing from `References/DataSources.json`.
