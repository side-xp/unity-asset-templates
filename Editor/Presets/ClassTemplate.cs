using System;
using System.CodeDom;
using System.Text.RegularExpressions;

using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Class / Struct",
        "Generates a script for a native class.\nIf a script is selected, the generated class will inherit from it.",
        "\"class-\" prefix  (followed by space or uppercase letter)",
        "\"-Class\" suffix",
        "\"struct-\" prefix (followed by space or uppercase letter, generates a struct instead)",
        "\"-Struct\" suffix (generates a struct instead)"
    )]
    public class ClassTemplate : IAssetTemplate
    {

        /// <summary>
        /// Matches the "class" or "struct" prefix, whether it uses a first lower or uppercase letter, but make sure the prefix is followed
        /// by a space or an uppercase letter.<br/>
        /// It outputs two groups:<br/>
        /// - "prefix": the used prefix (you should use <see cref="string.ToLower()"/> to normalize this value)<br/>
        /// - "name": the remaining part without the prefix (you should use <see cref="string.Trim()"/> on this value, as it may contaiin
        /// the preceeding space used for the prefix)
        /// </summary>
        private static readonly Regex PrefixPattern = new Regex(@"^(?<prefix>(?:[Cc]lass)|(?:[Ss]truct))(?=\s|[A-Z])(?<name>\s*\w+)");

        /// <summary>
        /// Matches the following suffixes:<br/>
        /// - space + "class" lowercase<br/>
        /// - space + "struct" lowercase<br/>
        /// - "Class" with first letter uppercase<br/>
        /// - "Struct" with first letter uppercase<br/>
        /// It outputs the group "suffix". You should use <see cref="string.ToLower()"/> to normalize this valu, and
        /// <see cref="string.Trim()"/> since this group also includes the starting space if any.
        /// </summary>
        private static readonly Regex SuffixPattern = new Regex(@"(?<suffix>(?:(?:\s+class)|(?:\s+struct))|(?:\s*Class)|(?:\s*Struct))$");

        [Tooltip("If checked, the [Serializable] attribute will be added to the generated class.")]
        public bool SerializableByDefault = true;

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            // Try match prefix
            Match match = PrefixPattern.Match(info.Name);
            if (match.Success)
                return true;

            // Try match suffix
            return SuffixPattern.Match(info.Name).Success;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            // Cancel if the class name can't be processed
            if (!ParseClassName(info.Name, out bool isClass, out string className))
            {
                Debug.LogError("Faield to generate class (or struct) from the \"Class\" asset template: invalid class name");
                return false;
            }

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

        /// <summary>
        /// Parse the given file name to get information about the expected output.
        /// </summary>
        /// <param name="name">The name of the file, as typed by the user.</param>
        /// <param name="isClass">Should the output script delcare a class or a struct?</param>
        /// <param name="className">Ouptuts the expected class name, without prefix or suffix.</param>
        /// <returns>Returns true if the given file name has been parsed successfully.</returns>
        private bool ParseClassName(string name, out bool isClass, out string className)
        {
            // Try match prefix
            Match match = PrefixPattern.Match(name);
            if (match.Success)
            {
                isClass = match.Groups["prefix"].Value.ToLower() == "class";
                className = match.Groups["name"].Value;
            }
            // Else, try match suffix
            else
            {
                match = SuffixPattern.Match(name);
                isClass = match.Groups["suffix"].Value.ToLower().Trim() == "class";
                className = name.Substring(0, name.Length - match.Groups["suffix"].Value.Length);
            }

            className = className.Trim();
            return !string.IsNullOrWhiteSpace(className);
        }

    }

}
