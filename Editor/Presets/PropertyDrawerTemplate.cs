using System;
using System.CodeDom;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using SideXP.Core;

using Object = UnityEngine.Object;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Property Drawer",
        "Generate a script for a custom property drawer.",
        "\"drawer-\" prefix (followed by space or uppercase letter)",
        "\"-PropertyDrawer\" suffix",
        "\"drawer\" alone, if another script is currently selected, so the name of the class will be [selected parent]PropertyDrawer"
    )]
    public class PropertyDrawerTemplate : IAssetTemplate
    {

        private const string PropertyDrawerSuffix = "PropertyDrawer";
        private const string DrawerShortcut = "drawer";
        private const string PositionParam = "position";
        private const string PropertyParam = "property";
        private const string LabelParam = "label";

        /// <summary>
        /// The pattern for matching prefix or suffix.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = null;

        public PropertyDrawerTemplate()
        {
            s_pattern = new PrefixSuffixPattern();
            s_pattern.AddPrefix(DrawerShortcut);
            s_pattern.AddSuffix(PropertyDrawerSuffix);
        }

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return s_pattern.Match(info.Name) || info.Name.ToLower() == DrawerShortcut;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            string className = null;
            // If the name used is just "drawer" shortcut
            if (info.Name.ToLower() == DrawerShortcut)
            {
                // Cancel if there's no selected script to decorate
                if (info.ParentType == null || !CanBeDecorated(info.ParentType))
                {
                    Debug.LogError($"Failed to generate script from the \"Property Drawer\" asset template: you must select a script that declares a {nameof(UnityEngine)}.{nameof(Object)} or a custom attribute class if you want to use the \"drawer\" shortcut.");
                    return false;
                }
            }
            // Cancel if the class name can't be processed
            else if (!s_pattern.Match(info.Name, out className, out string matchingPart, out _))
            {
                Debug.LogError("Failed to generate class (or struct) from the \"Property Drawer\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.InheritFrom(typeof(PropertyDrawer));

            scriptGenerator.ImportsNamespace.Imports.Add(new CodeNamespaceImport(nameof(UnityEngine)));
            scriptGenerator.ImportsNamespace.Imports.Add(new CodeNamespaceImport(nameof(UnityEditor)));

            string decoratedClassName = CanBeDecorated(info.ParentType)
                ? info.ParentType.Name
                : className;

            // Fix class name if needed (at this point, if the "drawer" shortcut has been used, this value is still empty)
            if (string.IsNullOrWhiteSpace(className))
                className = decoratedClassName;

            // [CustomPropertyDrawer] attribute
            if (!string.IsNullOrWhiteSpace(decoratedClassName))
            {
                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<CustomPropertyDrawer>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodeTypeOfExpression(new CodeTypeReference(decoratedClassName)))
                }));
            }
            // Use a code snippet if the decorated type is unknown
            else
            {
                scriptGenerator.MainClass.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<CustomPropertyDrawer>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodeSnippetExpression($"typeof({className})"))
                }));
            }

            // Add OnGUI() override
            {
                var onGUIOverride = new CodeMemberMethod()
                {
                    Name = "OnGUI",
                    Attributes = MemberAttributes.Override | MemberAttributes.Public
                };
                // Add comment
                onGUIOverride.Comments.Add(new CodeCommentStatement($"<inheritdoc cref=\"{nameof(PropertyDrawer)}.OnGUI({nameof(Rect)}, {nameof(SerializedProperty)}, {nameof(GUIContent)})\"/>", true));
                // Add parameters
                onGUIOverride.Parameters.Add(new CodeParameterDeclarationExpression(scriptGenerator.GetTypeReference<Rect>(true), PositionParam));
                onGUIOverride.Parameters.Add(new CodeParameterDeclarationExpression(scriptGenerator.GetTypeReference<SerializedProperty>(true), PropertyParam));
                onGUIOverride.Parameters.Add(new CodeParameterDeclarationExpression(scriptGenerator.GetTypeReference<GUIContent>(true), LabelParam));

                // Add "if multiple values" statement
                var multipleValuesConditions =
                    new CodeConditionStatement(
                        new CodePropertyReferenceExpression(
                            new CodeArgumentReferenceExpression(PropertyParam),
                            nameof(SerializedProperty.hasMultipleDifferentValues)));
                // Add "using MixedValueScope" block
                string indent = " ".Repeat(16);
                multipleValuesConditions.TrueStatements.Add(new CodeSnippetStatement($"{indent}using (new {nameof(EditorGUI)}.{nameof(EditorGUI.MixedValueScope)}(true))"));
                multipleValuesConditions.TrueStatements.Add(new CodeSnippetStatement($"{indent}{{"));
                multipleValuesConditions.TrueStatements.Add(new CodeSnippetStatement($"{indent}{" ".Repeat(4)}{nameof(EditorGUI)}.{nameof(EditorGUI.PropertyField)}({PositionParam}, {PropertyParam}, {LabelParam});"));
                multipleValuesConditions.TrueStatements.Add(new CodeSnippetStatement($"{indent}}}"));
                multipleValuesConditions.TrueStatements.Add(new CodeMethodReturnStatement());
                onGUIOverride.Statements.Add(multipleValuesConditions);

                // Invoke "EditorGUI.PropertyField()" out of the MixedValueScope if there's no multiple selected values
                onGUIOverride.Statements.Add(new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(scriptGenerator.GetTypeReference<EditorGUI>(true)),
                    nameof(EditorGUI.PropertyField),
                    new CodeExpression[]
                    {
                        new CodeArgumentReferenceExpression("position"),
                        new CodeArgumentReferenceExpression("property"),
                        new CodeArgumentReferenceExpression("label")
                    }));

                scriptGenerator.MainClass.Members.Add(onGUIOverride);
            }

            info.Rename(className + PropertyDrawerSuffix);
            scriptGenerator.MainClass.Name = info.Name;
            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;
        }

        private bool CanBeDecorated(Type type)
        {
            return
                (type != null
                && (type.Is<Object>()
                    || type.Is<PropertyAttribute>()
                    || type.GetCustomAttribute<System.SerializableAttribute>() != null));
        }

    }

}
