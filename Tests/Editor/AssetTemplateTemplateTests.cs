using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class AssetTemplateTemplateTests
    {

        private static AssetInfo MakeInfo(string name)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_TemplatePrefix_ReturnsTrue()
        {
            Assert.IsTrue(new AssetTemplateTemplate().CanGenerateAsset(MakeInfo("template SxpTestFoo")));
        }

        [Test]
        public void CanGenerateAsset_AssetTemplateSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new AssetTemplateTemplate().CanGenerateAsset(MakeInfo("SxpTestFooAssetTemplate")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new AssetTemplateTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_ImplementsIAssetTemplateAsSerializableEditorContent()
        {
            var output = new AssetOutputInfo();
            bool success = new AssetTemplateTemplate().GenerateAsset(MakeInfo("template SxpTestFoo"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("class SxpTestFoo", output.Content);
            StringAssert.Contains("IAssetTemplate", output.Content);
            StringAssert.Contains("AssetTemplate", output.Content);
            StringAssert.Contains("Serializable", output.Content);
            Assert.IsTrue(output.IsEditorContent);
        }

        #endregion

    }

}
