namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Text (*.txt)",
        "Generates an empty text file.",
        "File extension is *.txt",
        Order = 100
    )]
    public class TextTemplate : IAssetTemplate
    {

        private const string TextExtension = "txt";

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return info.Extension == TextExtension;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            output.Content = string.Empty;
            return true;
        }

    }

}
