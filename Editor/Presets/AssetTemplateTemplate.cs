using System;
using System.CodeDom;

using UnityEngine;
using UnityEditor;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Asset Template",
        "Generates a script that implements " + nameof(IAssetTemplate) + " for creating your own asset templates.",
        "\"template-\" prefix (followed by space or uppercase letter)",
        "\"AssetTemplate-\" suffix"
    )]
    public class AssetTemplateTemplate : IAssetTemplate
    {

        private const string ContentMarkerComment = "// [REPLACE WITH SCRIPT CONTENT]";

        /// <summary>
        /// The pattern for matching prefixes or suffixes.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = null;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static AssetTemplateTemplate()
        {
            s_pattern = new PrefixSuffixPattern();
            s_pattern.AddPrefix("template");
            s_pattern.AddSuffix("AssetTemplate", new PrefixSuffixPattern.PartOptions
            {
                FirstLetterCaseInsensitive = true,
                RemovePart = false
            });
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
            if (!s_pattern.Match(info.Name, out string className, out string matchingPart, out _))
            {
                Debug.LogError("Failed to generate class (or struct) from the \"Class / Struct\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.InheritFrom(typeof(IAssetTemplate));

            // [Serializable] attribute
            scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<SerializableAttribute>(true, true)));

            // [AssetTemplate] attribute
            {
                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<AssetTemplateAttribute>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodePrimitiveExpression(ObjectNames.NicifyVariableName(className))),
                    new CodeAttributeArgument(new CodePrimitiveExpression(ObjectNames.NicifyVariableName("@todo What is this asset template purpose?"))),
                    new CodeAttributeArgument(new CodePrimitiveExpression(ObjectNames.NicifyVariableName("@todo List here the prefixes or suffixes triggers this template..."))),
                }));
            }

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;
            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();
            output.IsEditorContent = true;

            return true;
        }

    }

}
