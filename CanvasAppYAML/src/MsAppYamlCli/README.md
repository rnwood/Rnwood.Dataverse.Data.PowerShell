# MsAppYamlCli

YAML-first pack/unpack console app for Power Apps `.msapp` files.

## Commands

- `unpack <input.msapp> <output-directory>`
  - Extracts the full package.
  - Primary editable source is `Src/*.pa.yaml` (including `Src/Components/*.pa.yaml`).

- `pack <input-directory> <output.msapp>`
  - Reads YAML from `Src` as source-of-truth.
  - Resolves `Control: ...` templates from:
    1. templates already present in the app being packed
    2. embedded `All controls.msapp` resource bundled in `MsAppToolkit`
  - Throws if a YAML control type cannot be matched to a template.
  - Validates datasource targets across formulas in the app and fails if any required datasource is missing from `References/DataSources.json`.
    - Includes `Collect(...)`, `ClearCollect(...)`, `Patch(...)`, `Remove(...)`, `RemoveIf(...)`, `Update(...)`, `UpdateIf(...)`, `Defaults(...)`, `Refresh(...)`.
  - Regenerates/updates derived artifacts:
    - `Controls/*.json`
    - `Components/*.json`
    - `ComponentsMetadata.json`
    - `Properties.json` (`ControlCount.screen`)
    - `Src/_EditorState.pa.yaml` order lists
  - Repackages to `.msapp`.

- `init-empty-app <output-directory> [screen-name]`
  - Creates a minimal unpacked app folder from scratch.
  - Includes required base artifacts (`Controls`, `Src`, `References`, `Properties.json`, etc.).
  - Seeds one empty screen (`Screen1` by default) and `_EditorState.pa.yaml`.
  - Intended as a CLI-only starting point for YAML-first authoring.

- `gen-collection-datasource <input-directory> <collection-name> <json-example-file>`
  - User-driven creation/update of a `StaticDataSourceInfo` entry in `References/DataSources.json`.
  - Infers schema from the JSON example payload and stores example rows in `Data`.

- `upsert-collection-datasource <input-directory> <collection-name> <json-example-file>`
  - Alias of `gen-collection-datasource`.

- `remove-datasource <input-directory> <datasource-name>`
  - Removes a datasource by name from `References/DataSources.json`.

- `upsert-dataverse-datasource <input-directory> <environment-url> <table-logical-name> [datasource-name] [dataset-name]`
  - Connects to Dataverse using interactive device-code auth.
  - Fetches table metadata directly from the environment (entity, attributes, option sets, views, forms).
  - Generates/updates datasource artifacts in the target unpack directory:
    - `References/DataSources.json`:
      - `NativeCDSDataSourceInfo`
      - related `OptionSetInfo` entries
      - related `ViewInfo` entry
    - `References/QualifiedValues.json` form mappings (`<Entity> (Forms)`).
  - Uses API id `/providers/microsoft.powerapps/apis/shared_commondataserviceforapps`.

- `list-control-templates`
  - Lists every control template available from the embedded `All controls.msapp` resource.
  - Prints template name, version, template id, and YAML control name.
  - Marks templates that **require a `Variant` keyword** with `[Variant required: ...]` showing the available variant values.
  - Variant-requiring controls include: `Gallery`, `Form`, `GroupContainer`, `Icon`, and several shape templates.

- `template-properties <template-name> [template-version]`
  - Prints properties discovered from the selected embedded template XML.
  - If version is omitted, uses the highest semantic version found for that template name.
  - Includes default values when present.
  - **Required YAML keywords** section: shows whether `Variant` (or other keywords) must be present to pack.
  - **Available variants** section: lists variant names sourced from both the template XML and existing control nodes in the embedded catalog.

## Example

1. `unpack "Test Canvas 3.msapp" ".\work\app3"`
2. Edit/add/remove YAML files under `work\app3\Src`
3. `upsert-dataverse-datasource ".\work\app3" "https://org60220130.crm11.dynamics.com" "contact" "Contacts"`
4. `pack ".\work\app3" "Test Canvas 3.updated.msapp"`
