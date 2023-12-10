using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GCPTestContainers.Tests;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    public Func<IServiceCollection,Task> OverideServices { get; set; }
	
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(svc =>
        {
            if (OverideServices != null)
            {
                OverideServices(svc).GetAwaiter().GetResult();
            }
        });
    }
}