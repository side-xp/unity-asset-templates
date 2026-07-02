using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class MarkdownTemplateTests
    {

        private static AssetInfo MakeInfo(string fileName)
        {
            return new AssetInfo($"Assets/{fileName}", null, null, null);
        }

        [Test]
        public void CanGenerateAsset_MdExtension_ReturnsTrue()
        {
            Assert.IsTrue(new MarkdownTemplate().CanGenerateAsset(MakeInfo("SxpTestReadme.md")));
        }

        [Test]
        public void CanGenerateAsset_OtherExtension_ReturnsFalse()
        {
            Assert.IsFalse(new MarkdownTemplate().CanGenerateAsset(MakeInfo("SxpTestReadme.txt")));
        }

        [Test]
        public void GenerateAsset_ProducesBasicMarkdown()
        {
            var output = new AssetOutputInfo();
            bool success = new MarkdownTemplate().GenerateAsset(MakeInfo("SxpTestReadme.md"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("# Title", output.Content);
            StringAssert.Contains("markdownguide.org", output.Content);
        }

    }

}
