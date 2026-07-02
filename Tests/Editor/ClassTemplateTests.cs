using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class ClassTemplateTests
    {

        // Builds the AssetInfo a preset receives. A distinctive name avoids collisions with real project assets
        // when GenerateAsset resolves a unique path through AssetDatabase.
        private static AssetInfo MakeInfo(string name, string namespaceStr = null)
        {
            return new AssetInfo($"Assets/{name}.cs", namespaceStr, null, null);
        }

        #region CanGenerateAsset

        [Test]
        public void CanGenerateAsset_ClassPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new ClassTemplate().CanGenerateAsset(MakeInfo("class SxpTestFoo")));
        }

        [Test]
        public void CanGenerateAsset_StructPrefix_ReturnsTrue()
        {
            Assert.IsTrue(new ClassTemplate().CanGenerateAsset(MakeInfo("struct SxpTestBar")));
        }

        [Test]
        public void CanGenerateAsset_NoTrigger_ReturnsFalse()
        {
            Assert.IsFalse(new ClassTemplate().CanGenerateAsset(MakeInfo("SxpTestFoo")));
        }

        #endregion


        #region GenerateAsset

        [Test]
        public void GenerateAsset_ClassPrefix_GeneratesClass()
        {
            var output = new AssetOutputInfo();
            bool success = new ClassTemplate().GenerateAsset(MakeInfo("class SxpTestFoo"), ref output);

            Assert.IsTrue(success);
            Assert.IsFalse(string.IsNullOrEmpty(output.Content));
            StringAssert.Contains("class SxpTestFoo", output.Content);
        }

        [Test]
        public void GenerateAsset_StructPrefix_GeneratesStruct()
        {
            var output = new AssetOutputInfo();
            bool success = new ClassTemplate().GenerateAsset(MakeInfo("struct SxpTestBar"), ref output);

            Assert.IsTrue(success);
            StringAssert.Contains("struct SxpTestBar", output.Content);
        }

        [Test]
        public void GenerateAsset_SerializableByDefault_AddsSerializableAttribute()
        {
            var output = new AssetOutputInfo();
            // SerializableByDefault is true by default.
            new ClassTemplate().GenerateAsset(MakeInfo("class SxpTestFoo"), ref output);

            StringAssert.Contains("Serializable", output.Content);
        }

        [Test]
        public void GenerateAsset_SerializableDisabled_OmitsSerializableAttribute()
        {
            var output = new AssetOutputInfo();
            var template = new ClassTemplate { SerializableByDefault = false };
            template.GenerateAsset(MakeInfo("class SxpTestFoo"), ref output);

            // Neither the class name nor its namespace contains "Serializable", so its absence is meaningful.
            StringAssert.DoesNotContain("Serializable", output.Content);
        }

        [Test]
        public void GenerateAsset_WithNamespace_WrapsClassInNamespace()
        {
            var output = new AssetOutputInfo();
            new ClassTemplate().GenerateAsset(MakeInfo("class SxpTestFoo", "SxpTest.Sample"), ref output);

            StringAssert.Contains("namespace SxpTest.Sample", output.Content);
            StringAssert.Contains("class SxpTestFoo", output.Content);
        }

        #endregion

    }

}
