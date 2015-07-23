namespace TastyDomainDriven
{
	using System;
	using System.Collections.Generic;

	public class QueueBus : IBus
    {
        private Queue<ICommand> queue =new Queue<ICommand>();

        public void Dispatch(ICommand cmd)
        {
            if (cmd.Timestamp == DateTime.MinValue)
            {
                throw new ArgumentException("Command must have a timestamp: " + cmd.GetType());
            }

            this.queue.Enqueue(cmd);
        }

        public void EmptyQueue(IBus bus)
        {
            while (this.queue.Count > 0)
            {
                bus.Dispatch(this.queue.Dequeue());
            }
        }
    }
}