using System.Net;
using System.Net.Http.Json;
using DataAccessor.Contracts;
using Xunit;

namespace MeetingFlow.ComponentTests.DataAccessor;

public class AttendeeTests : IClassFixture<DataAccessorFactory>
{
    private readonly HttpClient _client;

    public AttendeeTests(DataAccessorFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAttendee_WithValidData_ReturnsCreated()
    {
        // 1. Arrange
        var request = new CreateAttendeeRequest("Israel Choen", "test@example.com", "0500000000", "Company");

        // 2. Act
        var response = await _client.PostAsJsonAsync("/data/attendees", request);

        // 3. Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateAttendee_WithInvalidEmail_ReturnsBadRequest()
    {
        // 1. Arrange
        var request = new CreateAttendeeRequest("Israel Choen", "invalid-email", "0500000000", "Company");

        // 2. Act
        var response = await _client.PostAsJsonAsync("/data/attendees", request);

        // 3. Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}