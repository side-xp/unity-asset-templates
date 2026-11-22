using UnityEditor;

using SideXP.Core.EditorOnly;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Generate menus to edit the Asset Templates package editor config.
    /// </summary>
    public static class AssetTemplatesConfigProvider
    {

        /// <summary>
        /// Registers the page to edit the Asset Templates package settings from the Project Settings window.
        /// </summary>
        /// <returns>Returns the created settings provider.</returns>
        [SettingsProvider]
        private static SettingsProvider RegisterProjectSettingsMenu()
        {
            return EditorConfigSettingsProvider.Create<AssetTemplatesConfig>(
                EditorConstants.ProjectSettings + "/Templates",
                SettingsScope.Project
            );
        }

    }

}
