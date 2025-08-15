using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using SideXP.Core;

using Object = UnityEngine.Object;

namespace SideXP.AssetTemplates.EditorOnly
{

    [System.Serializable]
    [AssetTemplate(
        "JSON (*.json)",
        "Generates a JSON file.",
        "File extension is *.json",
        "\"json\" alone, if another asset is selected, so the generated JSON file will contain its JSON representation if applicable (for scripts, it works with ScriptableObject implementations or serializable classes with a defaut constructor)",
        Order = 100
    )]
    public class JsonTemplate : IAssetTemplate
    {

        private const string JsonExtension = "json";

        [Tooltip("By default, the generated JSON is a long string on a single line. If enabled, the serialized data will be \"pretty printed\", making it easy to read.")]
        public bool PrettyPrint = true;

        /// <inheritdoc cref="IAssetTemplate.CanGenerateAsset(AssetInfo)"/>
        public bool CanGenerateAsset(AssetInfo info)
        {
            return info.Extension == JsonExtension || info.Name.ToLower() == JsonExtension;
        }

        /// <inheritdoc cref="IAssetTemplate.GenerateAsset(AssetInfo, ref AssetOutputInfo)"/>
        public bool GenerateAsset(AssetInfo info, ref AssetOutputInfo output)
        {
            // If another asset is selected when this template is triggered
            if (!string.IsNullOrWhiteSpace(info.ParentPath))
            {
                // If the selected asset is a script
                if (info.ParentType != null)
                {
                    // If the parent type can be instanced
                    if (!info.ParentType.IsAbstract && !info.ParentType.IsInterface && !info.ParentType.ContainsGenericParameters)
                    {
                        // If the parent type implements ScriptableObject
                        if (info.ParentType.Is<ScriptableObject>())
                        {
                            try
                            {
                                ScriptableObject inst = ScriptableObject.CreateInstance(info.ParentType);
                                output.Content = JsonUtility.ToJson(inst, PrettyPrint);
                            }
                            catch (Exception) { }
                        }
                        // Else, if the parent type is a serializable class or struct
                        else if (info.ParentType.GetCustomAttribute<SerializableAttribute>() != null)
                        {
                            try
                            {
                                object inst = Activator.CreateInstance(info.ParentType);
                                output.Content = EditorJsonUtility.ToJson(inst, PrettyPrint);
                            }
                            catch (Exception) { }
                        }
                    }
                }
                // Else, if the selected asset is not a script
                else
                {
                    try
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(info.ParentPath);
                        if (obj is ScriptableObject)
                            output.Content = JsonUtility.ToJson(obj, PrettyPrint);
                        else
                            output.Content = EditorJsonUtility.ToJson(obj, PrettyPrint);
                    }
                    catch (Exception) { }
                }
            }

            output.SetExtension(JsonExtension);
            if (string.IsNullOrWhiteSpace(output.Content))
            {
                output.Content = "{" +
                    "\n" +
                    "\n}";
            }

            return true;
        }

    }

}
