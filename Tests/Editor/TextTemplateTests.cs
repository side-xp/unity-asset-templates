using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class TextTemplateTests
    {

        private static AssetInfo MakeInfo(string fileName)
        {
            return new AssetInfo($"Assets/{fileName}", null, null, null);
        }

        [Test]
        public void CanGenerateAsset_TxtExtension_ReturnsTrue()
        {
            Assert.IsTrue(new TextTemplate().CanGenerateAsset(MakeInfo("SxpTestNotes.txt")));
        }

        [Test]
        public void CanGenerateAsset_OtherExtension_ReturnsFalse()
        {
            Assert.IsFalse(new TextTemplate().CanGenerateAsset(MakeInfo("SxpTestNotes.cs")));
        }

        [Test]
        public void GenerateAsset_ProducesEmptyContent()
        {
            var output = new AssetOutputInfo();
            bool success = new TextTemplate().GenerateAsset(MakeInfo("SxpTestNotes.txt"), ref output);

            Assert.IsTrue(success);
            Assert.AreEqual(string.Empty, output.Content);
        }

    }

}
