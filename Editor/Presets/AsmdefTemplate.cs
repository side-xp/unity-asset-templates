using UnityEngine;

using SideXP.Core;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "Assembly Definition (*.asmdef)",
        "Generates an Assembly Definition file with basic configuration.",
        "File name ends with \"[AssemblyName].asmdef\"",
        "If the file name ends with \"[AssemblyName].Editor.asmdef\", a special config is used to make the Assembly editor-only"
    )]
    public class AsmdefTemplate : IAssetTemplate
    {

#pragma warning disable IDE1006 // Naming Styles
        [System.Serializable]
        private struct AsmdefInfo
        {
            public string name;
            public bool autoReferenced;
            public string rootNamespace;
            public string[] includePlatforms;
        }
#pragma warning restore IDE1006 // Naming Styles

        private const string AsmdefExtension = "asmdef";
        private const string EditorPart = "Editor";
        private const string EditorNamespaceReplacement = "EditorOnly";
        private const string UnityPart = "Unity";

        [Tooltip("If checked, the \"Root Namespace\" property of the assembly definition is set, based on its name." +
            "\nNote that \"." + EditorPart + "\" will become \"." + EditorNamespaceReplacement + "\". Also, if the name includes \"." + UnityPart + "\", that part will be removed." +
            "\nExamples:" +
            "\n- SideXP.Unity.Game -> SideXP.Game" +
            "\n- SideXP.Unity.Game.Editor -> SideXP.Game.EditorOnly")]
        public bool SetupRootNamespace = true;

        [Tooltip("If checked, enables the \"Auto Referenced\" option of the assembly definition file.")]
        public bool AutoReferenced = true;

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return info.Extension == AsmdefExtension;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            string[] nameParts = info.Name.Split('.');
            string rootNamespace = string.Empty;
            // Check if the assembly is meant to be editor-only
            bool isEditor = nameParts[nameParts.Length - 1] == EditorPart;

            // Setup root namespace if applicable
            if (SetupRootNamespace)
            {
                using (var namespaceScope = new ListPoolScope<string>())
                {
                    namespaceScope.List.AddRange(nameParts);

                    // If that part is "Editor", replace it by "EditorOnly"
                    if (isEditor)
                        namespaceScope.List[namespaceScope.List.Count - 1] = EditorNamespaceReplacement;

                    // Remove "Unity" parts
                    for (int i = namespaceScope.List.Count - 1; i >= 0; i--)
                    {
                        if (namespaceScope.List[i] == UnityPart)
                        {
                            namespaceScope.List.RemoveAt(i);
                            continue;
                        }
                    }

                    rootNamespace = string.Join(".", namespaceScope.List);
                }
            }

            // Make asseùbly definition info
            AsmdefInfo asmdefInto = new AsmdefInfo()
            {
                name = info.Name,
                autoReferenced = AutoReferenced,
                includePlatforms = isEditor ? new string[] { EditorPart } : new string[0],
                rootNamespace = rootNamespace,
            };

            output.Content = JsonUtility.ToJson(asmdefInto, true);
            return true;
        }

    }

}
