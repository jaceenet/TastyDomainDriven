using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.File;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class FileAsyncReaderTest
    {
        public string File = @"D:\temp\es";

        [Fact(Skip = "local test")]        
        public async Task ReadFile()
        {
            var fs = new FileAppendOnlyStoreAsync(File);
            var mainstream = await fs.ReadRecords(0, int.MaxValue);           
            Assert.NotEmpty(mainstream);
        }

        [Fact(Skip = "local test")]
        public async Task ReadFileAsEvents()
        {
            
            var fs = new FileAppendOnlyStoreAsync(new FileAppendOnlyStoreAsync.Options(File) { MasterStreamFileName = "mystream_stream" });
            var es = new EventStoreAsync(fs);
            var stream = await es.ReplayAll(0, int.MaxValue);
            Assert.NotEmpty(stream.Events);
        }
    }
}