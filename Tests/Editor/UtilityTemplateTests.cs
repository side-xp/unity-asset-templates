using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class UtilityTemplateTests
    {

        private static AssetInfo MakeInfo(string name)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_UtilityPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new UtilityTemplate().CanGenerateAsset(MakeInfo("utility SxpTestFoo")));
        }

        [Test]
        public void CanGenerateAsset_UtilitySuffix_ReturnsTrue()
        {
            Assert.IsTrue(new UtilityTemplate().CanGenerateAsset(MakeInfo("SxpTestFooUtility")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new UtilityTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_Prefix_GeneratesStaticClass()
        {
            var output = new AssetOutputInfo();
            bool success = new UtilityTemplate().GenerateAsset(MakeInfo("utility SxpTestFoo"), ref output);

            Assert.IsTrue(success);
            // The CodeDom output has "public class" rewritten to "public static class".
            StringAssert.Contains("static class SxpTestFoo", output.Content);
        }

        [Test]
        public void GenerateAsset_Suffix_RetainedByDefault()
        {
            var output = new AssetOutputInfo();
            new UtilityTemplate().GenerateAsset(MakeInfo("SxpTestFooUtility"), ref output);

            StringAssert.Contains("static class SxpTestFooUtility", output.Content);
        }

        [Test]
        public void GenerateAsset_Suffix_RemovedWhenRemoveSuffixEnabled()
        {
            var output = new AssetOutputInfo();
            var template = new UtilityTemplate { RemoveSuffix = true };
            template.GenerateAsset(MakeInfo("SxpTestFooUtility"), ref output);

            StringAssert.Contains("static class SxpTestFoo", output.Content);
            StringAssert.DoesNotContain("SxpTestFooUtility", output.Content);
        }

        #endregion

    }

}
