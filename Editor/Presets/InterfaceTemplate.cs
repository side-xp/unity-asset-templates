using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Interface",
        "Generates a script to declare an interface.",
        "\"interface-\" prefix (followed by space or uppercase letter)",
        "\"I-\" prefix (followed by uppercase letter)",
        "\"-Interface\" suffix",
        "If another Interface script is currently selected, try to inherit from it"
    )]
    public class InterfaceTemplate : IAssetTemplate
    {

        /// <summary>
        /// The pattern for matching prefixes or suffixes.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = null;

        /// <summary>
        /// If enabled, always add a IE-" prefix in the generated script.
        /// </summary>
        public bool AlwaysUseLetterPrefix = true;

        public InterfaceTemplate()
        {
            s_pattern = new PrefixSuffixPattern();
            s_pattern.AddPrefix("interface", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddSuffix("interface", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddPrefix("I", PrefixSuffixPattern.PartOptions.Default);
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
                Debug.LogError("Failed to generate class (or struct) from the \"Interface\" asset template: invalid class name");
                return false;
            }

            // Add "I-" prefix if required
            if (AlwaysUseLetterPrefix)
                className = "I" + className;

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.MainClass.IsInterface = true;

            // Inherit from selected script if it's an interface
            if (scriptGenerator.Info.ParentType != null && scriptGenerator.Info.ParentType.IsInterface)
                scriptGenerator.InheritFromContext(true);

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;
            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

    }

}
