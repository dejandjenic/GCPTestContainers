using System.Net.Http.Json;
using FluentAssertions;
using GCPTestContainers.Model;

namespace GCPTestContainers.Tests;

public class TodoTests : TestBase
{

    async Task<List<Todo>> GetTodos()
    {
        return (await client.GetFromJsonAsync<List<Todo>>("/todo"))!;
    }
    
    private async Task<string> AddTodo()
    {
        var todoText = Guid.NewGuid().ToString();
        await client.PostAsJsonAsync("/todo", new Todo()
        {
            Text = todoText
        });
        return todoText;
    }
    
    private async Task DeleteTodo(string id)
    {
        await client.DeleteAsync($"/todo/{id}");
    }
    
    [Fact]
    public async Task Should_Return_Empty_List()
    {
        var response = await GetTodos();
        response.Count.Should().Be(0);
    }
    
    [Fact]
    public async Task Should_Add_Todo()
    {
        var todos = await GetTodos();
        todos.Count.Should().Be(0);

        var todoText = await AddTodo();

        todos = await GetTodos();
        todos.Count.Should().Be(1);
        todos.FirstOrDefault()!.Text.Should().Be(todoText);

        await ClearTodoCollection();
        (await CheckSubscription()).Should().Be(1);
    }

    

    [Fact]
    public async Task Should_Delete_Todo()
    {
        var todos = await GetTodos();
        todos.Count.Should().Be(0);

        var todoText = await AddTodo();
        
        todos = await GetTodos();
        todos.Count.Should().Be(1);
        todos.FirstOrDefault()!.Text.Should().Be(todoText);
        
        await DeleteTodo(todos.FirstOrDefault()!.Id);
        
        todos = await GetTodos();
        todos.Count.Should().Be(0);
        
        await ClearTodoCollection();
        (await CheckSubscription()).Should().Be(2);
    }

    

    public TodoTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
        
    }
}