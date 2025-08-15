using System;
using System.CodeDom;

using UnityEngine;
using UnityEngine.Device;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Enum / Flags",
        "Generates a script to declare an enum, with optional [Flags] attribute.",
        "\"enum-\" prefix (followed by space or uppercase letter)",
        "\"E-\" prefix (followed by uppercase letter)",
        "\"-Enum\" suffix",
        "\"flags-\" prefix (followed by space or uppercase letter, adds the [Flags] attribute)",
        "\"F-\" prefix (followed by uppercase letter, adds the [Flags] attribute)",
        "\"-Flags\" suffix (adds the [Flags] attribute)"
    )]
    public class EnumTemplate : IAssetTemplate
    {

        /// <summary>
        /// The pattern for matching prefixes or suffixes.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = null;

        /// <summary>
        /// If enabled, always add a "E-" prefix for enums or "F-" prefix for flags in the generated script.
        /// </summary>
        public bool AlwaysUseLetterPrefix = true;

        public EnumTemplate()
        {
            s_pattern = new PrefixSuffixPattern();
            s_pattern.AddPrefix("enum", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddSuffix("enum", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddPrefix("E", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddPrefix("flags", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddSuffix("flags", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddPrefix("F", PrefixSuffixPattern.PartOptions.Default);
        }

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return s_pattern.Match(info.Name);
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            // Cancel if the name can't be processed
            if (!s_pattern.Match(info.Name, out string className, out string matchingPart, out _))
            {
                Debug.LogError("Failed to generate class (or struct) from the \"Enum / Flags\" asset template: invalid class name");
                return false;
            }

            bool isFlags = matchingPart.ToLower() == "flags" || matchingPart.ToLower() == "f";

            // Add "E-" or "F-" prefix if required
            if (AlwaysUseLetterPrefix)
            {
                if (matchingPart.ToLower() == "enum" || matchingPart.ToLower() == "e")
                    className = "E" + className;
                if (isFlags)
                    className = "F" + className;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.MainClass.IsEnum = true;

            // Add [Flags] attribute if needed, and example content
            if (isFlags)
            {
                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<FlagsAttribute>(true, true)));

                // Add "Black = 0" field
                scriptGenerator.MainClass.Members.Add(new CodeMemberField{ Name = "Black", InitExpression = new CodePrimitiveExpression(0) });
                // Add bitshift fields
                scriptGenerator.MainClass.Members.Add(new CodeMemberField{ Name = "Red", InitExpression = new CodeSnippetExpression("1 << 0") });
                scriptGenerator.MainClass.Members.Add(new CodeMemberField{ Name = "Green", InitExpression = new CodeSnippetExpression("1 << 1") });
                scriptGenerator.MainClass.Members.Add(new CodeMemberField{ Name = "Blue", InitExpression = new CodeSnippetExpression("1 << 2") });
                // Add combination fields
                scriptGenerator.MainClass.Members.Add(new CodeMemberField { Name = "Yellow", InitExpression = new CodeSnippetExpression("Red | Green") });
                scriptGenerator.MainClass.Members.Add(new CodeMemberField { Name = "Magenta", InitExpression = new CodeSnippetExpression("Red | Blue") });
                scriptGenerator.MainClass.Members.Add(new CodeMemberField { Name = "Cyan", InitExpression = new CodeSnippetExpression("Green | Blue") });
                scriptGenerator.MainClass.Members.Add(new CodeMemberField { Name = "White", InitExpression = new CodeSnippetExpression("Red | Green | Blue") });
            }

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;
            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

    }

}
