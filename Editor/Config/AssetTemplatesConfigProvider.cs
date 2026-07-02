using UnityEditor;

using SideXP.Core.EditorOnly;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Generate menus to edit the Asset Templates package editor config.
    /// </summary>
    public class AssetTemplatesConfigProvider : DefaultConfigSettingsProvider
    {

        [SettingsProvider]
        private static SettingsProvider RegisterProjectSettingsMenu()
        {
            return MakeSettingsProvider(AssetTemplatesConfig.I, EditorConstants.ProjectSettings + "/Templates", SettingsScope.Project);
        }

    }

}
