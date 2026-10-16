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