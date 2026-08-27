using System.Net;
using System.Net.Http.Json;
using DataAccessor.Contracts;
using Xunit;

namespace MeetingFlow.ComponentTests.DataAccessor;

public class VenueTests : IClassFixture<DataAccessorFactory>
{
    private readonly HttpClient _client;

    public VenueTests(DataAccessorFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateVenue_WithNegativeCapacity_ReturnsBadRequest()
    {
        // 1. Arrange
        var request = new CreateVenueRequest("A hall", "Main street 1", "Tel Aviv", -10);

        // 2. Act
        var response = await _client.PostAsJsonAsync("/data/venues", request);

        // 3. Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}