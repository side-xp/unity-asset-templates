using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class AssetOutputInfoTests
    {

        #region SetExtension

        [Test]
        public void SetExtension_ReplacesExistingExtension()
        {
            var output = new AssetOutputInfo { Path = "Assets/Foo.cs" };
            string result = output.SetExtension("txt");

            Assert.AreEqual("Assets/Foo.txt", result);
            Assert.AreEqual("Assets/Foo.txt", output.Path);
        }

        [Test]
        public void SetExtension_PathWithoutExtension_AppendsExtension()
        {
            // Regression: an extensionless path used to throw (Substring out of range).
            var output = new AssetOutputInfo { Path = "Assets/Foo" };
            string result = output.SetExtension("txt");

            Assert.AreEqual("Assets/Foo.txt", result);
            Assert.AreEqual("Assets/Foo.txt", output.Path);
        }

        [Test]
        public void SetExtension_MultipleDots_ReplacesOnlyLastSegment()
        {
            var output = new AssetOutputInfo { Path = "Assets/Foo.bar.cs" };
            string result = output.SetExtension("txt");

            Assert.AreEqual("Assets/Foo.bar.txt", result);
        }

        [Test]
        public void SetExtension_NullOrWhitespace_LeavesPathUnchanged()
        {
            var output = new AssetOutputInfo { Path = "Assets/Foo.cs" };

            Assert.AreEqual("Assets/Foo.cs", output.SetExtension(null));
            Assert.AreEqual("Assets/Foo.cs", output.SetExtension("   "));
            Assert.AreEqual("Assets/Foo.cs", output.Path);
        }

        #endregion


        #region Properties

        [Test]
        public void Path_NullOrWhitespace_BecomesEmpty()
        {
            var output = new AssetOutputInfo { Path = "Assets/Foo.cs" };

            output.Path = null;
            Assert.AreEqual(string.Empty, output.Path);

            output.Path = "   ";
            Assert.AreEqual(string.Empty, output.Path);
        }

        [Test]
        public void Path_RelativePath_IsPreserved()
        {
            var output = new AssetOutputInfo { Path = "Assets/Foo.cs" };
            Assert.AreEqual("Assets/Foo.cs", output.Path);
        }

        [Test]
        public void Content_DefaultIsNull_AndIsSettable()
        {
            var output = new AssetOutputInfo();
            Assert.IsNull(output.Content);

            output.Content = "some content";
            Assert.AreEqual("some content", output.Content);
        }

        [Test]
        public void IsEditorContent_DefaultIsFalse_AndIsSettable()
        {
            var output = new AssetOutputInfo();
            Assert.IsFalse(output.IsEditorContent);

            output.IsEditorContent = true;
            Assert.IsTrue(output.IsEditorContent);
        }

        #endregion

    }

}
