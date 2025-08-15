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

        /// <summary>
        /// Sets the extension of the output file.
        /// </summary>
        /// <remarks>This function will update the <see cref="Path"/> value accordingly.</remarks>
        /// <param name="extension">The extension to set (without the "." char).</param>
        /// <returns>Returns the new path, with the extension changed.</returns>
        public string SetExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return Path;

            _path = Path.Substring(0, Path.Length - (System.IO.Path.GetExtension(Path).Length - 1));
            _path += extension;
            return _path;
        }

    }

}
