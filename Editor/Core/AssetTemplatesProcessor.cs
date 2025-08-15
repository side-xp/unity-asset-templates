using SideXP.Core;
using SideXP.Core.EditorOnly;
using SideXP.Core.Reflection;

using System;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

using Object = UnityEngine.Object;

namespace SideXP.AssetTemplates.EditorOnly
{

    /// <summary>
    /// Custom asset processor to trigger asset generation from templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The expected workflow is as follows:<br/>
    /// - The user clicks on the menu at Assets > Create > Create Asset From Template<br/>
    /// - The <see cref="CreateAssetFromTemplate"/> function is called, displaying a text field to name the asset<br/>
    /// - The user types the name of the asset<br/>
    /// - The "name edit" command is handled by an instance of <see cref="EndNameAssetTemplate"/> class, which will start iterating through
    /// the exising asset template classes, and trigger the appropriate one if any
    /// </para>
    /// </remarks>
    public class AssetTemplatesProcessor : AssetPostprocessor
    {

        // Menu
        private const string MenuName = "Assets/Create/Create Asset From Template";
        private const int MenuPriority = -220;
        private const string DefaultFileName = "NewAsset";

        // Defaults
        private const string DefaultExtension = "cs";
        private const string DefaultScriptTemplateFileName =
#if UNITY_6000_0_OR_NEWER
            "1-Scripting__MonoBehaviour Script-NewMonoBehaviourScript.cs.txt";
#else
            "81-C# Script-NewBehaviourScript.cs.txt";
#endif

        // Reflection

        /// <summary>
        /// The name of the function to invoke through reflection from <see cref="ProjectWindowUtil"/> to create a new script from a
        /// template.
        /// </summary>
        private const string CreateScriptFromTemplateMethodName = "CreateScriptAssetFromTemplate";

        // Paths
        private const string EditorFolder = "Editor";
        private const string TargetScriptTemplatesDirectory = "Data/Resources/ScriptTemplates";
        private static readonly string ScriptTemplatesDirectory = null;

        // Cache
        private static Object s_selectedObjectBeforeAction = null;

        /// <summary>
        /// Represents the action to perform after naming a file to create using the template creation menu.
        /// </summary>
        private class EndNameAssetTemplate : EndNameEditAction
        {

            /// <inheritdoc cref="EndNameEditAction.Action(int, string, string)"/>
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string name = Path.GetFileNameWithoutExtension(pathName);
                string extension = Path.GetExtension(pathName);

                // Fix extension: use default extension if not defined, or just remove the "." character
                extension = string.IsNullOrWhiteSpace(extension)
                    ? DefaultExtension
                    : extension.Substring(1);

                // Fix path: ensure the extension is defined and the target path is unique
                pathName = Path.GetDirectoryName(pathName);
                pathName += $"/{name}.{extension}";
                pathName = AssetDatabase.GenerateUniqueAssetPath(pathName);

                // Process namespace
                string namespaceStr = ScriptUtility.GetNamespaceFromPath(pathName);

                // Process inheritance
                string inheritFromPath = null;
                Type baseType = null;
                if (s_selectedObjectBeforeAction != null)
                {
                    inheritFromPath = AssetDatabase.GetAssetPath(s_selectedObjectBeforeAction);
                    ScriptUtility.TryGetDeclaredType(s_selectedObjectBeforeAction as TextAsset, out baseType);
                }

                AssetInfo info = new AssetInfo(pathName, namespaceStr, inheritFromPath, baseType);
                AssetOutputInfo output = new AssetOutputInfo { Path = info.Path };

                // Try to generate the content from the appropriate asset template
                Type triggeredAssetTemplateType = null;
                foreach (Type assetTemplateType in AssetTemplatesUtility.GetAvailableAssetTemplateTypes(true))
                {
                    IAssetTemplate assetTemplate = AssetTemplatesUtility.GetAssetTemplateInstance(assetTemplateType);
                    // Skip if the asset template is not valid or disabled
                    if (assetTemplate == null || !AssetTemplatesUtility.IsEnabled(assetTemplate))
                        continue;

                    // Skip if the asset template can't generate the asset
                    if (!assetTemplate.CanGenerateAsset(info))
                        continue;

                    triggeredAssetTemplateType = assetTemplateType;
                    if (!assetTemplate.GenerateAsset(info, ref output))
                        Debug.LogError($"Failed to generate the asset from template \"{assetTemplateType.FullName}\"");

                    break;
                }

                // Cancel if the output path is not valid
                if (triggeredAssetTemplateType != null && string.IsNullOrWhiteSpace(output.Path))
                {
                    Debug.LogError($"Failed to generate a file from asset templates: invalid path provided by the asset template of type {triggeredAssetTemplateType.FullName}");
                    return;
                }

                // If the generated content is supposed to be editor-only
                if (output.IsEditorContent)
                {
                    bool isInEditorFolder = false;
                    string tmpPath = output.Path;
                    // Try to find an "Editor" folder the asset is placed in
                    while (!string.IsNullOrWhiteSpace(tmpPath))
                    {
                        tmpPath = Path.GetDirectoryName(tmpPath);
                        if (tmpPath.EndsWith("Editor"))
                        {
                            isInEditorFolder = true;
                            break;
                        }
                    }

                    if (!isInEditorFolder)
                    {
                        string dir = Path.GetDirectoryName(output.Path);
                        try
                        {
                            Directory.CreateDirectory($"{dir}/{EditorFolder}");
                            output.Path = $"{dir}/{EditorFolder}/{Path.GetFileName(output.Path)}";
                            //if (output.Path.IsProjectPath())
                            //    AssetDatabase.Refresh();
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning($"Failed to create an Editor/ folder at {dir}. Since the generated asset should be editor-only, you must place it in an Editor/ folder to exclude it from build.");
                        }
                    }
                }

                // Create the asset with the given content
                // Note that empty scripts are not allowed, and the default script tample will be used instead
                if (output.Content != null || !output.Path.EndsWith("cs"))
                {
                    if (output.Content == null)
                        output.Content = string.Empty;

                    /**
                     * @note
                     * Even if the name doesn't suggest it, this function can be used to create assets that are not scripts but still
                     * contain text (so it's applicable for C#, JSON, regular *.asset, ...). Internally, it just write the given content
                     * into a file using System.IO.File.WriteAllText(), and reimport the asset.
                     */
                    ProjectWindowUtil.CreateScriptAssetWithContent(output.Path, output.Content);
                }
                else
                {
                    MethodInfo createScriptFromTemplateMethod = typeof(ProjectWindowUtil).GetMethod(CreateScriptFromTemplateMethodName, ReflectionUtility.StaticFlags);
                    // It's expected that the method takes two string parameters: the path to the file to create, and the path to the template file
                    try
                    {
                        createScriptFromTemplateMethod.Invoke(null, new object[]
                        {
                            output.Path,
                            $"{ScriptTemplatesDirectory}/{DefaultScriptTemplateFileName}"
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("Failed to create a script file from the default template. See previous logs for more information.");
                    }
                }

                s_selectedObjectBeforeAction = null;
            }

        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static AssetTemplatesProcessor()
        {
            ScriptTemplatesDirectory = PathUtility.ToPath($"{Path.GetDirectoryName(EditorApplication.applicationPath)}/{TargetScriptTemplatesDirectory}");
        }

        [MenuItem(MenuName, false, MenuPriority)]
        private static void CreateAssetFromTemplate()
        {
            s_selectedObjectBeforeAction = Selection.activeObject;
            var action = ScriptableObject.CreateInstance<EndNameAssetTemplate>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, DefaultFileName, null, null, true);
        }

    }

}
