# RaceDay

RaceDay is a full-stack event management application built with ASP.NET Core and .NET 8. It is designed to manage South African road events, including running, walking, and cycling events.

The application provides a web interface for participants and organisers, supported by a RESTful Web API.

Organisers can create and manage events, categories, routes, enrolments, and participant results. Participants can browse events, enrol in categories, manage their profiles, and view their results.

Event images are stored in Azure Blob Storage, while SQL Server is used for application data. Both the API and MVC web application can run together using Docker Compose.

## Features

- User registration and JWT authentication.
- Role-based access for organisers and participants.
- Event creation and management.
- Event category management.
- Route information management.
- Participant enrolments and cancellation.
- Participant profile management.
- Results recording and viewing.
- Event image uploads and retrieval using Azure Blob Storage.
- MVC web interface.
- Automated tests using xUnit.
- GitHub Actions continuous integration.
- Docker containerisation for the API and web application.

## Technologies Used

| Technology | Purpose |
|---|---|
| C# / .NET 8 | Application development |
| ASP.NET Core Web API | Backend REST API |
| ASP.NET Core MVC | Web frontend |
| Entity Framework Core | Database access |
| SQL Server | Relational database |
| JWT | Authentication |
| BCrypt | Password hashing |
| Swagger / OpenAPI | API documentation |
| Azure Blob Storage | Event image storage |
| xUnit | Automated testing |
| Docker | Application containerisation |
| Docker Compose | Multi-container orchestration |
| Git / GitHub | Version control |
| GitHub Actions | Continuous integration |

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
│   ├── Services/
│   └── Dockerfile
├── RaceDay.Web/
│   ├── Controllers/
│   ├── Views/
│   ├── wwwroot/
│   └── Dockerfile
├── RaceDay.API.Tests/
├── Documentation/
├── Database/
├── docker-compose.yml
├── RaceDay.slnx
├── .gitignore
└── README.md
```

## Prerequisites

For local development:

- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- SQL Server Management Studio (SSMS), recommended
- Git
- Visual Studio or Visual Studio Code

For Docker deployment:

- Docker Desktop
- Docker Compose

An Azure Storage account and private blob container are required for event image storage.

## Getting Started

### 1. Clone the repository

```powershell
git clone https://github.com/Rylosan/RaceDay.Api.git
cd RaceDay.Api
```

### 2. Restore dependencies

```powershell
dotnet restore RaceDay.slnx
```

### 3. Configure the database

RaceDay uses SQL Server with Entity Framework Core.

The default API development connection string uses SQL Server LocalDB:

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

### 4. Apply database migrations

From the `RaceDay.API` folder, run:

```powershell
dotnet ef database update
```

This creates the `RaceDayDb` database and applies the existing migrations.

### 5. Configure application secrets

Sensitive configuration must not be committed to Git.

From the `RaceDay.API` folder, initialise User Secrets if required:

```powershell
dotnet user-secrets init
```

Configure the JWT signing key:

```powershell
dotnet user-secrets set "Jwt:Key" "your-long-development-secret-key"
```

Configure Azure Blob Storage:

```powershell
dotnet user-secrets set "AzureStorage:ConnectionString" "YOUR_STORAGE_CONNECTION_STRING"
dotnet user-secrets set "AzureStorage:ContainerName" "race-day-media"
```

Use your own secure development values. Never commit connection strings, passwords, or signing keys.

## Running the Applications Locally

### Run the API

From the `RaceDay.API` folder:

```powershell
dotnet run
```

Open the Swagger UI using the address displayed in the terminal, followed by `/swagger`.

### Run the MVC application

Open a second terminal and navigate to `RaceDay.Web`:

```powershell
cd RaceDay.Web
dotnet run
```

The MVC application communicates with the API using its configured API base URL.

## Docker Deployment

RaceDay includes Dockerfiles for both the API and MVC application, along with a Docker Compose configuration.

Docker Compose starts:

- `api` – the RaceDay ASP.NET Core Web API.
- `web` – the RaceDay MVC frontend.

SQL Server and Azure Blob Storage remain external services.

### Docker configuration

The Docker Compose configuration uses the following environment variables:

| Variable | Purpose |
|---|---|
| `JWT_KEY` | JWT signing key |
| `SQL_PASSWORD` | SQL Server login password |
| `AZURE_STORAGE_CONNECTION_STRING` | Azure Storage connection string |
| `AZURE_STORAGE_CONTAINER` | Azure blob container name |

For the local Docker setup, these values can be loaded from the API project's .NET User Secrets.

Run the following from the solution root in PowerShell:

```powershell
$secrets = dotnet user-secrets list --project .\RaceDay.API\RaceDay.API.csproj

function Get-SecretValue($name) {
    $line = $secrets | Where-Object {
        $_ -match "^$([regex]::Escape($name))\s*="
    } | Select-Object -First 1

    if (-not $line) {
        return $null
    }

    $separator = $line.IndexOf("=")
    return $line.Substring($separator + 1).Trim()
}

$env:JWT_KEY = Get-SecretValue "Jwt:Key"
$env:SQL_PASSWORD = Get-SecretValue "Docker:SqlPassword"
$env:AZURE_STORAGE_CONNECTION_STRING = Get-SecretValue "AzureStorage:ConnectionString"
$env:AZURE_STORAGE_CONTAINER = Get-SecretValue "AzureStorage:ContainerName"

