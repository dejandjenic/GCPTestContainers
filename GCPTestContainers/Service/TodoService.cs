using GCPTestContainers.Events;
using GCPTestContainers.Model;
using GCPTestContainers.Repository;

namespace GCPTestContainers.Service;

public interface ITodoService
{
    Task<List<Todo>> List();
    Task Add(Todo item);
    Task Delete(string id);
}

public class TodoService(ITodoRepository repository,IEventPublisher eventPublisher) : ITodoService
{
    public Task<List<Todo>> List()
    {
        return repository.List();
    }
    
    public async Task Add(Todo item)
    {
        await eventPublisher.Publish(new TodoCreated());
        await repository.Add(item);
    } 
    
    public async Task Delete(string id)
    {
        await eventPublisher.Publish(new TodoDeleted());
        await repository.Delete(id);
    } 
}