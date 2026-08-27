MeetingFlow.md
# MeetingFlow Architecture Analysis & Testing Strategy

## 1. System Architecture Analysis

### Public Boundary of the Complete Backend
- The public entry point for the backend is the **Gateway** running on port 8080.
- Public REST endpoints include `/meetings`, `/speakers`, `/registrations`, `/feedback`, and `/chat`.
- The Gateway uses public API models/DTOs to isolate internal database entities and prevents external clients from setting server-controlled fields (e.g., payment status, timestamps, internal IDs).
- Internal administrative endpoints (such as those in MeetingsManager) are strictly kept inside the internal network and not exposed via the Gateway.

### Boundary of Each Individual Microservice
- **Gateway**: Serves as the public edge router, handling request mapping and routing to downstream services.
- **MeetingsManager**: Orchestrates meetings, sessions, and speaker management logic.
- **RegistrationsManager**: Handles registration flows, fee calculations (pricing), and feedback processing.
- **SchedulingEngine**: Stateless engine performing pure domain logic for schedule conflicts and venue capacity checks.
- **AiChatEngine**: Handles AI interaction logic and action executions.
- **DataAccessor**: Manages EF Core entities and CRUD persistence over PostgreSQL.
- **NotificationsAccessor**: Handles saving and delivering notification messages.
- *Note*: Each service owns its contract project (`.Contracts`) to define interface boundaries for consumers.

### Synchronous HTTP Dependencies
- **Gateway → Managers / Engines**: Synchronous HTTP calls to `MeetingsManager`, `RegistrationsManager`, and `AiChatEngine`.
- **RegistrationsManager → DataAccessor & SchedulingEngine**: HTTP calls to fetch registration context, query capacity (`/scheduling/check-capacity`), and save new registrations.
- **MeetingsManager → DataAccessor & SchedulingEngine**: HTTP calls to fetch meeting data and check schedule conflicts (`/scheduling/check-conflict`).
- **AiChatEngine → DataAccessor**: HTTP calls for data retrieval or action executions.

### Asynchronous Messaging Dependencies
- **Message/Event**: `registration.created.v1` (defined in `MeetingFlow.IntegrationEvents`).
- **Publisher**: `RegistrationsManager` publishes the event to RabbitMQ after successfully saving a registration.
- **Consumer**: `NotificationsAccessor` listens to RabbitMQ, consumes the event, persists the notification, and triggers email sending.

### Infrastructure Owned / Required
- **PostgreSQL 16**: Shared database instance hosting 4 schemas:
  - `meetings`, `registrations`, `feedback` schemas owned by `DataAccessor`.
  - `notifications` schema owned by `NotificationsAccessor`.
- **RabbitMQ**: Message broker required by `RegistrationsManager` (Publisher) and `NotificationsAccessor` (Consumer).
- **Docker Compose**: Orchestrates all service containers, database initializations (`init.sql`), and network links.

---

## 2. Proposed Testing Strategy

| Area | Proposed test level | Entry point | What should be real? | What can be replaced? |
| :--- | :--- | :--- | :--- | :--- |
| **Scheduling rules** | Unit Test | `SchedulingEngine` (`POST /scheduling/*`) | Pure logic for conflict detection and capacity calculations | HTTP framework and network layers (test directly via unit tests) |
| **Data persistence** | Integration Test | `DataAccessor` (Repositories / `POST /data/*`) | Database instance (PostgreSQL / Testcontainers) and EF Core mappings | Calling upstream Manager services |
| **Registration orchestration** | Integration / Component Test | `RegistrationsManager` (`POST /registrations`) | Pricing algorithms (`InlineTicketPricing`), orchestration logic, and DTO mappings | HTTP responses from DataAccessor/SchedulingEngine, RabbitMQ publishing |
| **Notification delivery** | Integration Test | `NotificationsAccessor` (RabbitMQ Event Consumer) | Event consumption pipeline and notification persistence to `notifications` schema | External SMTP/Email server (use Fake Email Sender) |
| **Complete registration flow** | End-to-End (E2E) Test | Gateway (`POST /registrations` on port 8080) | Full system stack: Gateway, Managers, Accessors, PostgreSQL, and RabbitMQ | External 3rd party integrations and UI frontend |