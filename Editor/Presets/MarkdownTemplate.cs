namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "MarkDown (*.md)",
        "Generates a basic MarkDown file.",
        "File extension is *.md",
        Order = 100
    )]
    public class MarkdownTemplate : IAssetTemplate
    {

        private const string MarkdownExtension = "md";

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return info.Extension == MarkdownExtension;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            output.Content = "# Title" +
                "\n" +
                "\n[=> MarkDown Cheat Sheet](https://www.markdownguide.org/cheat-sheet)";
            return true;
        }

    }

}
