using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Azure.AzureBlob;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class AzureBlobAsyncAppenderTest
    {
        private AzureAsyncAppender appender;
        private IEventStoreAsync eventstore;
        private string connection;
        private string container;

        public AzureBlobAsyncAppenderTest()
        {
            this.connection = "";
            this.container = "testing";
            this.appender = new AzureAsyncAppender(connection, container, new AzureBlobAppenderOptions() {NamingPolicy = new NameDashGuidNaming("es")});
            this.eventstore = new EventStoreAsync(appender);
        }

        [Fact(Skip = "Missing connectionstring")]
        public async Task GetIndexAndLock()
        {
            var filelock = new FileIndexLock(CloudStorageAccount.Parse(this.connection), this.container, "lock.txt");
            var filelock2 = new FileIndexLock(CloudStorageAccount.Parse(this.connection), this.container, "lock.txt");
            await filelock.CreateIfNotExist();
            await filelock.GetLeaseAndRead();
            await filelock2.GetLeaseAndRead();
            await Task.Delay(2000);
            await filelock.Release();
        }

        [Fact(Skip = "Missing connectionstring")]
        public async Task WriteTwoStreamsAndReadAll()        
        {
            var id1 = GuidId.NewGuidId();
            var id2 = GuidId.NewGuidId();
            
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

        [Fact(Skip = "needs connectionstring")]
        public async Task CreateContainer()
        {
            await appender.Initialize();
        }

        [Fact(Skip = "Missing connectionstring")]
        public async Task CanReadEmpty()
        {
            var stream = await this.eventstore.LoadEventStream(new StringId("dsfslkfjsldkfjsldkfj"));
            Assert.Equal(0, stream.Version);
        }


        [Fact(Skip = "Missing connectionstring")]
        public async Task CanWriteBytes()
        {
            byte[] bytes = new byte[1024*10];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 1;
            }

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {


                var t = Task.Run(async () =>
                {
                    var guid = Guid.NewGuid();
                    await appender.Append("test-" + guid, bytes, 0);
                    await appender.Append("test-" + guid, bytes, 1);
                    await appender.Append("test-" + guid, bytes, 2);
                    await appender.Append("test-" + guid, bytes, 3);
                    await appender.Append("test-" + guid, bytes, 4);
                    await appender.Append("test-" + guid, bytes, 5);
                    await appender.Append("test-" + guid, bytes, 6);
                });

                tasks.Add(t);

            }

            await Task.WhenAll(tasks.ToArray());
        }

        [Fact(Skip = "needs connectionstring")]
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

        [Fact(Skip = "Missing connectionstring")]
        public async Task WriteSingleAppend()
        {
            var id = new StringId("test-" + Guid.NewGuid());
            await
                this.eventstore.AppendToStream(id, 0,
                    new IEvent[] {BigEventData.Create(id, 120), BigEventData.Create(id, 110)});
        }

        [Fact(Skip = "needs connectionstring")]
        public async Task AppendRandomId()
        {
            var id = GuidId.NewGuidId();

            await this.eventstore.AppendToStream(id, 0, new IEvent[] { BigEventData.Create(id, 440), BigEventData.Create(id, 200) });
            await this.eventstore.AppendToStream(id, 1, new IEvent[] { BigEventData.Create(id, 30), BigEventData.Create(id, 200) });

            var events = await this.eventstore.LoadEventStream(id);
            Assert.Equal(2, events.Version);
        }

        [Fact(Skip = "Missing connectionstring")]
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

        [Fact(Skip = "needs connectionstring")]
        public async Task ReadAll()
        {
            var items = await this.eventstore.ReplayAll();
            Assert.Equal(3, items.Version);
        }

        [Fact(Skip = "needs connectionstring")]
        public async Task ReadStream()
        {
            var items = await this.eventstore.LoadEventStream(new StringId("mystream"));
            Assert.Equal(2, items.Version);
        }
    }
}