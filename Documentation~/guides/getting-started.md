# Getting Started

## Common usage

- Go to `Assets > Create > Create Asset From Template`
- Name your file using the convention of one of the asset templates available in the project
- Wait for Unity to recompile (if the created file is a script)

## Templates Options

- Go to `Edit > Preferences > Sideways Experiments > Templates`
- Edit settings per-template by expanding the fields

From this menu, you can also check for what triggers a template by hovering its name. These trigger rules are displayed as a property tooltip.

## Example with a `ScriptableObject` script

The *ScriptableO bject* asset template is meant to be triggered when you create a file:

- with the `scriptable-` prefix
- with the `-ScriptableObject`, `-Scriptable` or `-SO` suffix

So, if you want to generate a script form the *Scriptable Object* template:

- Go to `Assets > Create > Create Asset From Template`
- Name the file `PlayerDataSO`
- Wait for Unity to recompile