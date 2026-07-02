using System;

using NUnit.Framework;

using UnityEngine;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    // A ScriptableObject with a serialized field, used to check the "serialize the selected asset" behavior.
    public class SampleJsonScriptableObject : ScriptableObject
    {
        public int SampleValue = 42;
    }

    public class JsonTemplateTests
    {

        private static AssetInfo MakeInfo(string fileName, string parentPath = null, Type parentType = null)
        {
            return new AssetInfo($"Assets/{fileName}", null, parentPath, parentType);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_JsonExtension_ReturnsTrue()
        {
            Assert.IsTrue(new JsonTemplate().CanGenerateAsset(MakeInfo("SxpTestData.json")));
        }

        [Test]
        public void CanGenerateAsset_JsonShortcutName_ReturnsTrue()
        {
            Assert.IsTrue(new JsonTemplate().CanGenerateAsset(MakeInfo("json")));
        }

        [Test]
        public void CanGenerateAsset_OtherExtension_ReturnsFalse()
        {
            Assert.IsFalse(new JsonTemplate().CanGenerateAsset(MakeInfo("SxpTestData.cs")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_NoParent_ProducesEmptyObjectAndForcesJsonExtension()
        {
            // Start from a different extension to confirm SetExtension forces ".json".
            var output = new AssetOutputInfo { Path = "Assets/SxpTestData.txt" };
            bool success = new JsonTemplate().GenerateAsset(MakeInfo("SxpTestData.json"), ref output);

            Assert.IsTrue(success);
            StringAssert.EndsWith(".json", output.Path);
            StringAssert.Contains("{", output.Content);
        }

        [Test]
        public void GenerateAsset_ScriptableObjectSelected_SerializesItsFields()
        {
            var output = new AssetOutputInfo { Path = "Assets/SxpTestData.json" };
            var info = MakeInfo("SxpTestData.json", "Assets/SampleJsonScriptableObject.cs", typeof(SampleJsonScriptableObject));
            new JsonTemplate().GenerateAsset(info, ref output);

            StringAssert.Contains("SampleValue", output.Content);
        }

        #endregion

    }

}
