using GCPTestContainers;
using GCPTestContainers.Model;
using GCPTestContainers.Repository;
using GCPTestContainers.Service;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings);

builder.Services.AddSingleton(FirestoreDb.Create(appSettings.ProjectId));
builder.Services.AddSingleton((PublisherClientImpl)PublisherClientImpl.Create(new TopicName(appSettings.ProjectId,appSettings.TopicName)));
builder.Services.AddScoped<ITodoRepository,TodoRepository>();
builder.Services.AddScoped<ITodoService,TodoService>();
builder.Services.AddScoped<IEventPublisher,EventPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/todo", (ITodoService service) =>  service.List())
    .WithName("GetTodos")
    .WithOpenApi();

app.MapPost("/todo", (ITodoService service,[FromBody]Todo model) =>  service.Add(model))
    .WithName("CreateTodo")
    .WithOpenApi();

app.MapDelete("/todo/{id}", (ITodoService service,[FromRoute]string id) =>  service.Delete(id))
    .WithName("DeleteTodo")
    .WithOpenApi();

app.Run();

public partial class Program{}