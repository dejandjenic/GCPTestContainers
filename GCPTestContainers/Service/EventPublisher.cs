using GCPTestContainers.Events;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Encoding = System.Text.Encoding;

namespace GCPTestContainers.Service;

public interface IEventPublisher
{
    Task Publish(BaseEvent eventData);
}

public class EventPublisher(PublisherClientImpl client) : IEventPublisher
{
    public async Task Publish(BaseEvent eventData)
    {
        await client.PublishAsync(System.Text.Json.JsonSerializer.Serialize(eventData));
    }
}