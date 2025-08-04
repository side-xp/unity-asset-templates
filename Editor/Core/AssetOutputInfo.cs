using System.Collections.Generic;

using SideXP.Core;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Information about an asset (or a group of assets) generated from a template.
    /// </summary>
    public struct AssetOutputInfo
    {

        /// <summary>
        /// 
        /// </summary>
        private List<string> _paths;

        /// <summary>
        /// States that the template has generated an asset at a given path.
        /// </summary>
        /// <remarks>
        /// This is used to make sure Unity imports and updates the database as expected.
        /// </remarks>
        /// <param name="path">The path (relative to the Assets/ folder of the project) of the created asset.</param>
        public void DidGenerateAssetAtPath(string path)
        {
            path = path.ToRelativePath();
            if (!_paths.Contains(path))
                _paths.Add(path);
        }

        /// <summary>
        /// Gets the paths to the generated assets.
        /// </summary>
        public string[] Paths
        {
            get
            {
                _paths.RemoveAll(i => string.IsNullOrEmpty(i));
                return _paths.ToArray();
            }
        }

    }

}
