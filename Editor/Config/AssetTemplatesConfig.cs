using UnityEngine;

using SideXP.Core.EditorOnly;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Editor config for the Asset Templates package features.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Project)]
    public class AssetTemplatesConfig : ScriptableObject, IEditorConfig
    {

        [SerializeField]
        [Tooltip("Defines the default extension for assets created from the Asset Template menu.")]
        private string _defaultExtension = "cs";

        // Persistent data

        [SerializeField, HideInInspector]
        [Tooltip("The settings defined for available asset templates, serialized as a JSON string.")]
        private string _assetTemplatesSettings = string.Empty;

        /// <summary>
        /// Gets the loaded settings or load them from disk if not already.
        /// </summary>
        public static AssetTemplatesConfig Instance => EditorConfigUtility.Get<AssetTemplatesConfig>();

        /// <inheritdoc cref="Instance"/>
        public static AssetTemplatesConfig I => Instance;

        /// <inheritdoc cref="_defaultExtension"/>
        public static string DefaultExtension
        {
            get => I._defaultExtension;
            set => I._defaultExtension = value;
        }

        /// <inheritdoc cref="IEditorConfig.PostLoad"/>
        public void PostLoad() { }

        /// <inheritdoc cref="_assetTemplatesSettings"/>
        internal static string AssetTemplatesSettings
        {
            get => I._assetTemplatesSettings;
            set
            {
                I._assetTemplatesSettings = value;
                EditorConfigUtility.Save(I);
            }
        }

    }

}
