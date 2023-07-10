using EventSourcing.Shared.Events;
using EventStore.ClientAPI;
using System.Text;
using System.Text.Json;

namespace EventSourcing.API.EventStores
{
    public abstract class AbstractStream
    {
        protected readonly LinkedList<IEvent> Events = new LinkedList<IEvent>();
        private string _streamName { get; } // sadece constructor için o yüzden set e gerek yok
        private readonly IEventStoreConnection _eventStoreConnection;

        protected AbstractStream(string streamName, IEventStoreConnection eventStoreConnection)
        {
            _streamName = streamName;
            _eventStoreConnection = eventStoreConnection;
        }

        public async Task SaveAsync()
        {
            // Events içerisinde dolaşıp, bir eventdat oluşturacağız. eventstore a yeni bir event oluşturabilmek için eventdataya ihtiyaç oluyor. 
            var newEvents = Events.ToList().Select(x => new EventData(
                Guid.NewGuid(),
                x.GetType().Name,
                true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(x, inputType: x.GetType())),
                Encoding.UTF8.GetBytes(x.GetType().FullName))).ToList();
            
            // stream e ekle. tabloya ekleme gibi
            await _eventStoreConnection.AppendToStreamAsync(_streamName, ExpectedVersion.Any, newEvents);

            //eventleri temizlemeyi unutma 
            Events.Clear();
        }
    }
}
