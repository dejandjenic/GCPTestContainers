using GCPTestContainers.Model;
using Google.Cloud.Firestore;

namespace GCPTestContainers.Repository;

public interface ITodoRepository
{
    Task<List<Todo>> List();
    Task Add(Todo item);
    Task Delete(string id);
}

public class TodoRepository(FirestoreDb db) : ITodoRepository
{
    public const string TodoCollectionName = "Todo";
    public async Task<List<Todo>> List()
    {
        return (await db.Collection(TodoCollectionName).GetSnapshotAsync()).
        Documents.
        Select(x => 
            x.ConvertTo<Todo>()).
        ToList();
    }

    public async Task Add(Todo item)
    {
        await db.
            Collection(TodoCollectionName)
            .Document().
            CreateAsync(item);
    } 
    
    public async Task Delete(string id)
    {
        await db.
            Collection(TodoCollectionName)
            .Document(id).
            DeleteAsync();
    } 
}