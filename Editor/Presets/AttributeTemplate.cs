using System;
using System.CodeDom;

using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Attribute",
        "Generates a script for implementing a custom attribute.",
        "\"-Attribute\" suffix",
        "\"-Attr\" suffix"
    )]
    public class AttributeTemplate : IAssetTemplate
    {

        /// <summary>
        /// The pattern for matching prefix or suffix.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = null;

        static AttributeTemplate()
        {
            s_pattern = new PrefixSuffixPattern();
            s_pattern.AddPrefix("attr", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddPrefix("attribute", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddSuffix("attr", PrefixSuffixPattern.PartOptions.Default);
            s_pattern.AddSuffix("attribute", new PrefixSuffixPattern.PartOptions
            {
                FirstLetterCaseInsensitive = true,
                MatchIfNextLetterUppercase = true,
                RemovePart = false,
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
                Debug.LogError("Faield to generate class (or struct) from the \"Class / Struct\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.InheritFrom(typeof(Attribute), true);

            // [AttributeUsage] attribute
            scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<AttributeUsageAttribute>(), new CodeAttributeArgument[]
            {
                new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(nameof(AttributeTargets)), nameof(AttributeTargets.Class))),
                new CodeAttributeArgument(nameof(AttributeUsageAttribute.AllowMultiple), new CodePrimitiveExpression(false)),
                new CodeAttributeArgument(nameof(AttributeUsageAttribute.Inherited), new CodePrimitiveExpression(true)),
            }));

            if (className.EndsWith("attribute"))
                className = className.Substring(0, className.Length - "attribute".Length);
            if (!className.EndsWith("Attribute"))
                className += "Attribute";

            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;
            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

    }

}
