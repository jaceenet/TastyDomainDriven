using TastyDomainDriven.Azure.AzureBlob;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class DirectoryNamingTest
    {
        [Fact]
        public void TestSimple()
        {
            IDirectoryNaming naming = new PrefixedDirectory("es");
            Assert.Equal("es/name", naming.GetPath("name"));            
        }

        [Fact]
        public void TestGuidLikePath()
        {
            IDirectoryNaming naming = new DirectoryNaming(new PrefixedDirectory("es"), new CharReplaceNaming('-', '/', 2));
            var name = "name-37248776-4211-466D-A378-B38C3B2A921C";
            Assert.Equal("es/name/37248776/4211-466D-A378-B38C3B2A921C", naming.GetPath(name));
        }
    }
}