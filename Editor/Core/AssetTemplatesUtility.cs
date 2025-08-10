using System;
using System.Collections;
using System.Collections.Generic;

using SideXP.Core;
using SideXP.Core.Reflection;

using UnityEngine;
using UnityEditor;

namespace SideXP.AssetTemplates.EditorOnly
{

    public class AssetTemplatesUtility : MonoBehaviour
    {

        /// <summary>
        /// Serialized data for asset template settings.
        /// </summary>
        [System.Serializable]
        private class SerializedAssetTemplateSettings
        {
            public string Type = string.Empty;
            public string Json = string.Empty;
            public bool Enabled = true;

            /// <inheritdoc cref="SerializedAssetTemplateSettings"/>
            public SerializedAssetTemplateSettings(Type assetTemplateType)
            {
                Type = assetTemplateType.AssemblyQualifiedName;
                Json = string.Empty;
                Enabled = !assetTemplateType.TryGetAttribute(out AssetTemplateAttribute attr) || !attr.DisabledByDefault;
            }
        }

        /// <summary>
        /// Serialized data for settings of asset templates ensemble.
        /// </summary>
        /// <remarks>Used for serialization.</remarks>
        [System.Serializable]
        private class SerializedAssetTemplatesSettingsGroup : IEnumerable<SerializedAssetTemplateSettings>
        {
            public SerializedAssetTemplateSettings[] Settings = null;

            public SerializedAssetTemplatesSettingsGroup()
            {
                Settings = new SerializedAssetTemplateSettings[0];
            }

            public SerializedAssetTemplatesSettingsGroup(SerializedAssetTemplateSettings[] settings)
            {
                Settings = settings;
            }

            public IEnumerator<SerializedAssetTemplateSettings> GetEnumerator()
            {
                return ((IEnumerable<SerializedAssetTemplateSettings>)Settings).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Settings).GetEnumerator();
            }
        }

        /// <summary>
        /// Informations about a loaded asset template.
        /// </summary>
        private class AssetTemplateInfo
        {
            public Type Type = null;
            public SerializedAssetTemplateSettings Settings = null;
            private IAssetTemplate _instance = null;

            public AssetTemplateInfo(Type assetTemplateType)
                : this (assetTemplateType, new SerializedAssetTemplateSettings(assetTemplateType)) { }

            public AssetTemplateInfo(Type assetTemplateType, SerializedAssetTemplateSettings settings)
            {
                Type = assetTemplateType;
                Settings = settings;
            }

            /// <summary>
            /// Gets the asset template instance. Null if the instance can't be created.
            /// </summary>
            public IAssetTemplate Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        try
                        {
                            _instance = Activator.CreateInstance(Type) as IAssetTemplate;
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            Debug.LogError($"Failed to create an instance of an Asset Template of type {Type.FullName}. See previous log for more info.");
                            return null;
                        }

