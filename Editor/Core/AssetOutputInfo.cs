using SideXP.Core;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Information about an asset (or a group of assets) generated from a template.
    /// </summary>
    public struct AssetOutputInfo
    {

        /// <summary>
        /// The path to the generated asset, relative to the Assets/ folder of the project.
        /// </summary>
        private string _path;

        /// <summary>
        /// The content of the asset to generate.
        /// </summary>
        private string _content;

        /// <inheritdoc cref="_path"/>
        public string Path
        {
            get => _path;
            set => _path = !string.IsNullOrWhiteSpace(value) ? value.ToRelativePath() : string.Empty;
        }

        /// <inheritdoc cref="_content"/>
        public string Content
        {
            get => _content;
            set => _content = value;
        }

    }

}
