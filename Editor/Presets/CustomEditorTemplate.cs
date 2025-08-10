using System.CodeDom;

using UnityEngine;
using UnityEditor;

using SideXP.Core;

using Object = UnityEngine.Object;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Editor",
        "Generates a script for a custom Editor.",
        "\"editor-\" prefix (followed by space or uppercase letter)",
        "\"-Editor\" suffix" +
        "If another Object script is currently selected, it's used as value for the [CustomEditor] attribute",
        "\"editor\" alone, if another Object script is currently selected, so the name of the class will be [selected parent]Editor"
    )]
    public class CustomEditorTemplate : IAssetTemplate
    {

        /// <summary>
        /// The pattern for matching prefix or suffix.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = new PrefixSuffixPattern("editor");

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return s_pattern.Match(info.Name) || info.Name.ToLower() == "editor";
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            string className = null;
            // If the name used is just "editor" shortcut
            if (info.Name.ToLower() == "editor")
            {
                // Cancel if there's no selected script to decorate
                if (info.ParentType == null || !info.ParentType.Is<Object>())
                {
                    Debug.LogError($"Failed to generate script from the \"Editor\" asset template: you must select a script that declares a {nameof(UnityEngine)}.{nameof(Object)} class if you want to use the \"editor\" shortcut.");
                    return false;
                }
            }
            // Try to match prefix/suffix
            else if (!s_pattern.Match(info.Name, out className, out string matchingPart, out _))
            {
                Debug.LogError("Failed to generate class (or struct) from the \"Editor\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.InheritFrom(typeof(Editor));

            scriptGenerator.ImportsNamespace.Imports.Add(new CodeNamespaceImport(nameof(UnityEngine)));
            scriptGenerator.ImportsNamespace.Imports.Add(new CodeNamespaceImport(nameof(UnityEditor)));

            // Use the parent type or class name without prefix or suffix as value for the [CustomEditor] attribute
            string decoratedClassName = info.ParentType != null && info.ParentType.Is<Object>()
                ? info.ParentType.Name
                : className;

            // Fix class name if needed (at this point, if the "editor" shortcut has been used, this value is still empty)
            if (string.IsNullOrWhiteSpace(className))
                className = decoratedClassName;

            // Add the "-Editor" suffix for the generated editor script
            className += "Editor";
            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;

            // [CustomEditor] attribute
            if (!string.IsNullOrWhiteSpace(decoratedClassName))
            {
                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<CustomEditor>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodeTypeOfExpression(new CodeTypeReference(decoratedClassName)))
                }));
            }

            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

    }

}
