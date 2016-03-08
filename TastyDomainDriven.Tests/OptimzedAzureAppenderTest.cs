using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Azure.AzureBlob;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class OptimzedAzureAppenderTest
    {
        private AzureAsyncAppender appender;
        private IEventStoreAsync eventstore;

        public OptimzedAzureAppenderTest()
        {
            this.appender = new AzureAsyncAppender("", "testing", "events");
            this.eventstore = new EventStoreAsync(appender);
        }

        [Fact]
        public async Task ReadStreamVersion()        
        {
            var id1 = GuidId.NewGuidId();
            var id2 = GuidId.NewGuidId();
            var id3 = GuidId.NewGuidId();

            await
                this.eventstore.AppendToStream(id1, 0,
                    new IEvent[] {BigEventData.Create(id1, 228)});

            await
                this.eventstore.AppendToStream(id1, 1,
                    new IEvent[] { BigEventData.Create(id1, 118), BigEventData.Create(id1, 98), BigEventData.Create(id1, 328) });

            await
                this.eventstore.AppendToStream(id1, 2,
                    new IEvent[] { BigEventData.Create(id1, 28) });

            await
                this.eventstore.AppendToStream(id2, 0,
                    new IEvent[] {BigEventData.Create(id2, 128), BigEventData.Create(id2, 228)});

            var stream = await this.eventstore.LoadEventStream(id1, 1, 2);

            Assert.Equal(3, stream.Events.Count);
            Assert.Equal(2, stream.Version);
        }

        [Fact]
        public async Task CreateContainer()
        {
            await appender.Initialize();
        }

        [Fact]
        public async Task CanWriteBytes()
        {
            byte[] bytes = new byte[1024*10];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 1;
            }

            for (int i = 0; i < 10; i++)
            {
                var guid = Guid.NewGuid();

                await appender.Append("test-" + guid, bytes, 0);
                await appender.Append("test-" + guid, bytes, 1);
                await appender.Append("test-" + guid, bytes, 2);
                await appender.Append("test-" + guid, bytes, 3);
                await appender.Append("test-" + guid, bytes, 4);
                await appender.Append("test-" + guid, bytes, 5);
                await appender.Append("test-" + guid, bytes, 6);
            }
        }

        [Fact]
        public async Task CanAppendEvents()
        {
            var id = new StringId("mystream");
            var events = new IEvent[]
            {
                BigEventData.Create(id, 20), BigEventData.Create(id, 1024),
                BigEventData.Create(id, 400121), BigEventData.Create(id, 2048)
            };

            await this.eventstore.AppendToStream(id, 0, events);
            await this.eventstore.AppendToStream(id, 1, events);
            await this.eventstore.AppendToStream(new StringId("stream2"), 0, events);
        }

        [Fact]
        public async Task AppendRandomId()
        {
            var id = GuidId.NewGuidId();

            await this.eventstore.AppendToStream(id, 0, new IEvent[] { BigEventData.Create(id, 440), BigEventData.Create(id, 200) });
            await this.eventstore.AppendToStream(id, 1, new IEvent[] { BigEventData.Create(id, 30), BigEventData.Create(id, 200) });

            var events = await this.eventstore.LoadEventStream(id);
            Assert.Equal(2, events.Version);
        }

        [Fact]
        public async Task CanConflictOnAppend()
        {
            var id = GuidId.NewGuidId();
            var events = new IEvent[]
            {
                BigEventData.Create(id, 20), BigEventData.Create(id, 1024),
                BigEventData.Create(id, 400121), BigEventData.Create(id, 2048)
            };

            var run1 = this.eventstore.AppendToStream(id, 0, events);
            var run2 = this.eventstore.AppendToStream(id, 1, events);
            var run3 = this.eventstore.AppendToStream(id, 2, events);
            var run4 = this.eventstore.AppendToStream(id, 3, events);
            
            try
            {
                Task.WaitAll(run3, run2, run4, run1);
            }
            catch (AggregateException ex)
            {
                throw;
            }
        }

        [Fact]
        public async Task ReadAll()
        {
            var items = await this.eventstore.ReplayAll();
            Assert.Equal(3, items.Version);
        }

        [Fact]
        public async Task ReadStream()
        {
            var items = await this.eventstore.LoadEventStream(new StringId("mystream"));
            Assert.Equal(2, items.Version);
        }
    }
}