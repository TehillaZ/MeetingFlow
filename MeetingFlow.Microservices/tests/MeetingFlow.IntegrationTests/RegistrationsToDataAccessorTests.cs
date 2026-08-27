using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MeetingFlow.IntegrationTests;

public class RegistrationsToDataAccessorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _dataAccessorClient;

    public RegistrationsToDataAccessorTests(WebApplicationFactory<Program> factory)
    {
        _dataAccessorClient = factory.CreateClient();
    }

    [Fact]
    public async Task RegistrationsManager_CanSendRequest_To_DataAccessor()
    {
       
        var response = await _dataAccessorClient.GetAsync("/data/venues");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}