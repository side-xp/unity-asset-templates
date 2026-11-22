using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class AssetInfoTests
    {

        #region Constructor

        [Test]
        public void Constructor_ParsesPathIntoNameLocationExtension()
        {
            var info = new AssetInfo("Assets/Foo/MyAsset.cs", "My.Namespace", "Assets/Parent.cs", null);

            Assert.AreEqual("MyAsset", info.Name);
            // The extension is stored without the leading "." character.
            Assert.AreEqual("cs", info.Extension);
            Assert.AreEqual("My.Namespace", info.Namespace);
            Assert.AreEqual("Assets/Parent.cs", info.ParentPath);
            // Location is the containing directory; separators are normalized for a platform-independent comparison.
            Assert.AreEqual("Assets/Foo", info.Location.Replace('\\', '/'));
        }

        [Test]
        public void Constructor_NoExtension_ExtensionIsEmpty()
        {
            var info = new AssetInfo("Assets/Foo/MyAsset", null, null, null);

            Assert.AreEqual("MyAsset", info.Name);
            Assert.AreEqual(string.Empty, info.Extension);
        }

        [Test]
        public void Constructor_MultipleDots_NameKeepsInnerDotsExtensionIsLastSegment()
        {
            var info = new AssetInfo("Assets/Foo/My.Asset.json", null, null, null);

            Assert.AreEqual("My.Asset", info.Name);
            Assert.AreEqual("json", info.Extension);
        }

        #endregion


        #region ParentNamespace

        [Test]
        public void ParentNamespace_NoParentType_IsNull()
        {
            var info = new AssetInfo("Assets/Foo.cs", null, null, null);
            Assert.IsNull(info.ParentNamespace);
        }

        [Test]
        public void ParentNamespace_WithParentType_ReturnsItsNamespace()
        {
            var info = new AssetInfo("Assets/Foo.cs", null, null, typeof(string));

            Assert.AreEqual(typeof(string), info.ParentType);
            Assert.AreEqual("System", info.ParentNamespace);
        }

        #endregion


        #region ToString

        [Test]
        public void ToString_ContainsNameAndExtension()
        {
            var info = new AssetInfo("Assets/Foo/MyAsset.cs", null, null, null);
            string text = info.ToString();

            StringAssert.Contains("MyAsset", text);
            StringAssert.Contains("cs", text);
        }

        #endregion

    }

}
