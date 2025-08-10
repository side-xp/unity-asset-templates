using System.CodeDom;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Editor Window",
        "Generates a script for a custom editor window.",
        "\"window-\" prefix (followed by space or uppercase letter)",
        "\"-EditorWindow\" suffix"
    )]
    public class EditorWindowTemplate : IAssetTemplate
    {

        private const string EditorWindowSuffix = "EditorWindow";
        private const string WindowTitleConst = "WindowTitle";
        private const string MenuItemConst = "MenuItem";
        private const string WindowVar = "window";

        /// <summary>
        /// The pattern for matching prefix or suffix.
        /// </summary>
        private static PrefixSuffixPattern s_pattern = null;

        [Tooltip("The base string to use for the [MenuItem] attribute meant to add a menu in the toolbar to open the window." +
            "\nIt must not end with a slash character.")]
        public string WindowMenuBase = "Tools";

        [Tooltip("By default, a method OnGUI() is added to the generated script." +
            "\nIf checked, adds a method CreateGUI() instead.")]
        public bool UseVisualElement = false;

        static EditorWindowTemplate()
        {
            s_pattern = new PrefixSuffixPattern();
            s_pattern.AddPrefix("window");
            s_pattern.AddSuffix(EditorWindowSuffix);
        }

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return s_pattern.Match(info.Name);
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            // Cancel if the class name can't be processed
            if (!s_pattern.Match(info.Name, out string className, out string matchingPart, out bool isPrefix))
            {
                Debug.LogError("Failed to generate class (or struct) from the \"Editor Window\" asset template: invalid class name");
                return false;
            }

            ScriptGenerator scriptGenerator = new ScriptGenerator(info);
            scriptGenerator.InheritFrom(typeof(EditorWindow));

            scriptGenerator.ImportsNamespace.Imports.Add(new CodeNamespaceImport(nameof(UnityEngine)));
            scriptGenerator.ImportsNamespace.Imports.Add(new CodeNamespaceImport(nameof(UnityEditor)));

            // Use the string without name parts as title
            string title = className;
            title = ObjectNames.NicifyVariableName(title);

            // Add "-EditorWindow" suffix for class name
            className += EditorWindowSuffix;
            info.Rename(className);
            scriptGenerator.MainClass.Name = info.Name;

            // WindowTitle constant
            scriptGenerator.MainClass.Members.Add(new CodeMemberField(typeof(string), WindowTitleConst)
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Const,
                InitExpression = new CodePrimitiveExpression(title)
            });

            // MenuItem constant
            scriptGenerator.MainClass.Members.Add(new CodeMemberField(typeof(string), MenuItemConst)
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Const,
                InitExpression = new CodePrimitiveExpression($"{WindowMenuBase}/{title}")
            });

            // Create Open() method
            {
                var openMethod = new CodeMemberMethod()
                {
                    Name = "Open",
                    Attributes = MemberAttributes.Public | MemberAttributes.Static,
                    ReturnType = new CodeTypeReference(scriptGenerator.MainClass.Name)
                };

                // Add [MenuItem] attribute
                openMethod.CustomAttributes.Add(new CodeAttributeDeclaration(scriptGenerator.GetTypeReference<MenuItem>(), new CodeAttributeArgument[]
                {
                    new CodeAttributeArgument(new CodeFieldReferenceExpression() { FieldName = MenuItemConst })
                }));

                // Add "GetWindow()" call
                openMethod.Statements.Add(new CodeVariableDeclarationStatement(scriptGenerator.MainClass.Name, WindowVar) { InitExpression = MakeGetWindowInitExpression() });

                // Add "window.Show()" call
                openMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeFieldReferenceExpression() { FieldName = WindowVar },
                            nameof(EditorWindow.Show)
                        )
                    ));

                // Add "return window" statement
                openMethod.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression() { FieldName = WindowVar }));

                // Register method
                scriptGenerator.MainClass.Members.Add(openMethod);
            }

            // Create GUI render method
            CodeMemberMethod guiRenderMethod = null;
            if (UseVisualElement)
            {
                guiRenderMethod = new CodeMemberMethod()
                {
                    Name = "CreateGUI",
                    Attributes = MemberAttributes.Private
                };
                // Add new label element statement
                var visualElementAddInvoke = new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression() { FieldName = nameof(EditorWindow.rootVisualElement) },
                    nameof(VisualElement.Add));
                visualElementAddInvoke.Parameters.Add(
                    new CodeObjectCreateExpression(
                        scriptGenerator.GetTypeReference<Label>(),
                        new CodePrimitiveExpression(title)));
                guiRenderMethod.Statements.Add(visualElementAddInvoke);
            }
            else
            {
                guiRenderMethod = new CodeMemberMethod()
                {
                    Name = "OnGUI",
                    Attributes = MemberAttributes.Private
                };
            }
            scriptGenerator.MainClass.Members.Add(guiRenderMethod);

            output.Path = info.Path;
            output.Content = scriptGenerator.Generate();

            return true;

            // Generates "<ClassName> window = GetWindow<ClassName>(false, WindowTitle, true)"
            CodeMethodInvokeExpression MakeGetWindowInitExpression()
            {
                var getWindowRef = new CodeMethodReferenceExpression() { MethodName = nameof(EditorWindow.GetWindow) };
                getWindowRef.TypeArguments.Add(scriptGenerator.MainClass.Name);

                var getWindowInvoke = new CodeMethodInvokeExpression(getWindowRef, new CodeExpression[]
                {
                    new CodePrimitiveExpression(false),
                    new CodeFieldReferenceExpression() { FieldName = WindowTitleConst },
                    new CodePrimitiveExpression(true)
                });

                return getWindowInvoke;
            }
        }

    }

}
