using UnityEditor;

using SideXP.Core.EditorOnly;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Generate menus to edit the Core package editor config.
    /// </summary>
    public class AssetTemplatesConfigProvider : DefaultConfigSettingsProvider
    {

        [SettingsProvider]
        private static SettingsProvider RegisterUserSettingsMenu()
        {
            return MakeSettingsProvider(AssetTemplatesConfig.I, EditorConstants.Preferences + "/Templates", SettingsScope.User);
        }

    }

}