if (
    $env:JWT_KEY -and
    $env:SQL_PASSWORD -and
    $env:AZURE_STORAGE_CONNECTION_STRING -and
    $env:AZURE_STORAGE_CONTAINER
) {
    Write-Host "All required secrets loaded."
} else {
    Write-Host "One or more required secrets are missing."
}
```

Keep the same PowerShell session open while running Docker Compose, because the environment variables are session-specific.

### Build the Docker images

From the solution root:

```powershell
docker compose build
```

### Start the application

```powershell
docker compose up -d
```

### Access the applications

| Application | Local address |
|---|---|
| MVC website | http://localhost:8081 |
| Web API | http://localhost:5049 |
| Swagger UI | http://localhost:5049/swagger |

### Check container status

```powershell
docker compose ps
```

### View application logs

```powershell
docker compose logs -f
```

To view only API logs:

```powershell
docker compose logs -f api
```

To view only MVC logs:

```powershell
docker compose logs -f web
```

### Stop the application

```powershell
docker compose down
```

This stops and removes the application containers without deleting the SQL Server databases.

## Azure Blob Storage

RaceDay uses Azure Blob Storage to store event images.

The storage container is configured as private. Images are accessed through authenticated application endpoints rather than anonymous public blob access.

The API handles image uploads and retrieval, while the MVC application displays images through the API.

Azure Storage configuration is supplied through environment variables or .NET User Secrets during development.

## Authentication

RaceDay uses JSON Web Tokens (JWT) to authenticate users and role-based authorization to restrict protected endpoints.

Two roles are supported:

- `Organiser`
- `Participant`

### Register a user

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

### Using JWT authentication in Swagger

1. Start the RaceDay API.
2. Open Swagger UI.
3. Register a user or log in using `/api/Auth/login`.
4. Copy the returned JWT token.
5. Click **Authorize** in Swagger.
6. Enter the token in the authorization field.
7. Click **Authorize**, then close the window.

An authenticated user may still receive `403 Forbidden` when attempting an operation that is not permitted for their role.

An invalid or missing token on a protected endpoint results in `401 Unauthorized`.

## API Endpoint Reference

All protected endpoints require a valid JWT token.

### Authentication

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/Auth/register` | Public | Register a user |
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
| POST | `/api/Events` | Organiser | Create an event |
| PUT | `/api/Events/{id}` | Organiser | Update an owned event |
| DELETE | `/api/Events/{id}` | Organiser | Delete an owned event |

### Categories

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/events/{eventId}/categories` | Authenticated | View event categories |
| POST | `/api/events/{eventId}/categories` | Organiser | Add a category |
| PUT | `/api/categories/{id}` | Organiser | Update a category |
| DELETE | `/api/categories/{id}` | Organiser | Delete a category |

### Enrolments

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/Enrollments` | Participant | Enrol in an event |
| GET | `/api/Enrollments/me` | Participant | View own enrolments |
| DELETE | `/api/Enrollments/{id}` | Participant | Cancel own enrolment |

A participant cannot enrol more than once in the same event.

### Results

| Method | Endpoint | Access | Description |
|---|---|---|---|
| POST | `/api/Results` | Organiser | Record a result |
| PUT | `/api/Results/{id}` | Organiser | Update a result |
| GET | `/api/Results/me` | Participant | View own results |
| GET | `/api/events/{eventId}/results` | Authenticated | View event results |

Organisers can record or modify results only for events they own.

### Route Information

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/events/{eventId}/route` | Authenticated | View event route information |
| PUT | `/api/events/{eventId}/route` | Organiser | Create or update route information |

## Database Relationships

The RaceDay database uses the following main relationships:

- One organiser can create many events.
- One event can contain many categories.
- One participant can have many enrolments.
- Each enrolment belongs to one event, one participant, and one category.
- Each enrolment can have one result.
- Each event can have one route information record.
- A participant may enrol only once in the same event.

Entity Framework Core maps these relationships and manages the database structure.

## Common HTTP Status Codes

| Status Code | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `204 No Content` | Resource successfully deleted |
| `400 Bad Request` | Invalid request or business rule violation |
| `401 Unauthorized` | Authentication is missing or invalid |
| `403 Forbidden` | User is authenticated but does not have permission |
| `404 Not Found` | Requested resource does not exist |

## Automated Testing

The RaceDay API includes automated integration tests using:

- xUnit
- Entity Framework Core InMemory
- ASP.NET Core MVC Testing

The test suite currently contains **16 passing tests** covering:

- User registration and login.
- Invalid login attempts.
- Event creation and viewing.
- Role-based event restrictions.
- Participant event enrolment.
- Duplicate enrolment prevention.
- Result authorization.
- Organiser ownership restrictions.
- Route information creation and viewing.
- Route information role restrictions.

Run all automated tests from the solution root:

```powershell
dotnet test RaceDay.slnx
```

All 16 tests should pass successfully.

## Continuous Integration

GitHub Actions is configured to build the solution and execute automated tests when changes are pushed to the repository.

Workflow configuration:

```text
.github/workflows/dotnet.yml
```

This helps ensure that changes do not introduce build errors or failing tests.

## Part 1 Documentation

The Part 1 design and planning artifacts are included in the repository:

- **Entity Relationship Diagram (ERD):** `Documentation/RaceDay_ERD.pdf`
- **Editable ERD source:** `Documentation/RaceDay_ERD.drawio`
- **API Endpoint Plan:** `Documentation/API-Endpoint-Plan.md`
- **SQL Server Database Script:** `Database/RaceDayDatabase.sql`

The database script creates the RaceDay database structure, including primary keys, foreign keys, relationships, and sample data.

## Repository

GitHub repository:

https://github.com/Rylosan/RaceDay.Api
