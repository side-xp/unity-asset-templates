using System.CodeDom;

using UnityEngine;
using UnityEditor;

using SideXP.Core;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Component",
        "Generates a script for a component.",
        "\"comp-\" prefix (followed by space or uppercase letter)",
        "\"-Comp\" suffix",
        "\"component-\" prefix (followed by space or uppercase letter)",
        "\"-Component\" suffix",
        "If another Component script is currently selected, try to inherit from it"
    )]
    public class ComponentTemplate : IAssetTemplate
    {

        /// <summary>
        /// The pattern for matching prefix or suffix.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = new PrefixSuffixPattern("comp", "component");

        [Tooltip("The base URL to use for the [HelpURL] attribute of the generated component scripts." +
            "\nIf empty, the [HelpURL] attribute won't be added in the generated script.")]
        public string BaseHelpURL = string.Empty;

        [Tooltip("The base string to use for the [AddComponentMenu] attribute of the generated component scripts." +
            "\nIt must not end with a slash character." +
            "\nIf empty, the [AddComponentMenu] attribute won't be added in the generated script.")]
        public string BaseAddComponentMenu = string.Empty;

        [Tooltip("By default, the \"-Comp\" or \"-Component\" suffix is left as part of the name. If enabled, the suffix will be removed." +
            "\nNote that it doesn't applies to the prefix, which will alsways be removed from the final name.")]
        public bool RemoveSuffix = false;

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return s_pattern.Match(info.Name);
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            // Cancel if the class name can't be processed
            if (!s_pattern.Match(info.Name, out string className, out string matchingPart, out bool isPrefix))
            {
                Debug.LogError("Failed to generate class (or struct) from the \"Component\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            // Inherit from the parent type if applicable and if it derives from MonoBegaviour
            if (scriptGenerator.Info.ParentType != null && scriptGenerator.Info.ParentType.Is(typeof(MonoBehaviour)))
                scriptGenerator.InheritFromContext();
            // Otherwuse just inherit from MonoBehaviour
            else
                scriptGenerator.InheritFrom(typeof(MonoBehaviour));

            // Add "using UnityEngine"
            scriptGenerator.ImportsNamespace.Imports.Add(new CodeNamespaceImport(nameof(UnityEngine)));

            // [HelpURL] attribute
            if (!string.IsNullOrWhiteSpace(BaseHelpURL))
            {
                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<HelpURLAttribute>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodePrimitiveExpression(BaseHelpURL))
                }));
            }

            // [AddComponentMenu] attribute
            {
                string componentMenu = ObjectNames.NicifyVariableName(info.Name);
                if (!string.IsNullOrWhiteSpace(BaseAddComponentMenu))
                    componentMenu = $"{BaseAddComponentMenu}/{componentMenu}";

                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<AddComponentMenu>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodePrimitiveExpression(componentMenu))
                }));
            }

            // If the matching part is a suffix, and it shouldn't be removed, add the suffix
            if (!isPrefix && !RemoveSuffix)
            {
                if (matchingPart.ToLower() == "comp")
                    className += "Comp";
                else if (matchingPart.ToLower() == "component")
                    className += "Component";
            }

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;

            // [AddComponentMenu] attribute
            {
                string componentMenu = ObjectNames.NicifyVariableName(info.Name);
                if (!string.IsNullOrWhiteSpace(BaseAddComponentMenu))
                    componentMenu = $"{BaseAddComponentMenu}/{componentMenu}";

                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<AddComponentMenu>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodePrimitiveExpression(componentMenu))
                }));
            }

            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

    }

}
