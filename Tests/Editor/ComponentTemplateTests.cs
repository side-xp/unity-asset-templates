using System;

using NUnit.Framework;

using UnityEngine;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    // A MonoBehaviour-derived type used to exercise "inherit from the selected component" behavior.
    public class SampleComponentBehaviour : MonoBehaviour { }

    // An interface used to exercise "implement the selected interface" behavior.
    public interface ISampleContract { }

    public class ComponentTemplateTests
    {

        // Builds the AssetInfo a preset receives. A distinctive name avoids collisions with real project assets
        // when GenerateAsset resolves a unique path through AssetDatabase.
        private static AssetInfo MakeInfo(string name, Type parentType = null)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, parentType);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_CompPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new ComponentTemplate().CanGenerateAsset(MakeInfo("comp SxpTestFoo")));
        }

        [Test]
        public void CanGenerateAsset_CompSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new ComponentTemplate().CanGenerateAsset(MakeInfo("SxpTestFooComp")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new ComponentTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region Inheritance

        [Test]
        public void GenerateAsset_NoParent_InheritsMonoBehaviour()
        {
            var output = new AssetOutputInfo();
            bool success = new ComponentTemplate().GenerateAsset(MakeInfo("comp SxpTestFoo"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("class SxpTestFoo", output.Content);
            StringAssert.Contains("MonoBehaviour", output.Content);
        }

        [Test]
        public void GenerateAsset_ParentComponentSelected_InheritsParentInsteadOfMonoBehaviour()
        {
            var output = new AssetOutputInfo();
            new ComponentTemplate().GenerateAsset(MakeInfo("comp SxpTestFoo", typeof(SampleComponentBehaviour)), ref output);

            StringAssert.Contains("SampleComponentBehaviour", output.Content);
            // The parent already is a component, so MonoBehaviour must not be re-added as the base type.
            StringAssert.DoesNotContain("MonoBehaviour", output.Content);
        }

        [Test]
        public void GenerateAsset_ParentInterfaceSelected_InheritsMonoBehaviourAndImplementsInterface()
        {
            var output = new AssetOutputInfo();
            new ComponentTemplate().GenerateAsset(MakeInfo("comp SxpTestFoo", typeof(ISampleContract)), ref output);

            StringAssert.Contains("MonoBehaviour", output.Content);
            StringAssert.Contains("ISampleContract", output.Content);
            // C# requires the base class before implemented interfaces.
            int monoBehaviourIndex = output.Content.IndexOf("MonoBehaviour", StringComparison.Ordinal);
            int interfaceIndex = output.Content.IndexOf("ISampleContract", StringComparison.Ordinal);
            Assert.Greater(interfaceIndex, monoBehaviourIndex, "MonoBehaviour must precede the interface in the base list.");
        }

        #endregion


        #region Attributes

        [Test]
        public void GenerateAsset_Always_AddsAddComponentMenu()
        {
            var output = new AssetOutputInfo();
            new ComponentTemplate().GenerateAsset(MakeInfo("comp SxpTestFoo"), ref output);

            StringAssert.Contains("AddComponentMenu", output.Content);
        }

        [Test]
        public void GenerateAsset_NoBaseHelpURL_OmitsHelpURL()
        {
            var output = new AssetOutputInfo();
            // BaseHelpURL is empty by default.
            new ComponentTemplate().GenerateAsset(MakeInfo("comp SxpTestFoo"), ref output);

            StringAssert.DoesNotContain("HelpURL", output.Content);
        }

        [Test]
        public void GenerateAsset_BaseHelpURLSet_AddsHelpURLWithThatUrl()
        {
            var output = new AssetOutputInfo();
            var template = new ComponentTemplate { BaseHelpURL = "https://example.com/help" };
            template.GenerateAsset(MakeInfo("comp SxpTestFoo"), ref output);

            StringAssert.Contains("HelpURL", output.Content);
            StringAssert.Contains("https://example.com/help", output.Content);
        }

        [Test]
        public void GenerateAsset_BaseAddComponentMenuSet_PrefixesMenuPath()
        {
            var output = new AssetOutputInfo();
            var template = new ComponentTemplate { BaseAddComponentMenu = "SxpTest" };
            template.GenerateAsset(MakeInfo("comp SxpTestFoo"), ref output);

            StringAssert.Contains("SxpTest/", output.Content);
        }

        #endregion


        #region Suffix handling

        [Test]
        public void GenerateAsset_Suffix_RetainedByDefault()
        {
            var output = new AssetOutputInfo();
            new ComponentTemplate().GenerateAsset(MakeInfo("SxpTestFooComp"), ref output);

            StringAssert.Contains("class SxpTestFooComp", output.Content);
        }

        [Test]
        public void GenerateAsset_Suffix_RemovedWhenRemoveSuffixEnabled()
        {
            var output = new AssetOutputInfo();
            var template = new ComponentTemplate { RemoveSuffix = true };
            template.GenerateAsset(MakeInfo("SxpTestFooComp"), ref output);

            StringAssert.Contains("class SxpTestFoo", output.Content);
            StringAssert.DoesNotContain("SxpTestFooComp", output.Content);
        }

        #endregion

    }

}
