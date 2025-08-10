using SideXP.Core;

using System;
using System.CodeDom;
using System.Runtime.Remoting.Contexts;

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
            return info.Name == PackageManifestName;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            info.Rename(PackageManifestName);

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

            output.Path = info.Path;
            output.Content = JsonUtility.ToJson(packageInfo, true);
            return true;
        }

    }

}
