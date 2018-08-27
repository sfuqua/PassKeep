using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Helpers;
using PassKeep.Tests.Mocks;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for URL resolution and string placeholder substitution.
    /// </summary>
    [TestClass]
    public sealed class StringResolutionTests : TestClassBase
    {
        private const string ValidUrl = "http://example.com/a";
        private const string ValidOverride = "http://example.com/b";

        private const string InvalidUrl = "x-bad-x";

        public override TestContext TestContext
        {
            get; set;
        }

        [TestMethod]
        public void NullAndMissingUrlValues()
        {
            // When URL is null (which should never really happen), the URI string
            // should be empty.
            IKeePassEntry nullUrl = new MockEntry
            {
                Url = null
            };

            Assert.AreEqual(string.Empty, nullUrl.ConstructUriString());
            Assert.IsNull(nullUrl.GetLaunchableUri());

            // Basic test case for empty string (common case)
            IKeePassEntry emptyUrl = new MockEntry
            {
                Url = new UnprotectedString(string.Empty)
            };

            Assert.AreEqual(string.Empty, emptyUrl.ConstructUriString());
            Assert.IsNull(emptyUrl.GetLaunchableUri());

            // When OverrideUrl is not null, but empty, it is ignored
            IKeePassEntry emptyOverride = new MockEntry
            {
                Url = new UnprotectedString("http://example.com/"),
                OverrideUrl = string.Empty
            };

            Assert.AreEqual(emptyOverride.Url.ClearValue, emptyOverride.ConstructUriString());
            Assert.IsNotNull(emptyOverride.GetLaunchableUri());
        }

        /// <summary>
        /// Valid override URLs should override URLs.
        /// </summary>
        [TestMethod]
        public void ValidOverrideUrl()
        {
            IKeePassEntry entry = new MockEntry
            {
                Url = new UnprotectedString(ValidUrl),
                OverrideUrl = ValidOverride
            };

            Assert.AreEqual(ValidOverride, entry.ConstructUriString());
            Assert.AreEqual(ValidOverride, entry.GetLaunchableUri().AbsoluteUri);
        }

        /// <summary>
        /// Invalid override URLs should still override valid URLs.
        /// </summary>
        [TestMethod]
        public void InvalidOverrideUrl()
        {
            IKeePassEntry entry = new MockEntry
            {
                Url = new UnprotectedString(ValidUrl),
                OverrideUrl = InvalidUrl
            };

            Assert.AreEqual(InvalidUrl, entry.ConstructUriString());
            Assert.IsNull(entry.GetLaunchableUri());
        }

        /// <summary>
        /// "Happy path" tests of PlaceholderResolver's URL support.
        /// </summary>
        [TestMethod]
        public void TestBasicUrlComponents()
        {
            string basicUrl = "http://www.example.com/path";
            Assert.AreEqual(
                "www.example.com/path",
                PlaceholderResolver.GetUrlComponent(basicUrl, "RMVSCM")
            );
            Assert.AreEqual(
                "http",
                PlaceholderResolver.GetUrlComponent(basicUrl, "SCM")
            );
            Assert.AreEqual(
                string.Empty,
                PlaceholderResolver.GetUrlComponent(basicUrl, "USERINFO")
            );
            Assert.AreEqual(
                "www.example.com",
                PlaceholderResolver.GetUrlComponent(basicUrl, "HOST")
            );
            Assert.AreEqual(
                "80",
                PlaceholderResolver.GetUrlComponent(basicUrl, "PORT")
            );
            Assert.AreEqual(
                "/path",
                PlaceholderResolver.GetUrlComponent(basicUrl, "PATH")
            );
            Assert.AreEqual(
                string.Empty,
                PlaceholderResolver.GetUrlComponent(basicUrl, "QUERY")
            );
        }

        [TestMethod]
        public void TestComplexUrlComponents()
        {
            string complexUrl = "foo://sfuqua:blah@host.bar.baz/path/path?query=flaf";
            Assert.AreEqual(
                "sfuqua:blah@host.bar.baz/path/path?query=flaf",
                PlaceholderResolver.GetUrlComponent(complexUrl, "RMVSCM")
            );
            Assert.AreEqual(
                "foo",
                PlaceholderResolver.GetUrlComponent(complexUrl, "SCM")
            );
            Assert.AreEqual(
                "sfuqua:blah",
                PlaceholderResolver.GetUrlComponent(complexUrl, "USERINFO")
            );
            Assert.AreEqual(
                "sfuqua",
                PlaceholderResolver.GetUrlComponent(complexUrl, "USERNAME")
            );
            Assert.AreEqual(
                "blah",
                PlaceholderResolver.GetUrlComponent(complexUrl, "PASSWORD")
            );
            Assert.AreEqual(
                "host.bar.baz",
                PlaceholderResolver.GetUrlComponent(complexUrl, "HOST")
            );
            Assert.AreEqual(
                "-1",
                PlaceholderResolver.GetUrlComponent(complexUrl, "PORT")
            );
            Assert.AreEqual(
                "/path/path",
                PlaceholderResolver.GetUrlComponent(complexUrl, "PATH")
            );
            Assert.AreEqual(
                "?query=flaf",
                PlaceholderResolver.GetUrlComponent(complexUrl, "QUERY")
            );
        }

        [TestMethod]
        public void TestMoreComplexUrlComponents()
        {
            string complexUrl = "https://sfuqua@host.bar.baz:1337/path/path?query=flaf";
            Assert.AreEqual(
                "sfuqua@host.bar.baz:1337/path/path?query=flaf",
                PlaceholderResolver.GetUrlComponent(complexUrl, "RMVSCM")
            );
            Assert.AreEqual(
                "https",
                PlaceholderResolver.GetUrlComponent(complexUrl, "SCM")
            );
            Assert.AreEqual(
                "sfuqua",
                PlaceholderResolver.GetUrlComponent(complexUrl, "USERINFO")
            );
            Assert.AreEqual(
                "sfuqua",
                PlaceholderResolver.GetUrlComponent(complexUrl, "USERNAME")
            );
            Assert.AreEqual(
                string.Empty,
                PlaceholderResolver.GetUrlComponent(complexUrl, "PASSWORD")
            );
            Assert.AreEqual(
                "host.bar.baz",
                PlaceholderResolver.GetUrlComponent(complexUrl, "HOST")
            );
            Assert.AreEqual(
                "1337",
                PlaceholderResolver.GetUrlComponent(complexUrl, "PORT")
            );
            Assert.AreEqual(
                "/path/path",
                PlaceholderResolver.GetUrlComponent(complexUrl, "PATH")
            );
            Assert.AreEqual(
                "?query=flaf",
                PlaceholderResolver.GetUrlComponent(complexUrl, "QUERY")
            );
        }

        [TestMethod]
        public void TestBadUrlComponents()
        {
            // Bad URL
            Assert.AreEqual(
                string.Empty,
                PlaceholderResolver.GetUrlComponent("foo//bar", "SCM")
            );

            // Bad component
            Assert.IsNull(PlaceholderResolver.GetUrlComponent("http://example.com", "FOO"));
        }

        /// <summary>
        /// Examples from http://keepass.info/help/base/placeholders.html#url
        /// </summary>
        [TestMethod]
        public void KeePassTestVectors()
        {
            string url = "http://user:pw@keepass.info:81/path/example.php?q=e&s=t";
            Assert.AreEqual(
                "user:pw@keepass.info:81/path/example.php?q=e&s=t",
                PlaceholderResolver.GetUrlComponent(url, "RMVSCM")
            );
            Assert.AreEqual(
                "http",
                PlaceholderResolver.GetUrlComponent(url, "SCM")
            );
            Assert.AreEqual(
                "user:pw",
                PlaceholderResolver.GetUrlComponent(url, "USERINFO")
            );
            Assert.AreEqual(
                "user",
                PlaceholderResolver.GetUrlComponent(url, "USERNAME")
            );
            Assert.AreEqual(
                "pw",
                PlaceholderResolver.GetUrlComponent(url, "PASSWORD")
            );
            Assert.AreEqual(
                "keepass.info",
                PlaceholderResolver.GetUrlComponent(url, "HOST")
            );
            Assert.AreEqual(
                "81",
                PlaceholderResolver.GetUrlComponent(url, "PORT")
            );
            Assert.AreEqual(
                "/path/example.php",
                PlaceholderResolver.GetUrlComponent(url, "PATH")
            );
            Assert.AreEqual(
                "?q=e&s=t",
                PlaceholderResolver.GetUrlComponent(url, "QUERY")
            );
        }

        /// <summary>
        /// Basic tests for placeholder strings that might trip up parsing.
        /// </summary>
        [TestMethod]
        public void BadPlaceholders()
        {
            IKeePassEntry entry = GetTestEntry();
            Assert.AreEqual(
                "foo{username",
                PlaceholderResolver.Resolve("foo{username", entry)
            );
            Assert.AreEqual(
                "foo{username{",
                PlaceholderResolver.Resolve("foo{username{", entry)
            );
            Assert.AreEqual(
                "foo{BAR}",
                PlaceholderResolver.Resolve("foo{BAR}", entry)
            );
            Assert.AreEqual(
                $"{{USERNAME{entry.UserName.ClearValue}}}",
                PlaceholderResolver.Resolve("{USERNAME{UsErnAME}}", entry)
            );
            Assert.AreEqual(
                $"{entry.Password.ClearValue}}}{{{entry.Title.ClearValue}",
                PlaceholderResolver.Resolve("{pASsword}}{{TITLE}", entry)
            );
            Assert.AreEqual(
                $"{{{{{{{{{entry.Password.ClearValue}{{}}{{}}}}}}}}}}}}}}}}{{{{}}{{",
                PlaceholderResolver.Resolve("{{{{{PASSWORD}{}{}}}}}}}}{{}{", entry)
            );
            Assert.AreEqual(
                "{C}",
                PlaceholderResolver.Resolve("{C}", entry)
            );
            Assert.AreEqual(
                string.Empty,
                PlaceholderResolver.Resolve("{C:}", entry)
            );
            Assert.AreEqual(
                string.Empty,
                PlaceholderResolver.Resolve("{C::asdf:::}", entry)
            );
        }

        [TestMethod]
        public void PlaceholderTest()
        {
            IKeePassEntry entry = GetTestEntry();

            Assert.AreEqual(
                $"this is a test string with scheme http and {entry.Title.ClearValue}",
                PlaceholderResolver.Resolve("this is a test string {C:comment here}with scheme {URL:SCM} and {TITLE}", entry)
            );
        }

        [TestMethod]
        public void PlaceholderE2EUrlTest()
        {
            IKeePassEntry entry = GetTestEntry();
            entry.OverrideUrl = "http://{C:test}example.com/{URL:HOST}";

            Assert.AreEqual(
                "http://example.com/www.example.com",
                entry.ConstructUriString()
            );
        }

        /// <summary>
        /// Returns an entry with known field values that can be used
        /// to test string resolution.
        /// </summary>
        /// <returns></returns>
        private IKeePassEntry GetTestEntry()
        {
            return new MockEntry
            {
                Title = new UnprotectedString("EntryTitle"),
                UserName = new UnprotectedString("TheUsername"),
                Password = new UnprotectedString("APassword"),
                Url = new UnprotectedString("http://www.example.com/"),
                Notes = new UnprotectedString("The entry's notes")
            };
        }
    }
}
