using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class AttributeTemplateTests
    {

        private static AssetInfo MakeInfo(string name)
        {
            return new AssetInfo($"Assets/{name}.cs", null, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_AttrSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new AttributeTemplate().CanGenerateAsset(MakeInfo("SxpTestFooAttr")));
        }

        [Test]
        public void CanGenerateAsset_AttributeSuffix_ReturnsTrue()
        {
            Assert.IsTrue(new AttributeTemplate().CanGenerateAsset(MakeInfo("SxpTestFooAttribute")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new AttributeTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_AttrSuffix_NormalizesNameToAttributeAndInheritsAttribute()
        {
            var output = new AssetOutputInfo();
            bool success = new AttributeTemplate().GenerateAsset(MakeInfo("SxpTestFooAttr"), ref output);

            Assert.IsTrue(success);
            // The "-Attr" short suffix is normalized to the full "Attribute" suffix.
            StringAssert.Contains("class SxpTestFooAttribute", output.Content);
            StringAssert.Contains(": Attribute", output.Content);
        }

        [Test]
        public void GenerateAsset_AttributeSuffix_KeepsName()
        {
            var output = new AssetOutputInfo();
            new AttributeTemplate().GenerateAsset(MakeInfo("SxpTestFooAttribute"), ref output);

            StringAssert.Contains("class SxpTestFooAttribute", output.Content);
        }

        [Test]
        public void GenerateAsset_Always_AddsAttributeUsageForClasses()
        {
            var output = new AssetOutputInfo();
            new AttributeTemplate().GenerateAsset(MakeInfo("SxpTestFooAttr"), ref output);

            StringAssert.Contains("AttributeUsage", output.Content);
            StringAssert.Contains("AttributeTargets.Class", output.Content);
        }

        #endregion

    }

}
