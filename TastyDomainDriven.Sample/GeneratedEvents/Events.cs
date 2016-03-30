



namespace Events
{


	using System;
	using TastyDomainDriven;


	public sealed class MyNewEventEvent : TastyDomainDriven.IEvent
    {
		private readonly string _id;
		private readonly IIdentity _aggregateId;
		private readonly Guid _eventId;
		private readonly DateTime _timestamp;

        public MyNewEventEvent(string id, Guid eventId, DateTime timestamp)
        {
			this._id = id;
			this._eventId = eventId;
			this._timestamp = timestamp;
				
		}

        		
		
		public string Id 
		{ 
			get 
			{
				return (string)this._id; 
			}	
		} 					
					
		
		public IIdentity AggregateId 
		{ 
			get 
			{
				return (IIdentity)this._aggregateId; 
			}	
		} 					
					
		
		public Guid EventId 
		{ 
			get 
			{
				return (Guid)this._eventId; 
			}	
		} 					
					
		
		public DateTime Timestamp 
		{ 
			get 
			{
				return (DateTime)this._timestamp; 
			}	
		} 					
					
			
			
}

namespace Serializers
{
	using Events;
	using System.IO;
	using TastyDomainDriven.Dsl;

	using System;
	using TastyDomainDriven;


	public sealed class MyNewEventSerializer	: IEventVersionSerializer
    {
		private void WriteEvent(MyNewEventEvent obj, BinaryWriter binaryWriter)
        {
			if (obj.Id != null)
            {
				binaryWriter.Write(1);
                binaryWriter.Write(obj.Id);
            }

            binaryWriter.Write(0);
			if (obj.EventId != Guid.Empty)
            {
				binaryWriter.Write(2);
                binaryWriter.Write(obj.EventId.ToByteArray());
            }

            binaryWriter.Write(0);
			if (obj.Timestamp != DateTime.MinValue)
            {
				binaryWriter.Write(3);
                binaryWriter.Write(obj.Timestamp.ToBinary());
            }

            binaryWriter.Write(0);
				
		}
		
		private MyNewEventEvent ReadEvent(BinaryReader binaryReader)
        {
			string id = null;
			Guid eventId = Guid.Empty;
			DateTime timestamp = DateTime.MinValue;
				

			var field = binaryReader.ReadInt32();

			while(field != 0)
			{
				if (field == 1)
				{									
					id = binaryReader.ReadString();
				}

				if (field == 2)
				{									
					eventId = new Guid(binaryReader.ReadBytes(16));
				}

				if (field == 3)
				{									
					timestamp = DateTime.FromBinary(binaryReader.ReadInt64());
				}

				
				field = binaryReader.ReadInt32();
			}

			return new MyNewEventEvent(id, eventId, timestamp);
		}        

		public int GetEventId { get { return 199; } }
	    public Type EventType { get { return typeof (MyNewEventEvent); } }

		public void Write(object @event, BinaryWriter writer)
	    {
	        this.WriteEvent((MyNewEventEvent) @event, writer);
	    }

	    public object Read(BinaryReader reader)
	    {
	        return this.ReadEvent(reader);
	    }
    }

	}
}