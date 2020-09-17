using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TastyDomainDriven.AggregateService;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Memory;
using TastyDomainDriven.PerformanceMeasurements;
using TastyDomainDriven.Projections;
using TastyDomainDriven.Sample;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.CommandServices;
using TastyDomainDriven.Sample.Events;
using TastyDomainDriven.Sample.Projections;
using TastyDomainDriven.Sample.Properties;
using TastyDomainDriven.Serialization;
using Xunit;

// ReSharper disable once CheckNamespace
namespace TastyDomainDriven.Tests.DI
{
    public sealed class DiTests
    {
        private IServiceProvider GetServices()
        {
            IServiceCollection collection = new ServiceCollection();

            collection.AddTransient<IPerformanceLogger, TestPerformanceLogger>();
            collection.AddTransient<AggregateWithDi>();
            collection.AddTransient<StateWithDi>();

            return collection.BuildServiceProvider();
        }

        [Fact]
        public async Task TestDi()
        {
            var services = GetServices();

            var appender = new MemoryAppendStoreAsync();
            IEventStoreAsync es = new EventStoreAsync(appender);

            IDtoConverter<SayingDto, Saying> converter = new MyDtoConverters();
            Stream mem = System.IO.File.Open("test.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            IDtoSerializer<SayingDto> dtoSerializer = new MicrosoftJsonSerializer<SayingDto>(mem);
            ITableReaderWriter<Saying> said = new TableWriterForSerializer<Saying, SayingDto>(new MemoryHashTableWriter<Saying>(), converter, dtoSerializer);

            IAsyncProjection projection = new TestProjection(new AsyncProjectFromImplementation(new SayingHistoryProjection(said)));

            CompositeAsyncProjection composite = new CompositeAsyncProjection(services.GetService<IPerformanceLogger>(), projection);

            IEventStoreAsync es2 = new EventStoreAsyncPublisher(es, composite);

            //IAsyncCommandDispatcher dispatcher = new ConsoleLoggerDispatcher(new CompositeAsyncCommandDispatcher(new SaySomething(es2)));
            ICommandHandler handler = new ServiceWithDi(es2, services, services.GetService<IPerformanceLogger>());

            await handler.GetExecutor(new SayCommand()
            {
                PersonId = new PersonId(1),
                Say = "Say hello",
                Timestamp = DateTime.UtcNow
            }).Execute();
        }
    }

    internal class MyDtoConverters : IDtoConverter<SayingDto, Saying>
    {
        public SayingDto Serialize(Saying obj)
        {
            return new SayingDto() { Id = obj.PersonId.id, Said = obj.Said };
        }

        public Saying Deserialize(SayingDto obj)
        {
            return new Saying(new PersonId(obj.Id)) { Said = obj.Said };
        }
    }

    internal sealed class TestProjection : IAsyncProjection
    {
        private readonly IAsyncProjection asyncProjection;

        public TestProjection(IAsyncProjection asyncProjection)
        {
            this.asyncProjection = asyncProjection;
        }

        public async Task Consume<T>(T @event) where T : IEvent
        {
            Console.WriteLine("Putting {0} into {1}", @event, asyncProjection.GetType());
            await this.asyncProjection.Consume(@event);
        }
    }

    internal sealed class TestPerformanceLogger : IPerformanceLogger
    {
        public Task TrackProjection(ProjectionPerformance performance)
        {
            Debug.WriteLine("TrackProjection: " + performance.Event.GetType().ToString());
            return Task.CompletedTask;
        }

        public Task TrackAggregate(AggregatePerformance performance)
        {
            Debug.WriteLine("TrackAggregate: " + performance.AggregateType.ToString());
            return Task.CompletedTask;
        }
    }

    public class StateWithDi : AggregateState, IStateEvent<PersonSaidEvent>
    {
        private IPerformanceLogger logger;

        public StateWithDi(IPerformanceLogger logger)
        {
            this.logger = logger;
        }

        public void When(PersonSaidEvent e)
        {
            
        }
    }

    internal class ServiceWithDi : AggregateHandlerWithDi<AggregateWithDi>
    {
        public ServiceWithDi(IEventStoreAsync eventStore, IServiceProvider provider, IPerformanceLogger performanceLogger = null) : base(eventStore, provider, performanceLogger)
        {
            Register<SayCommand>(When);
        }

        private Task When(SayCommand arg)
        {
            return Update(arg.PersonId, x => x.Handle(arg));
        }
    }

    internal class AggregateWithDi : AggregateRoot<StateWithDi>
    {
        private IPerformanceLogger logger;

        public AggregateWithDi(IPerformanceLogger logger)
        {
            this.logger = logger;
        }

        protected override StateWithDi CreateState()
        {
            return new StateWithDi(this.logger);
        }

        public void Handle(SayCommand cmd)
        {
            Apply(new PersonSaidEvent()
            {
                AggregateId = Id,
                EventId = Guid.NewGuid(),
                PersonId = cmd.PersonId,
                Saying = cmd.Say,
                Timestamp = cmd.Timestamp
            });
        }
    }

    internal class UpdateAggregateAsyncByDi<T> : UpdateAggregateAsync<T> where T : IAggregate
    {
        private IServiceProvider provider;

        public UpdateAggregateAsyncByDi(IServiceProvider provider, IPerformanceLogger logger) : base(logger)
        {
            this.provider = provider;
        }

        protected override T CreateAggregate()
        {
            return (T)this.provider.GetService(typeof(T));
        }
    }

    internal class AggregateHandlerWithDi<T> : AggregateHandler<T> where T : IAggregate
    {
        private IServiceProvider provider;

        public AggregateHandlerWithDi(IEventStoreAsync eventStore, IServiceProvider provider, IPerformanceLogger performanceLogger = null) : base(eventStore, performanceLogger)
        {
            this.provider = provider;
        }

        protected override async Task Update<TIdent>(TIdent id, Action<T> execute)
        {
            await new UpdateAggregateAsyncByDi<T>(this.provider, this.performanceLogger).Execute(this.eventStore, id, execute);
        }
    }
}
