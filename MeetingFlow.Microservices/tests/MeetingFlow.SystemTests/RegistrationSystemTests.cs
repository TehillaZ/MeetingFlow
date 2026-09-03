using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace MeetingFlow.SystemTests;

/// <summary>
/// Exercises the full registration flow through the real deployed stack (see docker-compose.yml):
///   Gateway -> RegistrationsManager -> DataAccessor -> PostgreSQL
///                                  |-> SchedulingEngine
///                                  '-> RabbitMQ -> NotificationsAccessor -> PostgreSQL
/// No service is faked or swapped; only the public Gateway boundary is exercised for the
/// behavior under test. Requires `docker compose up -d --build` to be running first.
/// </summary>
public class RegistrationSystemTests
{
    static readonly Uri GatewayUrl = new(
        Environment.GetEnvironmentVariable("GATEWAY_URL") ?? "http://localhost:8080");
    static readonly Uri DataAccessorUrl = new(
        Environment.GetEnvironmentVariable("DATA_ACCESSOR_URL") ?? "http://localhost:5010");
    static readonly Uri NotificationsAccessorUrl = new(
        Environment.GetEnvironmentVariable("NOTIFICATIONS_ACCESSOR_URL") ?? "http://localhost:5011");

    // "Frontend Architecture Summit" from DataAccessor.Data.SeedData: Published, 2000-seat venue,
    // only a handful of pre-seeded registrations. Picked for the largest capacity headroom so
    // repeated test runs never trip the SchedulingEngine capacity check.
    static readonly Guid SeededMeetingId = Guid.Parse("b2000000-0000-0000-0000-000000000001");

    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task RegistrationFlow_ThroughGateway_IsPersisted_AndTriggersNotification()
    {
        using var gateway = new HttpClient { BaseAddress = GatewayUrl };
        using var dataAccessor = new HttpClient { BaseAddress = DataAccessorUrl };
        using var notificationsAccessor = new HttpClient { BaseAddress = NotificationsAccessorUrl };

        await WaitUntilHealthyAsync(gateway);
        await WaitUntilHealthyAsync(notificationsAccessor);

        // Arrange: a fresh attendee, created directly against DataAccessor. Gateway has no public
        // "create attendee" endpoint (attendees are assumed to already exist), so this step
        // deliberately reaches past the public boundary -- it is scenario setup, not the thing
        // this test is proving. A brand-new attendee per run also means the test can be re-run
        // against the same database without ever colliding with a prior run's registration.
        var attendeeId = await CreateAttendeeAsync(dataAccessor);

        // Act: the one call that matters for this test, through the real public boundary.
        var createResponse = await gateway.PostAsJsonAsync(
            "/registrations",
            new CreateRegistrationRequest(SeededMeetingId, attendeeId, "General"),
            Json);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateRegistrationResult>(Json);
        Assert.NotNull(created);
        var registrationId = created!.Registration.Id;

        // Assert 1: the saved registration can be read back, through Gateway.
        var byMeeting = await gateway.GetFromJsonAsync<List<RegistrationDto>>(
            $"/registrations/by-meeting/{SeededMeetingId}", Json);
        Assert.NotNull(byMeeting);
        Assert.Contains(byMeeting!, r => r.Id == registrationId && r.AttendeeId == attendeeId);

        // Assert 2: the notification is *eventually* created -- it travels over RabbitMQ to
        // NotificationsAccessor asynchronously, after the HTTP response above already returned.
        var notification = await PollForNotificationAsync(notificationsAccessor, attendeeId);
        Assert.NotNull(notification);
        Assert.Contains(registrationId.ToString(), notification!.Body);
    }

    static async Task<Guid> CreateAttendeeAsync(HttpClient dataAccessor)
    {
        var uniqueEmail = $"system-test-{Guid.NewGuid():N}@example.com";
        var response = await dataAccessor.PostAsJsonAsync(
            "/data/attendees",
            new CreateAttendeeRequest("System Test Attendee", uniqueEmail, null, "SystemTests"),
            Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var attendee = await response.Content.ReadFromJsonAsync<AttendeeContactDto>(Json);
        Assert.NotNull(attendee);
        return attendee!.Id;
    }

    static async Task<NotificationDto?> PollForNotificationAsync(HttpClient notificationsAccessor, Guid attendeeId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(20);
        while (DateTime.UtcNow < deadline)
        {
            var notifications = await notificationsAccessor.GetFromJsonAsync<List<NotificationDto>>(
                $"/notifications/by-attendee/{attendeeId}", Json);
            if (notifications is { Count: > 0 })
            {
                return notifications[0];
            }
            await Task.Delay(500);
        }
        return null;
    }

    static async Task WaitUntilHealthyAsync(HttpClient client)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync("/health");
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException)
            {
                // Not up yet; keep polling until the deadline.
            }
            await Task.Delay(1000);
        }
        throw new InvalidOperationException(
            $"{client.BaseAddress} never became healthy. Is `docker compose up -d --build` running?");
    }

    sealed record CreateAttendeeRequest(string FullName, string Email, string? Phone, string? Company);
    sealed record AttendeeContactDto(Guid Id, string FullName, string Email);
    sealed record CreateRegistrationRequest(Guid MeetingId, Guid AttendeeId, string TicketType);
    sealed record AttendeeSummaryDto(Guid Id, string FullName, string? Company);
    sealed record RegistrationDto(
        Guid Id,
        Guid MeetingId,
        Guid AttendeeId,
        DateTimeOffset RegisteredAt,
        string TicketType,
        string PaymentStatus,
        AttendeeSummaryDto? Attendee);
    sealed record CreateRegistrationResult(RegistrationDto Registration, decimal CalculatedPrice);
    sealed record NotificationDto(
        Guid Id,
        Guid AttendeeId,
        string Type,
        string Subject,
        string Body,
        DateTimeOffset? SentAt);
}
