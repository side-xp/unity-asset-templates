using System;

using NUnit.Framework;

using UnityEngine;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    // A ScriptableObject-derived type used to exercise "inherit from the selected ScriptableObject" behavior.
    public class SampleScriptableObject : ScriptableObject { }

    public class ScriptableObjectTemplateTests
    {

        private static AssetInfo MakeInfo(string name, Type parentType = null)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, parentType);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_ScriptablePrefix_ReturnsTrue()
        {
            Assert.IsTrue(new ScriptableObjectTemplate().CanGenerateAsset(MakeInfo("scriptable SxpTestData")));
        }

        [Test]
        public void CanGenerateAsset_AssetSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new ScriptableObjectTemplate().CanGenerateAsset(MakeInfo("SxpTestDataAsset")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new ScriptableObjectTemplate().CanGenerateAsset(MakeInfo("SxpTestData")));
        }

        #endregion


        #region Inheritance

        [Test]
        public void GenerateAsset_NoParent_InheritsScriptableObject()
        {
            var output = new AssetOutputInfo();
            bool success = new ScriptableObjectTemplate().GenerateAsset(MakeInfo("scriptable SxpTestData"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("class SxpTestData", output.Content);
            StringAssert.Contains("ScriptableObject", output.Content);
        }

        [Test]
        public void GenerateAsset_ParentScriptableObjectSelected_InheritsIt()
        {
            var output = new AssetOutputInfo();
            new ScriptableObjectTemplate().GenerateAsset(MakeInfo("scriptable SxpTestData", typeof(SampleScriptableObject)), ref output);

            StringAssert.Contains("SampleScriptableObject", output.Content);
        }

        #endregion


        #region Attributes

        [Test]
        public void GenerateAsset_Always_AddsCreateAssetMenu()
        {
            var output = new AssetOutputInfo();
            new ScriptableObjectTemplate().GenerateAsset(MakeInfo("scriptable SxpTestData"), ref output);

            StringAssert.Contains("CreateAssetMenu", output.Content);
        }

        [Test]
        public void GenerateAsset_NoBaseHelpURL_OmitsHelpURL()
        {
            var output = new AssetOutputInfo();
            new ScriptableObjectTemplate().GenerateAsset(MakeInfo("scriptable SxpTestData"), ref output);

            StringAssert.DoesNotContain("HelpURL", output.Content);
        }

        [Test]
        public void GenerateAsset_BaseHelpURLSet_AddsHelpURLWithThatUrl()
        {
            var output = new AssetOutputInfo();
            var template = new ScriptableObjectTemplate { BaseHelpURL = "https://example.com/help" };
            template.GenerateAsset(MakeInfo("scriptable SxpTestData"), ref output);

            StringAssert.Contains("HelpURL", output.Content);
            StringAssert.Contains("https://example.com/help", output.Content);
        }

        [Test]
        public void GenerateAsset_BaseCreateAssetMenuSet_PrefixesMenuPath()
        {
            var output = new AssetOutputInfo();
            var template = new ScriptableObjectTemplate { BaseCreateAssetMenu = "SxpTest" };
            template.GenerateAsset(MakeInfo("scriptable SxpTestData"), ref output);

            StringAssert.Contains("SxpTest/", output.Content);
        }

        [Test]
        public void GenerateAsset_DefaultOrderSet_IncludesOrderArgument()
        {
            var output = new AssetOutputInfo();
            var template = new ScriptableObjectTemplate { DefaultOrder = 5 };
            template.GenerateAsset(MakeInfo("scriptable SxpTestData"), ref output);

            StringAssert.Contains("order", output.Content);
        }

        #endregion


        #region Suffix handling

        [Test]
        public void GenerateAsset_Suffix_RetainedByDefault()
        {
            var output = new AssetOutputInfo();
            new ScriptableObjectTemplate().GenerateAsset(MakeInfo("SxpTestDataAsset"), ref output);

            StringAssert.Contains("class SxpTestDataAsset", output.Content);
        }

        [Test]
        public void GenerateAsset_Suffix_RemovedWhenRemoveSuffixEnabled()
        {
            var output = new AssetOutputInfo();
            var template = new ScriptableObjectTemplate { RemoveSuffix = true };
            template.GenerateAsset(MakeInfo("SxpTestDataAsset"), ref output);

            StringAssert.Contains("class SxpTestData", output.Content);
            StringAssert.DoesNotContain("SxpTestDataAsset", output.Content);
        }

        #endregion

    }

}
