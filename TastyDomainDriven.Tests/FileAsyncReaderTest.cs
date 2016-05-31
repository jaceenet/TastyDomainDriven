﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Events;
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
            
            var fs = new FileAppendOnlyStoreAsync(new FileAppendOnlyStoreAsync.Options(File) { MasterStreamName= "mystream_stream" });
            var es = new EventStoreAsync(fs);
            var stream = await es.ReplayAll(0, int.MaxValue);
            Assert.NotEmpty(stream.Events);
        }

        [Fact(Skip = "local test")]
        public async Task WriteFile()
        {
            var fs = new FileAppendOnlyStoreAsync(new FileAppendOnlyStoreAsync.Options(File) { FileStream = s => Path.Combine(this.File, s + ".dat") });
            var es = new EventStoreAsync(fs);
            
            var id = new GuidId(Guid.NewGuid());
            await es.AppendToStream(id, 0, new List<IEvent>() {new MyNewEventEvent(id.ToString(), Guid.NewGuid(), DateTime.UtcNow)});
        }
    }
}