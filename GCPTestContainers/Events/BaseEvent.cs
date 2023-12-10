namespace GCPTestContainers.Events;

public class BaseEvent
{
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string? Type => this.GetType().FullName;
}