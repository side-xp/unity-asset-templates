using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Class",
        "Generates a script for a native class.\nIf a script is selected, the generated class will inherit from it.",
        "\"class-\" prefix",
        "\"-Class\" suffix",
        "\"struct-\" prefix (generates a struct instead)",
        "\"-Struct\" suffix (generates a struct instead)"
    )]
    public class ClassTemplate : IAssetTemplateGenerator
    {

        [Tooltip("If checked, the [Serializable] attribute will be added to the generated class.")]
        public bool SerializableByDefault = true;

        /// <inheritdoc cref="IAssetTemplateGenerator.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc cref="IAssetTemplateGenerator.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo outputInfo)
        {
            throw new System.NotImplementedException();
        }

    }

}
