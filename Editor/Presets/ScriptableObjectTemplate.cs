using System.CodeDom;

using UnityEngine;
using UnityEditor;

using SideXP.Core;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Scriptable Object",
        "Generates a script for a ScriptableObject class.",
        "\"scriptable-\" prefix (followed by space or uppercase letter)",
        "\"asset-\" prefix (followed by space or uppercase letter)",
        "\"-ScriptableObject\" suffix",
        "\"-Scriptable\" suffix",
        "\"-SO\" suffix",
        "\"-Asset\" suffix"
    )]
    public class ScriptableObjectTemplate : IAssetTemplate
    {

        /// <summary>
        /// The pattern for matching prefixes or suffixes.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = null;

        [Tooltip("The base URL to use for the [HelpURL] attribute of the generated scripts." +
            "\nIf empty, the [HelpURL] attribute won't be added in the generated script.")]
        public string BaseHelpURL = string.Empty;

        [Tooltip("The base string to use for the [CreateAssetMenu] attribute of the generated scripts." +
            "\nIt must not end with a slash character." +
            "\nIf empty, the [CreateAssetMenu] attribute won't be added in the generated script.")]
        public string BaseCreateAssetMenu = string.Empty;

        [Tooltip("By default, the suffix used to trigger this asset template is left as is." +
            "\nIf checked, the suffix will be removed from the name of both the file and the class itself.")]
        public bool RemoveSuffix = false;

        public ScriptableObjectTemplate()
        {
            s_pattern = new PrefixSuffixPattern();
            s_pattern.AddPrefix("scriptable");
            s_pattern.AddPrefix("asset");
            s_pattern.AddSuffix("Scriptable");
            s_pattern.AddSuffix("ScriptableObject");
            s_pattern.AddSuffix("SO");
            s_pattern.AddSuffix("Asset");
        }

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
                Debug.LogError("Failed to generate class (or struct) from the \"Scriptable Object\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            if (scriptGenerator.Info.ParentType != null && scriptGenerator.Info.ParentType.Is(typeof(ScriptableObject)))
                scriptGenerator.InheritFromContext();
            else
                scriptGenerator.InheritFrom(typeof(ScriptableObject));

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

            // [CreateAssetMenu] attribute
            {
                string menuName = ObjectNames.NicifyVariableName(className);
                if (!string.IsNullOrWhiteSpace(BaseCreateAssetMenu))
                    menuName = $"{BaseCreateAssetMenu}/{menuName}";

                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<CreateAssetMenuAttribute>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(nameof(CreateAssetMenuAttribute.fileName), new CodePrimitiveExpression("New" + className)),
                    new CodeAttributeArgument(nameof(CreateAssetMenuAttribute.menuName), new CodePrimitiveExpression(menuName)),
                }));
            }

            // If the matching part is a suffix, and it shouldn't be removed, add the suffix
            if (!isPrefix && !RemoveSuffix)
                 className += matchingPart.Trim();

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;

            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

    }

}
