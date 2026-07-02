using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class EnumTemplateTests
    {

        private static AssetInfo MakeInfo(string name)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_EnumPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new EnumTemplate().CanGenerateAsset(MakeInfo("enum SxpTestColor")));
        }

        [Test]
        public void CanGenerateAsset_FlagsPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new EnumTemplate().CanGenerateAsset(MakeInfo("flags SxpTestColor")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new EnumTemplate().CanGenerateAsset(MakeInfo("SxpTestColor")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_Enum_GeneratesEnumWithLetterPrefix()
        {
            var output = new AssetOutputInfo();
            // AlwaysUseLetterPrefix is true by default, so an enum gets the "E" prefix.
            bool success = new EnumTemplate().GenerateAsset(MakeInfo("enum SxpTestColor"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("enum ESxpTestColor", output.Content);
            StringAssert.DoesNotContain("Flags", output.Content);
        }

        [Test]
        public void GenerateAsset_Flags_AddsFlagsAttributeAndSampleMembers()
        {
            var output = new AssetOutputInfo();
            new EnumTemplate().GenerateAsset(MakeInfo("flags SxpTestColor"), ref output);

            StringAssert.Contains("enum FSxpTestColor", output.Content);
            StringAssert.Contains("Flags", output.Content);
            // The flags variant seeds example bitshift members.
            StringAssert.Contains("Red", output.Content);
            StringAssert.Contains("1 << 0", output.Content);
        }

        [Test]
        public void GenerateAsset_LetterPrefixDisabled_KeepsPlainName()
        {
            var output = new AssetOutputInfo();
            var template = new EnumTemplate { AlwaysUseLetterPrefix = false };
            template.GenerateAsset(MakeInfo("enum SxpTestColor"), ref output);

            StringAssert.Contains("enum SxpTestColor", output.Content);
            StringAssert.DoesNotContain("ESxpTestColor", output.Content);
        }

        #endregion

    }

}
