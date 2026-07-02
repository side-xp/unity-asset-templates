using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class CustomEditorTemplateTests
    {

        private static AssetInfo MakeInfo(string name, Type parentType = null)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, parentType);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_EditorPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new CustomEditorTemplate().CanGenerateAsset(MakeInfo("editor SxpTestFoo")));
        }

        [Test]
        public void CanGenerateAsset_EditorSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new CustomEditorTemplate().CanGenerateAsset(MakeInfo("SxpTestFooEditor")));
        }

        [Test]
        public void CanGenerateAsset_EditorShortcut_ReturnsTrue()
        {
            Assert.IsTrue(new CustomEditorTemplate().CanGenerateAsset(MakeInfo("editor")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new CustomEditorTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_Prefix_GeneratesEditorForNamedType()
        {
            var output = new AssetOutputInfo();
            bool success = new CustomEditorTemplate().GenerateAsset(MakeInfo("editor SxpTestFoo"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("class SxpTestFooEditor", output.Content);
            StringAssert.Contains("Editor", output.Content);
            StringAssert.Contains("CustomEditor", output.Content);
            StringAssert.Contains("typeof(SxpTestFoo)", output.Content);
            Assert.IsTrue(output.IsEditorContent);
        }

        [Test]
        public void GenerateAsset_ShortcutWithSelectedObject_NamesEditorAfterParent()
        {
            var output = new AssetOutputInfo();
            new CustomEditorTemplate().GenerateAsset(MakeInfo("editor", typeof(SampleScriptableObject)), ref output);

            StringAssert.Contains("class SampleScriptableObjectEditor", output.Content);
            StringAssert.Contains("typeof(SampleScriptableObject)", output.Content);
            Assert.IsTrue(output.IsEditorContent);
        }

        [Test]
        public void GenerateAsset_ShortcutWithoutSelection_FailsGracefully()
        {
            LogAssert.Expect(LogType.Error, new Regex("shortcut"));

            var output = new AssetOutputInfo();
            bool success = new CustomEditorTemplate().GenerateAsset(MakeInfo("editor"), ref output);

            Assert.IsFalse(success);
        }

        #endregion

    }

}
