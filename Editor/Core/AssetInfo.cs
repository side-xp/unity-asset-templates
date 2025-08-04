using SideXP.Core;

using System;
namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Information about an asset to create.
    /// </summary>
    public struct AssetInfo
    {

        //  Base info

        /// <summary>
        /// The name of the asset to create, as defined by the user.<br/>
        /// </summary>
        public string Name;

        /// <summary>
        /// The extension typed by the user, without the "." character.
        /// </summary>
        public string Extension;

        /// <summary>
        /// The relative path of the asset to create, from the root folder of this project.
        /// </summary>
        public string Location;

        /// <summary>
        /// The namespace used for the selected folder.
        /// </summary>
        public string Namespace;

        //  Inheritance

        /// <summary>
        /// The path of the "parent" file, the one that was selected when the asset creation was performed.
        /// </summary>
        public string ParentPath;

        /// <summary>
        /// The type declared in the parent file, if it's valid and is a script.
        /// </summary>
        public Type ParentType;

        /// <inheritdoc cref="AssetInfo"/>
        /// <param name="path">The path of the asset to create, relative to this project's root directory.</param>
        /// <param name="namespaceStr"><inheritdoc cref="Namespace" path="/summary"/></param>
        /// <param name="parentPath"><inheritdoc cref="ParentPath" path="/summary"/></param>
        /// <param name="parentType"><inheritdoc cref="ParentType" path="/summary"/></param>
        internal AssetInfo(string path, string namespaceStr, string parentPath, Type parentType)
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Location = System.IO.Path.GetDirectoryName(path);
            Extension = System.IO.Path.GetExtension(path);
            // Remove the "." character of the extension, if any
            if (!string.IsNullOrEmpty(Extension))
                Extension = Extension.Substring(0, 1);

            Namespace = namespaceStr;
            ParentPath = parentPath;
            ParentType = parentType;
        }

        /// <summary>
        /// The namespace of the <see cref="ParentType"/>, if applicable.
        /// </summary>
        public string ParentNamespace => ParentType != null ? ParentType.Namespace : null;

        /// <summary>
        /// The full path to the asset to create, relative from this project's root directory.
        /// </summary>
        public string Path => GetUniquePath(Name);

        /// <summary>
        /// Gets the full path to the asset to create with a given name, relative from this project's root directory.
        /// </summary>
        /// <param name="name">The name of the asset to create.</param>
        /// <returns>Returns the processed path.</returns>
        private string GetUniquePath(string name)
        {
            string output = $"{Location}/{Name}";
            string ext = Extension;
            if (string.IsNullOrWhiteSpace(ext))
                ext = AssetTemplatesConfig.DefaultExtension;
            if (!string.IsNullOrWhiteSpace(ext))
                output += $".{ext}";

            return output.ToRelativePath();
        }

    }

}
