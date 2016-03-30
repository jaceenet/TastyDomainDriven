



namespace Events
{


	using System;
	using TastyDomainDriven;


	public sealed class MyNewEventEvent : TastyDomainDriven.IEvent
    {
		private string _id;
		private IIdentity _aggregateId;
		private Guid _eventId;
		private DateTime _timestamp;

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

	using System;
	using TastyDomainDriven;


	public sealed class MyNewEventSerializer
    {
		public void Write(MyNewEventEvent obj, BinaryWriter binaryWriter)
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
		
		public MyNewEventEvent Read(BinaryReader binaryReader)
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
    }

}