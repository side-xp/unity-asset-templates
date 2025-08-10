using System;
using System.Collections.Generic;
using System.Reflection;

using SideXP.Core;
using SideXP.Core.Reflection;
using SideXP.Core.EditorOnly;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace SideXP.AssetTemplates.EditorOnly
{

    [CustomEditor(typeof(AssetTemplatesConfig))]
    public class AssetTemplateConfigEditor : Editor
    {

        private const float TemplatFullTypeHeight = 12f;
        private const float EnabledHeaderFieldWidth = MoreGUI.WidthS;
        private const float SettingsIndent = 16f;

        /// <summary>
        /// Stores the public or private serialized fields (as values) of asset template types (as keys).
        /// </summary>
        private static Dictionary<Type, FieldInfo[]> s_exposedAssetTemplatesProperties = new Dictionary<Type, FieldInfo[]>();

        /// <summary>
        /// Reorderable list used to display the available asset template types, and make them editable.
        /// </summary>
        private ReorderableList _assetTemplatesReorderableList = null;

        /// <summary>
        /// The list of the asset templaate of which the settings have been expanded in the reorderable list view.
        /// </summary>
        private List<Type> _expandedAssetTemplateTypes = new List<Type>();

        /// <inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            AssetTemplatesReorderableList.DoLayoutList();
        }

        /// <summary>
        /// Gets the public or private serialized fields declared from a given type.
        /// </summary>
        /// <param name="type">The type of which to get the exposed fields.</param>
        /// <returns>Returns the found public or private serialized fields.</returns>
        private static FieldInfo[] GetExposedFields(Type type)
        {
            if (!s_exposedAssetTemplatesProperties.TryGetValue(type, out FieldInfo[] fields))
            {
                fields = ReflectionUtility.GetExposedFields(type, true);
                s_exposedAssetTemplatesProperties.Add(type, fields);
            }

            return fields;
        }

        /// <inheritdoc cref="_assetTemplatesReorderableList"/>
        private ReorderableList AssetTemplatesReorderableList
        {
            get
            {
                if (_assetTemplatesReorderableList == null)
                {
                    _assetTemplatesReorderableList = new ReorderableList(AssetTemplatesUtility.GetAvailableAssetTemplateTypes(), typeof(Type), false, true, false, false);

                    // Header
                    _assetTemplatesReorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Asset Templates", EditorStyles.boldLabel);

                    // Element height
                    _assetTemplatesReorderableList.elementHeightCallback = index =>
                    {
                        Type assetTemplateType = AssetTemplatesUtility.GetAvailableAssetTemplateTypes()[index];
                        // Height for a single line, top and bottom padding
                        float height = EditorGUIUtility.singleLineHeight + MoreGUI.VMargin * 2;

                        // Stop if the element is not expanded (display a sinle line)
                        if (!_expandedAssetTemplateTypes.Contains(assetTemplateType))
                            return height;

                        // Add height to display the type's full name
                        height += TemplatFullTypeHeight;

                        int fieldsCount = GetExposedFields(assetTemplateType).Length;
                        // Add height for each settings field to display
                        if (fieldsCount > 0)
                            height += EditorGUIUtility.singleLineHeight * fieldsCount + MoreGUI.VMargin * (fieldsCount + 1);

                        return height;
                    };

                    // Draw element
                    _assetTemplatesReorderableList.drawElementCallback = (position, index, isActive, isFocused) =>
                    {
                        Type templateType = AssetTemplatesUtility.GetAvailableAssetTemplateTypes()[index];
                        IAssetTemplate templateInstance = AssetTemplatesUtility.GetAssetTemplateInstance(templateType);
                        AssetTemplateAttribute templateAttribute = templateType.GetCustomAttribute<AssetTemplateAttribute>(true);

                        position.y += MoreGUI.VMargin;
                        Rect rect = new Rect(position);
                        rect.height = EditorGUIUtility.singleLineHeight;

                        rect.width -= EnabledHeaderFieldWidth + MoreGUI.HMargin;

                        bool isExpanded = _expandedAssetTemplateTypes.Contains(templateType);
                        // Header
                        {
                            // Foldout field
                            GUIContent label = templateAttribute != null && !string.IsNullOrWhiteSpace(templateAttribute.Name)
                                ? new GUIContent(templateAttribute.Name, templateAttribute.Description)
                                : new GUIContent(templateType.Name);

                            // Add triggers to the tooltip
                            if (templateAttribute != null && templateAttribute.Triggers.Length > 0)
                            {
                                if (!string.IsNullOrWhiteSpace(label.tooltip))
                                    label.tooltip += "\n\n";
                                label.tooltip += "Triggers:";

                                foreach (string trigger in templateAttribute.Triggers)
                                {
                                    if (!string.IsNullOrWhiteSpace(trigger))
                                        label.tooltip += $"\n    - {trigger}";
                                }
                            }

                            isExpanded = EditorGUI.Foldout(rect, isExpanded, label, true, EditorStyles.foldout.RichText(true));

                            // Update the expanded elements list
                            if (isExpanded && !_expandedAssetTemplateTypes.Contains(templateType))
                                _expandedAssetTemplateTypes.Add(templateType);
                            else if (!isExpanded)
                                _expandedAssetTemplateTypes.Remove(templateType);

                            // Enabled header field
                            rect.x += rect.width + MoreGUI.HMargin;
                            rect.width = EnabledHeaderFieldWidth;
                            EditorGUI.BeginChangeCheck();
                            bool enabled = EditorGUI.ToggleLeft
                            (
                                rect,
                                new GUIContent("Enabled", "If disabled, this asset template won't be triggered."),
                                AssetTemplatesUtility.IsEnabled(templateType)
                            );
                            if (EditorGUI.EndChangeCheck())
                                AssetTemplatesUtility.SetEnabled(templateType, enabled);
                        }

                        if (!isExpanded)
                            return;

                        // Offset position (indent expanded settings)
                        position.x += SettingsIndent;
                        position.width -= SettingsIndent;

                        rect.x = position.x;
                        rect.width = position.width;
                        rect.y += rect.height;
                        rect.height = TemplatFullTypeHeight;
                        EditorGUI.LabelField(rect, $"<i><size=9>{templateType.FullName}</size></i>", EditorStyles.label.RichText(true));

                        rect.height = EditorGUIUtility.singleLineHeight;
                        foreach (FieldInfo fieldInfo in GetExposedFields(templateType))
                        {
                            rect.y += MoreGUI.VMargin + rect.height;
                            if (MoreEditorGUI.PropertyField(templateInstance, rect, fieldInfo, true))
                                AssetTemplatesUtility.SaveSettings();
                        }
                    };
                }
                return _assetTemplatesReorderableList;
            }
        }

    }

}
