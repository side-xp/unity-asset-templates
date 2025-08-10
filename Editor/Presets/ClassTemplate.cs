using System;
using System.CodeDom;

using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Class / Struct",
        "Generates a script for a native class.\nIf a script is selected, the generated class will inherit from it.",
        "\"class-\" prefix (followed by space or uppercase letter)",
        "\"struct-\" prefix (followed by space or uppercase letter, generates a struct instead)" +
        "If another script is currently selected, try to inherit from it"
    )]
    public class ClassTemplate : IAssetTemplate
    {

        /// <summary>
        /// The pattern for matching prefix or suffix.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = new PrefixSuffixPattern("class", "struct");

        [Tooltip("If checked, the [Serializable] attribute will be added to the generated class.")]
        public bool SerializableByDefault = true;

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

            bool isClass = matchingPart.ToLower() == "class";
            ScriptGenerator scriptGenerator = new ScriptGenerator(info);

            // If the user wants to generate a class script, apply inheritance
            if (isClass)
            {
                scriptGenerator.InheritFromContext();
            }
            // Else, if the user wants to generate a struct script
            else
            {
                scriptGenerator.MainClass.IsStruct = true;
                // Inherit only if the selected parent is an interface
                if (info.ParentType != null && info.ParentType.IsInterface)
                    scriptGenerator.InheritFromContext();
            }

            // [Serializable] attribute
            if (SerializableByDefault)
                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<SerializableAttribute>(true, true)));

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;
            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

    }

}
