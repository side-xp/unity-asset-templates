using UnityEngine;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Package Manifest (package.json)",
        "Generates a package manifest (package.json) with basic configuration.",
        "File name must be exactly package.json"
    )]
    public class PackageManifestTemplate : IAssetTemplate
    {

#pragma warning disable IDE1006 // Naming Styles
        [System.Serializable]
        private struct PackageInfo
        {
            public string name;
            public string version;
            public string description;
            public string displayName;
            public string unity;
            public PackageAuthorInfo author;
        }

        [System.Serializable]
        private struct PackageAuthorInfo
        {
            public string name;
            public string email;
            public string url;
        }
#pragma warning restore IDE1006 // Naming Styles

        private const string PackageManifestName = "package.json";

        public string PackageDefaultName = "com.company.name";
        public string PackageDefaultVersion = "0.0.1";
        public string PackageDefaultDisplayName = "New Package";
        public string PackageAuthorName = "";
        public string PackageAuthorEmail = "";
        public string PackageAuthorUrl = "";

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            // info.Name is the file name without its extension, so compare against the full "<name>.<ext>"
            return $"{info.Name}.{info.Extension}" == PackageManifestName;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            PackageInfo packageInfo = new PackageInfo
            {
                name = PackageDefaultName,
                version = PackageDefaultVersion,
                description = "",
                displayName = PackageDefaultDisplayName,
                unity = Application.unityVersion,
                author = new PackageAuthorInfo
                {
                    name = PackageAuthorName,
                    email = PackageAuthorEmail,
                    url = PackageAuthorUrl
                }
            };

            // The manifest must be named exactly "package.json" (no unique-suffix, no re-appended extension)
            output.Path = $"{info.Location}/{PackageManifestName}";
            output.Content = JsonUtility.ToJson(packageInfo, true);
            return true;
        }

    }

}
