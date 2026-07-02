using NUnit.Framework;

using SideXP.AssetTemplates.EditorOnly;

namespace SideXP.AssetTemplates.Tests
{

    public class PrefixSuffixPatternTests
    {

        #region Prefix matching

        [Test]
        public void Match_PrefixFollowedBySpace_MatchesAndRemovesPrefix()
        {
            var pattern = new PrefixSuffixPattern("comp");
            bool matched = pattern.Match("comp Foo", out string outputName, out string matchingPart, out bool isPrefix);

            Assert.IsTrue(matched);
            Assert.IsTrue(isPrefix);
            Assert.AreEqual("comp", matchingPart);
            Assert.AreEqual("Foo", outputName);
        }

        [Test]
        public void Match_PrefixFollowedByUppercase_Matches()
        {
            // MatchIfNextLetterUppercase is on by default, so a prefix glued to a PascalCase name still matches.
            var pattern = new PrefixSuffixPattern("comp");
            bool matched = pattern.Match("compFoo", out string outputName, out _, out bool isPrefix);

            Assert.IsTrue(matched);
            Assert.IsTrue(isPrefix);
            Assert.AreEqual("Foo", outputName);
        }

        [Test]
        public void Match_PrefixWithInvertedFirstLetterCasing_Matches()
        {
            // FirstLetterCaseInsensitive is on by default, so "Comp" matches a "comp" part.
            var pattern = new PrefixSuffixPattern("comp");
            bool matched = pattern.Match("Comp Foo", out string outputName, out string matchingPart, out bool isPrefix);

            Assert.IsTrue(matched);
            Assert.IsTrue(isPrefix);
            Assert.AreEqual("Comp", matchingPart);
            Assert.AreEqual("Foo", outputName);
        }

        [Test]
        public void Match_PrefixFollowedByLowercaseLetter_DoesNotMatch()
        {
            // "comp" immediately followed by a lowercase letter (as in "complete") is not a prefix.
            var pattern = new PrefixSuffixPattern("comp");
            Assert.IsFalse(pattern.Match("complete"));
        }

        #endregion


        #region Suffix matching

        [Test]
        public void Match_SuffixPrecededBySpace_MatchesAndRemovesSuffix()
        {
            var pattern = new PrefixSuffixPattern("comp");
            bool matched = pattern.Match("Foo comp", out string outputName, out string matchingPart, out bool isPrefix);

            Assert.IsTrue(matched);
            Assert.IsFalse(isPrefix);
            Assert.AreEqual("comp", matchingPart);
            Assert.AreEqual("Foo", outputName);
        }

        [Test]
        public void Match_PascalCaseSuffixWithoutSpace_Matches()
        {
            // An uppercase-first suffix ("Comp", via case inversion) may be attached directly, with no separating space.
            var pattern = new PrefixSuffixPattern("comp");
            bool matched = pattern.Match("FooComp", out string outputName, out string matchingPart, out bool isPrefix);

            Assert.IsTrue(matched);
            Assert.IsFalse(isPrefix);
            Assert.AreEqual("Comp", matchingPart);
            Assert.AreEqual("Foo", outputName);
        }

        [Test]
        public void Match_LowercaseSuffixWithoutSpace_DoesNotMatch()
        {
            // A lowercase-first suffix requires a separating space, so "Foocomp" is not a match.
            var pattern = new PrefixSuffixPattern("comp");
            Assert.IsFalse(pattern.Match("Foocomp"));
        }

        #endregion


        #region No match

        [Test]
        public void Match_UnrelatedName_ReturnsFalseAndOutputsOriginalName()
        {
            var pattern = new PrefixSuffixPattern("comp");
            bool matched = pattern.Match("Foo", out string outputName, out string matchingPart, out bool isPrefix);

            Assert.IsFalse(matched);
            Assert.AreEqual("Foo", outputName);
            Assert.IsNull(matchingPart);
            Assert.IsFalse(isPrefix);
        }