                        try
                        {
                            EditorJsonUtility.FromJsonOverwrite(Settings.Json, _instance);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            Debug.LogError($"Failed to apply the settings to the Asset Template of type {Type.FullName}. See previous log for more info.");
                        }
                    }
                    return _instance;
                }
            }

        }

        /// <summary>
        /// Stores the available asset template types (as keys) and their instance if it has been required (as values).
        /// </summary>
        private static Dictionary<Type, AssetTemplateInfo> s_assetTemplates = null;

        /// <inheritdoc cref="s_assetTemplates"/>
        private static Dictionary<Type, AssetTemplateInfo> AssetTemplates
        {
            get
            {
                if (s_assetTemplates == null)
                {
                    s_assetTemplates = new Dictionary<Type, AssetTemplateInfo>();

                    // Load settings
                    SerializedAssetTemplatesSettingsGroup settingsGroup = LoadSettings();
                    // For each available asset template type in the project
                    foreach (Type t in TypeCache.GetTypesDerivedFrom<IAssetTemplate>())
                    {
                        if (t.IsAbstract || t.IsInterface)
                            continue;

                        AssetTemplateInfo info = null;
                        // Use existing settings if applicable
                        foreach (SerializedAssetTemplateSettings settings in settingsGroup)
                        {
                            if (settings.Type == t.AssemblyQualifiedName)
                            {
                                info = new AssetTemplateInfo(t, settings);
                                break;
                            }
                        }
                        // Create new settings otherwise
                        if (info == null)
                            info = new AssetTemplateInfo(t);

                        s_assetTemplates.Add(t, info);
                    }
                }
                return s_assetTemplates;
            }
        }

        /// <summary>
        /// Gets the type of all the available asset template in the project.
        /// </summary>
        /// <returns>Returns the type of all the available asset template in the project.</returns>
        public static Type[] GetAvailableAssetTemplateTypes()
        {
            using (var scope = new ListPoolScope<Type>())
            {
                foreach (Type t in AssetTemplates.Keys)
                {
                    if (t.IsAbstract)
                        continue;

                    scope.List.Add(t);
                }

                scope.List.Sort((a, b) =>
                {
                    string aName = a.Name;
                    if (a.TryGetAttribute(out AssetTemplateAttribute templateAttrA) && !string.IsNullOrWhiteSpace(templateAttrA.Name))
                        aName = templateAttrA.Name;

                    string bName = b.Name;
                    if (b.TryGetAttribute(out AssetTemplateAttribute templateAttrB) && !string.IsNullOrWhiteSpace(templateAttrB.Name))
                        bName = templateAttrB.Name;

                    return aName.CompareTo(bName);
                });
                return scope.List.ToArray();
            }
        }

        /// <summary>
        /// Gets the instance of a specific asset template.
        /// </summary>
        /// <param name="assetTemplateType">The type of the asset template of which to get the instance.</param>
        /// <returns>Returns the found asset template instance.</returns>
        public static IAssetTemplate GetAssetTemplateInstance(Type assetTemplateType)
        {
            foreach ((Type t, AssetTemplateInfo info) in AssetTemplates)
            {
                if (t != assetTemplateType)
                    continue;

                return info.Instance;
            }

            Debug.LogError($"Failed to get asset template instance of type {assetTemplateType.FullName}: That type is abstract or doesn't inherit from {nameof(IAssetTemplate)}.");
            return null;
        }

        /// <summary>
        /// Checks if a given type of asset template is enabled.
        /// </summary>
        /// <param name="assetTemplateType">The type of the asset template to check.</param>
        public static bool IsEnabled(Type assetTemplateType)
        {
            foreach ((Type t, AssetTemplateInfo info) in AssetTemplates)
            {
                if (t != assetTemplateType)
                    continue;

                return info.Settings.Enabled;
            }
            return false;
        }

        /// <inheritdoc cref="IsEnabled(Type)"/>
        /// <typeparam name="T"><inheritdoc cref="IsEnabled(Type)" path="/param[@name='assetTemplateType']"/></typeparam>
        public static bool IsEnabled<T>()
            where T : IAssetTemplate
        {
            return IsEnabled(typeof(T));
        }

        /// <summary>
        /// Checks if a given asset template is enabled.
        /// </summary>
        /// <inheritdoc cref="IsEnabled(Type)"/>
        /// <param name="assetTemplate">The asset template to check.</param>
        public static bool IsEnabled(IAssetTemplate assetTemplate)
        {
            return assetTemplate != null && IsEnabled(assetTemplate.GetType());
        }

        /// <summary>
        /// Sets the <see cref="SerializedAssetTemplateSettings.Enabled"/> property for an asset template of the given type.
        /// </summary>
        /// <param name="assetTemplateType">The type of the asset template of which to change the state.</param>
        /// <param name="enabled">Is the asset template now enabled?</param>
        /// <returns>Returns true if the state of an asset template has been changed successfully.</returns>
        public static bool SetEnabled(Type assetTemplateType, bool enabled)
        {
            foreach ((Type t, AssetTemplateInfo info) in AssetTemplates)
            {
                if (t != assetTemplateType)
                    continue;

                // Cancel if the asset template already has the expected state
                if (info.Settings.Enabled == enabled)
                    return false;

                info.Settings.Enabled = enabled;
                SaveSettings();
                return true;
            }
            return false;
        }

        /// <inheritdoc cref="SetEnabled(Type, bool)"/>
        /// <typeparam name="T"><inheritdoc cref="SetEnabled(Type, bool)" path="/param[@name='assetTemplateType']"/></typeparam>
        public static bool SetEnabled<T>(bool enabled)
        {
            return SetEnabled(typeof(T), enabled);
        }

        /// <summary>
        /// Sets the <see cref="SerializedAssetTemplateSettings.Enabled"/> property for a given asset template.
        /// </summary>
        /// <inheritdoc cref="SetEnabled(Type, bool)"/>
        /// <param name="assetTemplate">The asset template to check.</param>
        public static bool SetEnabled(IAssetTemplate assetTemplate, bool enabled)
        {
            return assetTemplate != null && SetEnabled(assetTemplate.GetType(), enabled);
        }

        /// <summary>
        /// Packs the settings defined for the asset templates as JSON, and save it in the <see cref="AssetTemplatesConfig"/> asset.
        /// </summary>
        public static void SaveSettings()
        {
            SerializedAssetTemplatesSettingsGroup group = null;
            using (var list = new ListPoolScope<SerializedAssetTemplateSettings>())
            {
                foreach ((Type t, AssetTemplateInfo info) in AssetTemplates)
                {
                    info.Settings.Json = EditorJsonUtility.ToJson(info.Instance);
                    list.Add(info.Settings);
                }

                group = new SerializedAssetTemplatesSettingsGroup(list.ToArray());
            }

            AssetTemplatesConfig.AssetTemplatesSettings = EditorJsonUtility.ToJson(group);
        }

        /// <summary>
        /// Loads the asset templates settings from disk.
        /// </summary>
        /// <returns>Returns the loaded settings.</returns>
        private static SerializedAssetTemplatesSettingsGroup LoadSettings()
        {
            string json = AssetTemplatesConfig.AssetTemplatesSettings;
            if (string.IsNullOrWhiteSpace(json))
                return new SerializedAssetTemplatesSettingsGroup();

            SerializedAssetTemplatesSettingsGroup settingsGroup = new SerializedAssetTemplatesSettingsGroup();
            try
            {
                EditorJsonUtility.FromJsonOverwrite(json, settingsGroup);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Failed to load Asset Templates settings: see previous log for more info.");
            }
            return settingsGroup;
        }


    }

}
