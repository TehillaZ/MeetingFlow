using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using DataAccessor.Data;

namespace MeetingFlow.ComponentTests.DataAccessor;

public class DataAccessorFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Since EF Core 7, AddDbContext calls are additive rather than replacing, so the
            // production UseNpgsql configuration must be fully removed (not just the
            // DbContextOptions<T> descriptor) before UseInMemoryDatabase is registered, or EF
            // sees both providers configured against the same context and throws.
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<MeetingFlowDbContext>) ||
                    d.ServiceType == typeof(MeetingFlowDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>) &&
                     d.ServiceType.GenericTypeArguments[0] == typeof(MeetingFlowDbContext)))
                .ToList();

            foreach (var toRemove in descriptorsToRemove)
            {
                services.Remove(toRemove);
            }

            services.AddDbContext<MeetingFlowDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
            });
        });
    }
}