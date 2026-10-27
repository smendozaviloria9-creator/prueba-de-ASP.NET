# SkillHub — Workshop & Enrollment Management Platform

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL%2016-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Identity & JWT](https://img.shields.io/badge/Security-Identity%20%2B%20JWT-000000?logo=jsonwebtokens&logoColor=white)](https://jwt.io/)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20(100%25%20Passing)-brightgreen)](https://xunit.net/)

---

## 📌 Project Overview

**SkillHub** is a training center offering short, in-person workshops across various disciplines such as photography, culinary arts, public speaking, graphic design, music, and digital marketing.

This project delivers an enterprise-grade solution built in **.NET 10** following **Clean Architecture** principles. It resolves real-world business challenges such as overbooking, duplicate enrollments, schedule clashes, and cancellation constraints.

The solution features:
1. **REST API (`SkillHub.Api`)**: Exposes protected and public endpoints documented with **Swagger/OpenAPI**, secured via **JWT (JSON Web Tokens)**, enabling employees and external systems to manage workshops, bulk-import enrollments from Excel spreadsheets, and generate real-time analytics.
2. **Web Application (`SkillHub.Web`)**: An **ASP.NET Core MVC** presentation layer (Option A) that directly consumes the Application layer to deliver an intuitive participant portal for workshop browsing, real-time seat availability calculation, self-enrollment, cancellation management, and official PDF certificate downloads.

---

## 🏗️ Architecture Design

The project is structured according to **Clean Architecture (Onion Architecture)**, guaranteeing separation of concerns and independent testability:

```text
├── src/
│   ├── SkillHub.Domain/            # Enterprise entities, enums, exceptions, repository contracts
│   ├── SkillHub.Application/       # Business use cases, DTOs, service interfaces & implementations
│   ├── SkillHub.Infrastructure/    # PostgreSQL EF Core DbContext, Identity, QuestPDF, EPPlus, JWT
│   ├── SkillHub.Api/               # REST API controllers, Swagger UI, Exception middleware
│   └── SkillHub.Web/               # ASP.NET Core MVC presentation layer for participants
├── tests/
│   └── SkillHub.Tests/             # xUnit unit tests for domain rules and business workflows
├── skillhub_database.sql           # Complete PostgreSQL schema and data export
├── docker-compose.yml              # Multi-container orchestration (PostgreSQL + API + Web)
└── SkillHub.slnx                   # Solution file
```

### Dependency Rules:
- **Domain** has zero dependencies on databases or external frameworks (pure business logic).
- **Application** depends solely on Domain.
- **Infrastructure** implements persistence, token generation, PDF generation, and Excel parsing contracts.
- **Api** and **Web** are presentation entry points that consume the Application core.

---

## ⚙️ Implemented Business Rules

| # | Business Rule | Description & Implementation |
|---|---|---|
| 1 | **Capacity Control** | Active enrollments cannot exceed workshop maximum capacity. Cancelled enrollments release their seats immediately. Available capacity is computed dynamically: `Capacity - ActiveEnrollmentsCount`. |
| 2 | **Duplicate Prevention** | A participant cannot hold more than one `Active` enrollment in the same workshop. |
| 3 | **Schedule Conflict Detection** | Participants cannot enroll in workshops whose schedule (`StartDateTime` to `EndDateTime`) overlaps with another active workshop in their schedule. |
| 4 | **Future Workshops Only** | Both workshop creation and enrollment require future dates (`StartDateTime > UtcNow`). Workshops that have already started are rejected. |
| 5 | **Cancellation Constraints** | **Participants** can cancel only if there are **at least 24 hours** remaining before workshop start. **Employees** can cancel at any time provided the workshop has not started yet. Every cancellation audits `CancelledById` and `CancelledAt`. |
| 6 | **Re-enrollment** | Participants who cancelled an enrollment can re-enroll in the same workshop if capacity is available and no other rules are violated. |
| 7 | **Excel Bulk Import** | Processes `.xlsx` spreadsheets row-by-row. Valid rows are persisted even if others fail. The response details total processed, successful count, rejected count, and row-level rejection reasons. |
| 8 | **PDF Certificate** | Generated via **QuestPDF** behind an application abstraction (`IPdfReportGenerator`). Only available for active enrollments. Accessible by the participant or employees. |
| 9 | **Top Workshops Report** | Returns workshops ordered in descending order by the number of active enrollments. |

---

## 🛠️ Technologies & Libraries

- **Language & Runtime:** C# 13 / .NET 10.0 SDK
- **Web Frameworks:** ASP.NET Core Web API & ASP.NET Core MVC
- **ORM & Data Access:** Entity Framework Core 10.0 (Code First + Migrations)
- **Database Engine:** PostgreSQL 16 (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Authentication & Authorization:** ASP.NET Core Identity & JWT Bearer (`System.IdentityModel.Tokens.Jwt`)
- **PDF Generation:** QuestPDF 2026.9.0 (Community License)
- **Excel Processing:** EPPlus 8.7.0 (Non-Commercial Personal License)
- **API Documentation:** Swashbuckle Swagger UI (`Swashbuckle.AspNetCore`)
- **Testing Framework:** xUnit, Microsoft.NET.Test.Sdk
- **Containerization:** Docker & Docker Compose

---

## 🚀 Getting Started & Execution

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) running locally on port `5432` OR [Docker Desktop](https://www.docker.com/)

### 1. Database Configuration & Migrations

The connection string is pre-configured in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=skillhub_db;Username=firmeza_user;Password=1043448063"
}
```

Apply migrations to create the schema:
```bash
dotnet ef database update --project src/SkillHub.Infrastructure --startup-project src/SkillHub.Api
```

> **Note:** On application startup, the database is **automatically seeded** with all 6 instructors, 30 participants, and 15 workshops from the official assignment data.

### 2. Running the REST API

```bash
dotnet run --project src/SkillHub.Api
```

- **Swagger UI:** `http://localhost:5292/` or `http://localhost:5292/swagger`
- **Base API URL:** `http://localhost:5292/api/v1`

### 3. Running the Web Application (Participant Portal)

```bash
dotnet run --project src/SkillHub.Web
```

- **Web Portal URL:** `http://localhost:5238/` (or port assigned by launchSettings)
- Allows registering new participants, logging in with pre-seeded accounts, browsing workshops with live capacity pills, enrolling, managing enrollments, and downloading PDF certificates.

### 4. Running with Docker Compose (Optional Deliverable)

To run the entire system including PostgreSQL, API, and Web in isolated containers:
```bash
docker compose up --build
```
- **REST API:** `http://localhost:5051`
- **Web Portal:** `http://localhost:5050`
- **PostgreSQL:** `localhost:5434`

### 5. Running the Unit Tests

```bash
dotnet test tests/SkillHub.Tests/SkillHub.Tests.csproj
```
All 11 unit tests validate domain logic, boundary constraints, schedule overlaps, and cancellation policies.

---

## 🔑 Pre-Seeded Accounts & Credentials

### Employee / Instructor Accounts
All instructors are employees who can manage workshops, import Excel spreadsheets, cancel enrollments, and view reports:

| Full Name | Email | Password | Specialty | Role |
|---|---|---|---|---|
| Andrés Felipe Mejía Arango | `andres.mejia@correo.com` | `Instructor2026*` | Fotografía | Empleado |
| Rosa Elena Barrios Polo | `rosa.barrios@correo.com` | `Instructor2026*` | Cocina | Empleado |
| Mauricio Jiménez Cárdenas | `mauricio.jimenez@correo.com` | `Instructor2026*` | Oratoria | Empleado |
| Natalia Herrera Castaño | `natalia.herrera@correo.com` | `Instructor2026*` | Diseño gráfico | Empleado |
| Hernán Darío Quintero Gil | `hernan.quintero@correo.com` | `Instructor2026*` | Música | Empleado |
| Paola Andrea Vargas Mora | `paola.vargas@correo.com` | `Instructor2026*` | Marketing digital | Empleado |

### Sample Participant Accounts
Participants can log in to view available workshops, enroll, manage their own enrollments, and download PDF certificates:

| Full Name | Email | Password | Role |
|---|---|---|---|
| Jorge Andrés Pérez Rojas | `jorge.perez@correo.com` | `Skillhub2026*` | Participante |
| Luz Marina Ospina Ríos | `luz.ospina@correo.com` | `Skillhub2026*` | Participante |
| Carlos Eduardo Rincón Díaz | `carlos.rincon@correo.com` | `Skillhub2026*` | Participante |
| Valentina Gómez Ruiz | `valentina.gomez@correo.com` | `Skillhub2026*` | Participante |
| Juan Camilo Restrepo Vélez | `juan.restrepo@correo.com` | `Skillhub2026*` | Participante |
*(And 25 additional participants pre-seeded from `SkillHub_Data.xlsx`)*

---

## 📡 REST API Specification

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Public | Register a new participant (`FullName`, `Email`, `Password`) |
| `POST` | `/api/v1/auth/login` | Public | Authenticate and obtain JWT Bearer Token |
| `GET` | `/api/v1/workshops` | Public | List all upcoming workshops with live available capacity |
| `GET` | `/api/v1/workshops/{id}` | Public | Detailed view of a workshop |
| `POST` | `/api/v1/workshops` | Employee | Create a new workshop assigned to an instructor |
| `GET` | `/api/v1/workshops/{id}/enrollments` | Employee | List enrolled participants for a given workshop |
| `POST` | `/api/v1/enrollments` | Participant | Enroll authenticated participant into a workshop |
| `GET` | `/api/v1/enrollments/mine` | Participant | List enrollments belonging to authenticated participant |
| `PATCH` | `/api/v1/enrollments/{id}/cancel` | Owner / Employee | Cancel an enrollment (24h rule enforced for participants) |
| `POST` | `/api/v1/enrollments/import` | Employee | Bulk enroll participants from an uploaded Excel file (`.xlsx`) |
| `GET` | `/api/v1/enrollments/{id}/certificate` | Owner / Employee | Download official enrollment certificate as a PDF |
| `GET` | `/api/v1/reports/top-workshops` | Employee | Analytics report of workshops ordered by active enrollments |

---

## 💾 Database Export (Deliverable)

The final database export containing all schema definitions, constraints, indexes, and seeded data is included at the root of the repository:
- File: [`skillhub_database.sql`](file:///home/sebastian/prueba-de-ASP.NET/skillhub_database.sql)

---

## 👤 Author

- **Full Name:** Sebastián Mendoza Viloria
- **Clan:** Riwi
- **Email:** smendozaviloria9@gmail.com
- **Identity Document:** 1043448063
- **GitHub Repository:** [https://github.com/smendozaviloria9-creator/prueba-de-ASP.NET.git](https://github.com/smendozaviloria9-creator/prueba-de-ASP.NET.git)
