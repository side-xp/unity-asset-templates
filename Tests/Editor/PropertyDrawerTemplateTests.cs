using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class PropertyDrawerTemplateTests
    {

        private static AssetInfo MakeInfo(string name, Type parentType = null)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, parentType);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_DrawerPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new PropertyDrawerTemplate().CanGenerateAsset(MakeInfo("drawer SxpTestFoo")));
        }

        [Test]
        public void CanGenerateAsset_PropertyDrawerSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new PropertyDrawerTemplate().CanGenerateAsset(MakeInfo("SxpTestFooPropertyDrawer")));
        }

        [Test]
        public void CanGenerateAsset_DrawerShortcut_ReturnsTrue()
        {
            Assert.IsTrue(new PropertyDrawerTemplate().CanGenerateAsset(MakeInfo("drawer")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new PropertyDrawerTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_Prefix_GeneratesPropertyDrawerWithOnGUI()
        {
            var output = new AssetOutputInfo();
            bool success = new PropertyDrawerTemplate().GenerateAsset(MakeInfo("drawer SxpTestFoo"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("class SxpTestFooPropertyDrawer", output.Content);
            StringAssert.Contains("PropertyDrawer", output.Content);
            StringAssert.Contains("CustomPropertyDrawer", output.Content);
            StringAssert.Contains("OnGUI", output.Content);
            StringAssert.Contains("PropertyField", output.Content);
            Assert.IsTrue(output.IsEditorContent);
        }

        [Test]
        public void GenerateAsset_ShortcutWithSelectedObject_NamesDrawerAfterParent()
        {
            var output = new AssetOutputInfo();
            new PropertyDrawerTemplate().GenerateAsset(MakeInfo("drawer", typeof(SampleScriptableObject)), ref output);

            StringAssert.Contains("class SampleScriptableObjectPropertyDrawer", output.Content);
            StringAssert.Contains("typeof(SampleScriptableObject)", output.Content);
            Assert.IsTrue(output.IsEditorContent);
        }

        [Test]
        public void GenerateAsset_ShortcutWithoutSelection_FailsGracefully()
        {
            LogAssert.Expect(LogType.Error, new Regex("shortcut"));

            var output = new AssetOutputInfo();
            bool success = new PropertyDrawerTemplate().GenerateAsset(MakeInfo("drawer"), ref output);

            Assert.IsFalse(success);
        }

        #endregion

    }

}
