using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Events;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.File;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class StreamAppenderTests
    {
        private readonly MemoryStream appendStream = new MemoryStream();


        public class WriteToStreamTests 
            : StreamAppenderTests
        {
            [Fact]
            public async void WriteAppend()
            {
                var appender = new StreamAppenderAsync(StreamAppenderAsync.StreamAppenderOptions.UseSingleStream(this.appendStream));

                var es = new EventStoreAsync(appender);
                var id = new StringId("id1");
                var events = new IEvent[] { BigEventData.Create(id, 12),BigEventData.Create(id, 128), BigEventData.Create(id, 13) };

                await es.AppendToStream(id, 0, events.ToList());

                Assert.Equal(962, this.appendStream.Length);
            }

            [Fact]
            public async void ReadAppend()
            {
                var appender = new StreamAppenderAsync(StreamAppenderAsync.StreamAppenderOptions.UseSingleStream(this.appendStream));

                var es = new EventStoreAsync(appender);
                var id = new StringId("id1");
                var events = new IEvent[] { BigEventData.Create(id, 12), BigEventData.Create(id, 128), BigEventData.Create(id, 13) };

                var id2 = new StringId("id2");
                var events2 = new IEvent[] { BigEventData.Create(id, 100), BigEventData.Create(id, 128), BigEventData.Create(id, 100) };

                var events3 = new IEvent[] { BigEventData.Create(id, 44) };

                await es.AppendToStream(id, 0, events.ToList());
                await es.AppendToStream(id2, 0, events2.ToList());
                await es.AppendToStream(id, 1, events3.ToList());

                Assert.Equal(2800, this.appendStream.Length);

                var stream = await es.LoadEventStream(id);
                Assert.Equal(2, stream.Version);

                var stream2 = await es.LoadEventStream(id2);
                Assert.Equal(1, stream2.Version);
            }            
        }
    }
}