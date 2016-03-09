using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Azure.AzureBlob;

namespace TastyDomainDriven.Sample
{
    internal class PerformanceRun
    {
        private string connection;
        private string container;
        private AzureAsyncAppender appender;
        private EventStoreAsync eventstore;

        public PerformanceRun()
        {
            Console.WriteLine("");
            Console.Write("Azure Connection string:");
            this.connection = Console.ReadLine();
            Console.WriteLine("");
            Console.Write("Azure container:");
            this.container = Console.ReadLine();
            this.appender = new AzureAsyncAppender(connection, container, new AzureBlobAppenderOptions() { NamingPolicy = new NameDashGuidNaming("es") });
            this.eventstore = new EventStoreAsync(appender);
        }

        public async Task Run()
        {
            await this.appender.Initialize();
            var dummybytes = new byte[120];
            ran.NextBytes(dummybytes);
            await this.appender.Append("somethings", dummybytes, 3);
            await RunAlotOfWrites(1024,4*1024*1024);
        }

        static readonly Random ran = new Random();
        public async Task RunAlotOfWrites(int bytefrom, int byteto)
        {
            var id = "perf-" + Guid.NewGuid();
            for (int i = 0; i < 10; i++)
            {
                await PerformanceAppend(i, bytefrom, byteto, id);
            }

            Console.WriteLine("Press to read all data...");
            Console.ReadLine();
            
            var stop = new Stopwatch();
            stop.Start();
            var stream = await this.appender.ReadRecords(0, int.MaxValue);
            stop.Stop();

            Console.WriteLine("Got: {0} bytes in {1}ms", stream.Sum(x => x.Data.Length), stop.ElapsedMilliseconds);
            Console.ReadLine();
        }

        private async Task PerformanceAppend(int version, int bytefrom, int byteto, string streamid)
        {
            byte[] bytes = new byte[ran.Next(bytefrom, byteto)];
            ran.NextBytes(bytes);
            Console.WriteLine("");
            Console.Write("Uploading: {0} ({1} bytes)", streamid, bytes.Length);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            await appender.Append(streamid, bytes, version);            
            watch.Stop();

            var ms = watch.ElapsedMilliseconds;
            var kb = 1024;
            Console.Write(" done in {0} ms ({1:0.00} kbyte/sec ~ {2:0}kb)", ms, (bytes.Length/kb)/(ms/1000), bytes.Length/kb);
        }
    }
}