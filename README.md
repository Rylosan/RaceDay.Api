# RaceDay API

RaceDay is an ASP.NET Core Web API for managing South African road events such as running, walking, and cycling events.

The system allows organisers to create and manage events, categories, route information, enrolments, and participant results. Participants can browse events, enrol in event categories, manage their profiles, and view their results.

## Technologies Used

- ASP.NET Core Web API
- .NET 8
- C#
- Entity Framework Core
- SQL Server / LocalDB
- JWT Authentication
- BCrypt password hashing
- Swagger / OpenAPI
- xUnit
- Entity Framework Core In-Memory Database for automated tests
- Git
- GitHub Actions

## User Roles

### Organiser

Organisers can:

- Create, update, and delete their own events.
- Create, update, and delete event categories.
- Add or update route information.
- Record and update participant results.
- View event results.
- Manage their user profile.

### Participant

Participants can:

- Browse available events.
- View event categories and route information.
- Enrol in an event and select a category.
- View their enrolments.
- Cancel their own enrolments.
- View their results.
- Manage their user profile.

## Project Structure

```text
RaceDay.Api/
├── .github/
│   └── workflows/
│       └── dotnet.yml
├── RaceDay.API/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Migrations/
│   ├── Models/
│   └── Services/
├── RaceDay.API.Tests/
├── RaceDay.slnx
├── .gitignore
└── README.md

## Setup Instructions

### Prerequisites

Before running RaceDay, make sure the following are installed:

- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- SQL Server Management Studio (SSMS)
- Git
- Visual Studio or Visual Studio Code

### 1. Clone the Repository

Clone the RaceDay repository and open the project folder.

```powershell
git clone <repository-url>
cd RaceDay.Api
```

Replace `<repository-url>` with the URL of the RaceDay GitHub repository.

### 2. Restore Dependencies

Restore the NuGet packages:

```powershell
dotnet restore RaceDay.slnx
```

### 3. Database Configuration

RaceDay uses SQL Server with Entity Framework Core.

The default development connection string uses SQL Server LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;
```

The database contains the following main tables:

- Users
- Events
- Categories
- Enrollments
- Results
- RouteInfos

### 4. Create the Database

Entity Framework Core migrations are included in the project.

From the `RaceDay.API` folder, run:

```powershell
dotnet ef database update
```

This creates the `RaceDayDb` database and applies the existing migrations.

### 5. Configure the JWT Signing Key

For security, the JWT signing key is not stored in the Git repository.

From the `RaceDay.API` folder, initialise .NET User Secrets if required:

```powershell
dotnet user-secrets init
```

Set a local JWT signing key:

```powershell
dotnet user-secrets set "Jwt:Key" "your-long-development-secret-key"
```

The key should be sufficiently long for JWT signing and should not be committed to source control.

### 6. Run the API

From the `RaceDay.API` folder, run:

```powershell
dotnet run
```

The terminal will display the local address where the API is running.

Open the `/swagger` path in a browser to access the Swagger UI.

## Database Relationships

The RaceDay database uses the following main relationships:

- One organiser can create many events.
- One event can contain many categories.
- One participant can have many enrolments.
- Each enrolment belongs to one event, one participant, and one category.
- Each enrolment can have one result.
- Each event can have one route information record.
- A participant may enrol only once in the same event.

Entity Framework Core is used to map these relationships and enforce the database structure.

## Authentication

RaceDay uses JSON Web Tokens (JWT) to authenticate users and role-based authorization to restrict protected endpoints.

Two roles are supported:

- `Organiser`
- `Participant`

### Register a User

Users can register through:

```text
POST /api/Auth/register
```

Example organiser registration:

```json
{
  "firstName": "Race",
  "lastName": "Organiser",
  "email": "organiser@example.com",
  "password": "password123!",
  "role": "Organiser"
}
```

Example participant registration:

```json
{
  "firstName": "Race",
  "lastName": "Participant",
  "email": "participant@example.com",
  "password": "password123!",
  "role": "Participant"
}
```

Passwords are hashed using BCrypt before being stored in the database.

