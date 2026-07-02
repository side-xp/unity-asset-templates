using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class AsmdefTemplateTests
    {

        private static AssetInfo MakeInfo(string fileName)
        {
            return new AssetInfo($"Assets/{fileName}", null, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_AsmdefExtension_ReturnsTrue()
        {
            Assert.IsTrue(new AsmdefTemplate().CanGenerateAsset(MakeInfo("SxpTest.Sample.asmdef")));
        }

        [Test]
        public void CanGenerateAsset_OtherExtension_ReturnsFalse()
        {
            Assert.IsFalse(new AsmdefTemplate().CanGenerateAsset(MakeInfo("SxpTestSample.cs")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_Runtime_SetsNameAndRootNamespace()
        {
            var output = new AssetOutputInfo();
            bool success = new AsmdefTemplate().GenerateAsset(MakeInfo("SxpTest.Sample.asmdef"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("SxpTest.Sample", output.Content);
            StringAssert.Contains("autoReferenced", output.Content);
        }

        [Test]
        public void GenerateAsset_EditorAssembly_IsEditorOnlyWithEditorOnlyNamespace()
        {
            var output = new AssetOutputInfo();
            new AsmdefTemplate().GenerateAsset(MakeInfo("SxpTest.Sample.Editor.asmdef"), ref output);

            // includePlatforms is restricted to the Editor, and the ".Editor" namespace part becomes ".EditorOnly".
            StringAssert.Contains("\"Editor\"", output.Content);
            StringAssert.Contains("EditorOnly", output.Content);
        }

        [Test]
        public void GenerateAsset_UnityInName_StrippedFromRootNamespace()
        {
            var output = new AssetOutputInfo();
            new AsmdefTemplate().GenerateAsset(MakeInfo("SideXP.Unity.Game.asmdef"), ref output);

            // The "Unity" part is removed from the root namespace (but kept in the assembly name).
            StringAssert.Contains("SideXP.Game", output.Content);
        }

        [Test]
        public void GenerateAsset_AutoReferencedDisabled_ReflectedInOutput()
        {
            var output = new AssetOutputInfo();
            var template = new AsmdefTemplate { AutoReferenced = false };
            template.GenerateAsset(MakeInfo("SxpTest.Sample.asmdef"), ref output);

            StringAssert.Contains("\"autoReferenced\": false", output.Content);
        }

        #endregion

    }

}