        [Test]
        public void Match_BarePartWithoutName_DoesNotMatch()
        {
            // The part alone (no accompanying name) matches neither as a prefix (needs a following space or
            // uppercase letter) nor as a suffix (needs a preceding space), so it is rejected.
            var pattern = new PrefixSuffixPattern("comp");
            Assert.IsFalse(pattern.Match("comp"));
        }

        #endregion


        #region Whitespace handling

        [Test]
        public void Match_SurroundingWhitespace_IsTrimmed()
        {
            var pattern = new PrefixSuffixPattern("comp");
            bool matched = pattern.Match("  comp Foo  ", out string outputName, out _, out _);

            Assert.IsTrue(matched);
            Assert.AreEqual("Foo", outputName);
        }

        #endregion


        #region Options

        [Test]
        public void Match_RemovePartDisabled_KeepsPartInOutputName()
        {
            var pattern = new PrefixSuffixPattern();
            PrefixSuffixPattern.PartOptions options = PrefixSuffixPattern.PartOptions.Default;
            options.RemovePart = false;
            pattern.AddPrefix("comp", options);

            bool matched = pattern.Match("comp Foo", out string outputName, out string matchingPart, out bool isPrefix);

            Assert.IsTrue(matched);
            Assert.IsTrue(isPrefix);
            Assert.AreEqual("comp", matchingPart);
            Assert.AreEqual("comp Foo", outputName);
        }

        [Test]
        public void Match_MatchIfNextLetterUppercaseDisabled_RequiresSpaceAfterPrefix()
        {
            var pattern = new PrefixSuffixPattern();
            PrefixSuffixPattern.PartOptions options = PrefixSuffixPattern.PartOptions.Default;
            options.MatchIfNextLetterUppercase = false;
            pattern.AddPrefix("comp", options);

            // Without the uppercase allowance, a prefix glued to a PascalCase name no longer matches...
            Assert.IsFalse(pattern.Match("compFoo"));
            // ...but a space-separated prefix still does.
            Assert.IsTrue(pattern.Match("comp Foo"));
        }

        [Test]
        public void Match_FirstLetterCaseInsensitiveDisabled_RequiresExactCasing()
        {
            var pattern = new PrefixSuffixPattern();
            PrefixSuffixPattern.PartOptions options = PrefixSuffixPattern.PartOptions.Default;
            options.FirstLetterCaseInsensitive = false;
            pattern.AddPrefix("comp", options);

            Assert.IsFalse(pattern.Match("Comp Foo"));
            Assert.IsTrue(pattern.Match("comp Foo"));
        }

        [Test]
        public void AddPrefix_DuplicatePart_IsIgnored()
        {
            // Registering the same prefix twice must not throw; the second registration is a no-op.
            var pattern = new PrefixSuffixPattern();
            pattern.AddPrefix("comp");

            Assert.DoesNotThrow(() => pattern.AddPrefix("comp"));
            Assert.IsTrue(pattern.Match("comp Foo"));
        }

        #endregion


        #region Constructors

        [Test]
        public void Match_DistinctPrefixAndSuffixSets_AreNotInterchangeable()
        {
            // "pre" is registered only as a prefix, "post" only as a suffix.
            var pattern = new PrefixSuffixPattern(new[] { "pre" }, new[] { "post" });

            Assert.IsTrue(pattern.Match("pre Foo", out _, out _, out bool preIsPrefix));
            Assert.IsTrue(preIsPrefix);

            Assert.IsTrue(pattern.Match("Foo post", out _, out _, out bool postIsPrefix));
            Assert.IsFalse(postIsPrefix);

            // A prefix-only part used at the end (as a suffix) does not match.
            Assert.IsFalse(pattern.Match("Foo pre"));
            // A suffix-only part used at the start (as a prefix) does not match.
            Assert.IsFalse(pattern.Match("post Foo"));
        }

        #endregion

    }

}
