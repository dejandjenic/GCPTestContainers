using Google.Cloud.Firestore;

namespace GCPTestContainers.Model;

[FirestoreData]
public class Todo
{
    [FirestoreDocumentId]
    public string Id { get; set; }
    [FirestoreProperty]
    public string Text { get; set; }
}