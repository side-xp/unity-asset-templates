using UnityEngine;

using SideXP.Core.EditorOnly;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Editor config for the Asset Templates package features.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Project)]
    public class AssetTemplatesConfig : EditorConfig<AssetTemplatesConfig>
    {

        [SerializeField]
        [Tooltip("Defines the default extension for assets created from the Asset Template menu.")]
        private string _defaultExtension = "cs";

        // Persistent data

        [SerializeField, HideInInspector]
        [Tooltip("The settings defined for available asset templates, serialized as a JSON string.")]
        private string _assetTemplatesSettings = string.Empty;

        /// <inheritdoc cref="_defaultExtension"/>
        public static string DefaultExtension
        {
            get => I._defaultExtension;
            set => I._defaultExtension = value;
        }

        /// <inheritdoc cref="_assetTemplatesSettings"/>
        internal static string AssetTemplatesSettings
        {
            get => I._assetTemplatesSettings;
            set
            {
                I._assetTemplatesSettings = value;
                I.Save();
            }
        }

    }

}
