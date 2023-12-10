using GCPTestContainers.Model;
using GCPTestContainers.Repository;
using Google.Cloud.Firestore;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Firestore;
using Testcontainers.PubSub;

namespace GCPTestContainers.Tests;

public class TestBase 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
	
    private readonly CustomWebApplicationFactory<Program> _factory;
    protected HttpClient client;
    private static FirestoreDb firestoreDb;
    private static PublisherServiceApiClient publisher;
    private static SubscriberServiceApiClient subscriber;
    private static SubscriptionName subscriptionName;
    
    private static  Func<IServiceCollection,Task> OverideServices = async (svc) =>
    {
        var appSettings = svc.BuildServiceProvider().GetRequiredService<AppSettings>();
        await SetupFirestore(svc,appSettings);

        await SetupPubSub(svc,appSettings);
    };

    private static async Task SetupFirestore(IServiceCollection svc, AppSettings appSettings)
    {
        FirestoreContainer _firestoreContainer = new FirestoreBuilder().Build();
        await _firestoreContainer.StartAsync();
        
        
        var firestoreDbBuilder = new FirestoreDbBuilder();
        firestoreDbBuilder.ProjectId = appSettings.ProjectId;
        firestoreDbBuilder.Endpoint = _firestoreContainer.GetEmulatorEndpoint();
        firestoreDbBuilder.ChannelCredentials = ChannelCredentials.Insecure;

        firestoreDb = await firestoreDbBuilder.BuildAsync();

        svc.AddSingleton(firestoreDb);
    }

    private static async Task SetupPubSub(IServiceCollection svc, AppSettings appSettings)
    {
        PubSubContainer _pubSubContainer = new PubSubBuilder().Build();
        await _pubSubContainer.StartAsync();
        
        
        var projectId = appSettings.ProjectId;
        var topicId = appSettings.TopicName;
        var subscriptionId = $"{appSettings.TopicName}-sub";
		
        var topicName = new TopicName(projectId, topicId);
        subscriptionName = new SubscriptionName(projectId, subscriptionId);
		
        var publisherClientBuilder = new PublisherServiceApiClientBuilder();
        publisherClientBuilder.Endpoint = _pubSubContainer.GetEmulatorEndpoint();
        publisherClientBuilder.ChannelCredentials = ChannelCredentials.Insecure;
        
        var subscriberClientBuilder = new SubscriberServiceApiClientBuilder();
        subscriberClientBuilder.Endpoint = _pubSubContainer.GetEmulatorEndpoint();
        subscriberClientBuilder.ChannelCredentials = ChannelCredentials.Insecure;

	   
        publisher = await publisherClientBuilder.BuildAsync();
        subscriber = await subscriberClientBuilder.BuildAsync();
        
        publisher.CreateTopic(topicName);
        await subscriber.CreateSubscriptionAsync(subscriptionName, topicName, null, 60);
            
        svc.AddSingleton(
            new PublisherClientImpl(
                topicName, 
                new List<PublisherServiceApiClient>() { publisher },
                new PublisherClient.Settings(), 
                async () => { }));
    }

    public TestBase(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.OverideServices = OverideServices;
        client = _factory.CreateClient();
    }

    public async Task ClearTodoCollection()
    {
        var collection = firestoreDb.Collection(TodoRepository.TodoCollectionName);
        foreach (var item in (await collection.GetSnapshotAsync()).Documents.Select(x=> x.ConvertTo<Todo>()))
        {
            await collection.Document(item.Id).DeleteAsync();
        }
    }

    public async Task<int> CheckSubscription()
    {
        var messages = await subscriber.PullAsync(subscriptionName, 5);
        await subscriber.AcknowledgeAsync(subscriptionName,messages.ReceivedMessages.Select(x=>x.AckId));
        return messages.ReceivedMessages.Count;
    }
	
}