### Login

Registered users can log in through:

```text
POST /api/Auth/login
```

Example:

```json
{
  "email": "organiser@example.com",
  "password": "password123!"
}
```

A successful login returns a JWT token.

## Using JWT Authentication in Swagger

1. Start the RaceDay API.
2. Open the Swagger UI in your browser.
3. Register a user or log in using `/api/Auth/login`.
4. Copy the JWT token returned by the API.
5. Click the **Authorize** button near the top of Swagger.
6. Paste the JWT token into the authorization field.
7. Click **Authorize**.
8. Close the authorization window.

Swagger will now send the JWT token with requests to protected endpoints.

The API uses role-based authorization. An authenticated user may therefore still receive:

```text
403 Forbidden
```

when attempting an operation that is not permitted for their role.

For example:

- Only an `Organiser` can create an event.
- Only a `Participant` can create an enrolment.
- Organisers can modify only events that they own.
- Participants can delete only their own enrolments.

An invalid or missing authentication token on a protected endpoint results in:

```text
401 Unauthorized
``` 
## API Endpoint Reference

All protected endpoints require a valid JWT token. Role restrictions are shown where applicable.

### Authentication

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/Auth/register` | Public | Register a new organiser or participant |
| POST | `/api/Auth/login` | Public | Log in and receive a JWT token |

### User Profile

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/Users/profile` | Authenticated | View the logged-in user's profile |
| PUT | `/api/Users/profile` | Authenticated | Update the logged-in user's profile |

### Events

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/Events` | Authenticated | View all events |
| GET | `/api/Events/{id}` | Authenticated | View a specific event |
| POST | `/api/Events` | Organiser | Create a new event |
| PUT | `/api/Events/{id}` | Organiser | Update an event owned by the organiser |
| DELETE | `/api/Events/{id}` | Organiser | Delete an event owned by the organiser |

### Categories

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/events/{eventId}/categories` | Authenticated | View categories for an event |
| POST | `/api/events/{eventId}/categories` | Organiser | Add a category to an owned event |
| PUT | `/api/categories/{id}` | Organiser | Update a category belonging to an owned event |
| DELETE | `/api/categories/{id}` | Organiser | Delete a category belonging to an owned event |

### Enrolments

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/Enrollments` | Participant | Enrol in an event and select a category |
| GET | `/api/Enrollments/me` | Participant | View the logged-in participant's enrolments |
| DELETE | `/api/Enrollments/{id}` | Participant | Cancel the participant's own enrolment |

A participant cannot enrol more than once in the same event.

### Results

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/Results` | Organiser | Record a result for an enrolment |
| PUT | `/api/Results/{id}` | Organiser | Update a participant result |
| GET | `/api/Results/me` | Participant | View the logged-in participant's results |
| GET | `/api/events/{eventId}/results` | Authenticated | View results for an event |

Organisers can record or modify results only for events they own.

### Route Information

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/events/{eventId}/route` | Authenticated | View route information for an event |
| PUT | `/api/events/{eventId}/route` | Organiser | Create or update route information for an owned event |

## Common HTTP Status Codes

RaceDay uses standard HTTP response codes, including:

| Status Code | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `204 No Content` | Resource successfully deleted |
| `400 Bad Request` | Invalid request or business rule violation |
| `401 Unauthorized` | Authentication is missing or invalid |
| `403 Forbidden` | User is authenticated but does not have permission |
| `404 Not Found` | Requested resource does not exist |

## Automated Testing

The RaceDay API includes automated integration tests using xUnit, Entity Framework Core InMemory, and ASP.NET Core MVC Testing.

The test suite currently contains 14 passing tests covering:

- User registration and login
- Invalid login attempts
- Event creation and viewing
- Role-based event restrictions
- Participant event enrolment
- Duplicate enrolment prevention
- Result authorization
- Organiser ownership restrictions
- Route information creation and viewing
- Route information role restrictions

Run all automated tests with:

```powershell
dotnet test RaceDay.slnx
```

All 14 tests should pass successfully.