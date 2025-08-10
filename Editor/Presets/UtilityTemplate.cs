using System;
using System.CodeDom;

using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Utility",
        "Generates a script with a static class to create a utility with helper functions.",
        "\"utility-\" prefix (followed by space or uppercase letter)",
        "\"helper-\" prefix (followed by space or uppercase letter)",
        "\"extension-\" prefix (followed by space or uppercase letter)",
        "\"extensions-\" prefix (followed by space or uppercase letter)",
        "\"-Utility\" suffix",
        "\"-Helper\" suffix",
        "\"-Extension\" suffix",
        "\"-Extensions\" suffix"
    )]
    public class UtilityTemplate : IAssetTemplate
    {

        private const string ClassDeclarationStr = "public class";
        private const string StaticClassDeclarationStr = "public static class";

        /// <summary>
        /// The pattern for matching prefix or suffix.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = new PrefixSuffixPattern("utility", "helper", "extension", "extensions");

        [Tooltip("By default, the suffix used to trigger this asset template is left as is." +
            "\nIf checked, the suffix will be removed from the name of both the file and the class itself.")]
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
                Debug.LogError("Failed to generate class (or struct) from the \"Utility\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.MainClass.Attributes |= MemberAttributes.Static;

            // Add the matching suffix if required
            if (!isPrefix && !RemoveSuffix)
                className += matchingPart;

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;
            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            // @fix https://stackoverflow.com/a/6308395/6699339
            // The C# CodeDom provider doesn't support the static keyword because of Microsoft policy: they want the CodeDom to be
            // language-independant but since the static keyword doesn't exist in VB, they won't add it for C#.
            output.Content = output.Content.Replace(ClassDeclarationStr, StaticClassDeclarationStr);

            return true;
        }

    }

}
