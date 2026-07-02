using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class EditorWindowTemplateTests
    {

        private static AssetInfo MakeInfo(string name)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_WindowPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new EditorWindowTemplate().CanGenerateAsset(MakeInfo("window SxpTestFoo")));
        }

        [Test]
        public void CanGenerateAsset_EditorWindowSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new EditorWindowTemplate().CanGenerateAsset(MakeInfo("SxpTestFooEditorWindow")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new EditorWindowTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_Default_GeneratesEditorWindowWithOpenAndOnGUI()
        {
            var output = new AssetOutputInfo();
            bool success = new EditorWindowTemplate().GenerateAsset(MakeInfo("window SxpTestFoo"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("class SxpTestFooEditorWindow", output.Content);
            StringAssert.Contains("EditorWindow", output.Content);
            StringAssert.Contains("Open", output.Content);
            StringAssert.Contains("MenuItem", output.Content);
            StringAssert.Contains("GetWindow", output.Content);
            StringAssert.Contains("OnGUI", output.Content);
            StringAssert.DoesNotContain("CreateGUI", output.Content);
            Assert.IsTrue(output.IsEditorContent);
        }

        [Test]
        public void GenerateAsset_UseVisualElement_GeneratesCreateGUIInsteadOfOnGUI()
        {
            var output = new AssetOutputInfo();
            var template = new EditorWindowTemplate { UseVisualElement = true };
            template.GenerateAsset(MakeInfo("window SxpTestFoo"), ref output);

            StringAssert.Contains("CreateGUI", output.Content);
            StringAssert.DoesNotContain("OnGUI", output.Content);
        }

        [Test]
        public void GenerateAsset_WindowMenuBase_UsedInMenuItemPath()
        {
            var output = new AssetOutputInfo();
            var template = new EditorWindowTemplate { WindowMenuBase = "SxpTest" };
            template.GenerateAsset(MakeInfo("window SxpTestFoo"), ref output);

            StringAssert.Contains("SxpTest/", output.Content);
        }

        #endregion

    }

}
