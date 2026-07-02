using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class InterfaceTemplateTests
    {

        private static AssetInfo MakeInfo(string name, System.Type parentType = null)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, parentType);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_InterfacePrefix_ReturnsTrue()
        {
            Assert.IsTrue(new InterfaceTemplate().CanGenerateAsset(MakeInfo("interface SxpTestThing")));
        }

        [Test]
        public void CanGenerateAsset_InterfaceSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new InterfaceTemplate().CanGenerateAsset(MakeInfo("SxpTestThingInterface")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new InterfaceTemplate().CanGenerateAsset(MakeInfo("SxpTestThing")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_Default_GeneratesInterfaceWithLetterPrefix()
        {
            var output = new AssetOutputInfo();
            // AlwaysUseLetterPrefix is true by default, so the "I" prefix is added.
            bool success = new InterfaceTemplate().GenerateAsset(MakeInfo("interface SxpTestThing"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("interface ISxpTestThing", output.Content);
        }

        [Test]
        public void GenerateAsset_LetterPrefixDisabled_KeepsPlainName()
        {
            var output = new AssetOutputInfo();
            var template = new InterfaceTemplate { AlwaysUseLetterPrefix = false };
            template.GenerateAsset(MakeInfo("interface SxpTestThing"), ref output);

            StringAssert.Contains("interface SxpTestThing", output.Content);
            StringAssert.DoesNotContain("ISxpTestThing", output.Content);
        }

        [Test]
        public void GenerateAsset_ParentInterfaceSelected_InheritsIt()
        {
            // ISampleContract is declared in ComponentTemplateTests (same test assembly).
            var output = new AssetOutputInfo();
            new InterfaceTemplate().GenerateAsset(MakeInfo("interface SxpTestThing", typeof(ISampleContract)), ref output);

            StringAssert.Contains("interface ISxpTestThing", output.Content);
            StringAssert.Contains("ISampleContract", output.Content);
        }

        #endregion

    }

}
