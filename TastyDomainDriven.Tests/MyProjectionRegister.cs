using System;
using System.Threading.Tasks;
using TastyDomainDriven.Projections;
using Xunit;

namespace TastyDomainDriven.Tests
{
	public class MyProjectionRegisterTest
	{
		[Fact]
		public void MyAsyncRegister()
		{
			var register = new ImplementOpenGeneric();
			register.Add(typeof(IConsumesAsync<>), typeof(DummyProjector1), typeof(DummyProjector2));
			Assert.Equal(1, register.GetInvokeableTypes<IConsumesAsync<MyEventA>>().Length);
			Assert.Equal(2, register.GetInvokeableTypes<IConsumesAsync<MyEventB>>().Length);
		}

		private class DummyProjector1 : 
			IConsumesAsync<MyEventA>,
			IConsumesAsync<MyEventB>
		{
			public Task Consume(MyEventA e)
			{
				Console.WriteLine("Invoked MyEventA - Projector 1");
				return Task.FromResult(new object());
			}

			public Task Consume(MyEventB e)
			{
				Console.WriteLine("Invoked MyEventB - Projector 1");
				return Task.FromResult(new object());
			}
		}

		private class DummyProjector2 :		
			IConsumesAsync<MyEventB>
		{
			public Task Consume(MyEventB e)
			{
				Console.WriteLine("Invoked MyEventB - Projector 2");
				return Task.FromResult(new object());
			}
		}
	}

	internal class MyEventB : IEvent
	{
		public IIdentity AggregateId { get; }
		public Guid EventId { get; }
		public DateTime Timestamp { get; }
	}

	internal class MyEventA : IEvent
	{
		public IIdentity AggregateId { get; }
		public Guid EventId { get; }
		public DateTime Timestamp { get; }
	}
}