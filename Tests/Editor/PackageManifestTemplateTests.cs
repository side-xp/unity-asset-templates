using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class PackageManifestTemplateTests
    {

        private static AssetInfo MakeInfo(string fileName)
        {
            return new AssetInfo($"Assets/{fileName}", null, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_PackageJson_ReturnsTrue()
        {
            Assert.IsTrue(new PackageManifestTemplate().CanGenerateAsset(MakeInfo("package.json")));
        }

        [Test]
        public void CanGenerateAsset_OtherJsonFile_ReturnsFalse()
        {
            Assert.IsFalse(new PackageManifestTemplate().CanGenerateAsset(MakeInfo("manifest.json")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_WritesExactlyPackageJsonWithoutDoubledExtension()
        {
            var output = new AssetOutputInfo();
            bool success = new PackageManifestTemplate().GenerateAsset(MakeInfo("package.json"), ref output);

            Assert.IsTrue(success);
            Assert.AreEqual("Assets/package.json", output.Path);
        }

        [Test]
        public void GenerateAsset_UsesDefaultManifestValues()
        {
            var output = new AssetOutputInfo();
            new PackageManifestTemplate().GenerateAsset(MakeInfo("package.json"), ref output);

            StringAssert.Contains("com.company.name", output.Content);
            StringAssert.Contains("0.0.1", output.Content);
            StringAssert.Contains("New Package", output.Content);
        }

        [Test]
        public void GenerateAsset_UsesConfiguredName()
        {
            var output = new AssetOutputInfo();
            var template = new PackageManifestTemplate { PackageDefaultName = "com.sxp.test" };
            template.GenerateAsset(MakeInfo("package.json"), ref output);

            StringAssert.Contains("com.sxp.test", output.Content);
        }

        #endregion

    }

}
