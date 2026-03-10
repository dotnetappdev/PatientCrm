# PatientCRM

A modern, multi-tenant Patient Clinical Records Management System for NHS England, HSCNI and private UK healthcare practices. Built with ASP.NET Core 10, Entity Framework Core, SQL Server and Bootstrap.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Features](#features)
4. [Technology Stack](#technology-stack)
5. [Getting Started](#getting-started)
6. [Configuration](#configuration)
7. [Authentication and Security](#authentication-and-security)
8. [User Roles](#user-roles)
9. [API Reference](#api-reference)
10. [Seed Data](#seed-data)
11. [Deployment](#deployment)

---

## Overview

PatientCRM is a two-tier web application:

- **PatientCrm.Api** - A RESTful API that owns all business logic, data access, identity management and security.
- **PatientCrm.Web** - A Blazor Web App (.NET 10) with Interactive Server rendering that communicates exclusively with the API via `HttpClient`. The web app holds no direct database connection and contains no Entity Framework references.

This separation ensures the presentation layer is thin, testable and replaceable without touching the data layer.

---

## Architecture

```
┌────────────────────────────────────────────────────┐
│          PatientCrm.Web (Blazor Web App)            │
│  Interactive Server + Cookie Auth + TokenProvider   │
│  Razor Components call API via PatientApiClient     │
└──────────────┬─────────────────────────────────────┘
               │ HTTPS / JWT
┌──────────────▼─────────────────────────┐
│          PatientCrm.Api (REST)         │
│  JWT Bearer Auth + ASP.NET Identity    │
│  2FA + Passkeys (FIDO2/WebAuthn)       │
│  Roles + Claims + Policy Authorization │
└──────────────┬─────────────────────────┘
               │ EF Core
┌──────────────▼─────────────────────────┐
│         SQL Server Database            │
│  Multi-tenant row-level isolation      │
└────────────────────────────────────────┘
```

The solution contains four projects:

| Project | Description |
|---------|-------------|
| `PatientCrm.Core` | Domain entities, enums, interfaces |
| `PatientCrm.Infrastructure` | EF Core DbContext, repositories, seed data |
| `PatientCrm.Api` | REST API with Identity, 2FA, passkeys, JWT |
| `PatientCrm.Web` | Blazor Web App (Interactive Server), cookie auth, API client |

---

## Features

### Clinical

- Multi-tenant patient registration with NHS/HSCNI identifiers
- GP Records with QOF registers (diabetes, hypertension, AF, asthma, COPD, mental health)
- Dental Records with charting, BPE scores and treatment history
- Clinical notes using SOAP format with SNOMED CT coding
- Prescription management with repeat prescription support
- Appointment scheduling with telephone, in-person and video types
- Patient images and attachments (X-rays, reports, referral letters)
- Patient alerts with severity levels (Critical, High, Medium, Low)

### Administration

- Super Admin and Tenant Admin roles with full RBAC
- Multi-tenant client (practice) management
- User management with role assignment
- Audit logging for all record changes

### Security

- ASP.NET Core Identity with SQL Server persistence
- JWT Bearer tokens (8-hour expiry) for API authentication
- Cookie authentication for the web front-end
- Two-Factor Authentication (TOTP via authenticator app)
- Recovery codes (10 codes generated on 2FA setup)
- Passkeys (FIDO2 / WebAuthn) for passwordless login
- Role-based and claims-based authorization
- Account lockout after failed login attempts
- HTTPS enforcement and secure cookie policies

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| API Framework | ASP.NET Core Web API |
| Web Framework | ASP.NET Core Blazor Web App (Interactive Server, .NET 10) |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (LocalDB for development) |
| Authentication | ASP.NET Core Identity |
| 2FA | TOTP via `Microsoft.AspNetCore.Identity` |
| Passkeys | Fido2NetLib (FIDO2 / WebAuthn) |
| API Documentation | Swagger / OpenAPI 3 |
| Frontend Styling | Custom CSS (inspired by the Bookit booking app theme) |

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB is included with Visual Studio)
- Visual Studio 2022 or VS Code with C# Dev Kit

### 1. Clone the repository

```bash
git clone https://github.com/dotnetappdev/PatientCrm.git
cd PatientCrm
```

### 2. Configure the API

Edit `src/PatientCrm.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PatientCrm;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "YourStrongJwtSecretKeyAtLeast32CharactersLong!",
    "Issuer": "PatientCrm",
    "Audience": "PatientCrm"
  },
  "Fido2": {
    "RelyingPartyId": "localhost",
    "RelyingPartyName": "PatientCRM",
    "Origins": ["https://localhost:5001", "http://localhost:5000"]
  }
}
```

**Important:** Always set `Jwt:Key` via environment variables or user secrets in production. Never commit secrets to source control.

### 3. Configure the Web App

Edit `src/PatientCrm.Web/appsettings.Development.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001/"
}
```

### 4. Run the application

Run both projects simultaneously:

```bash
# Terminal 1 - Start the API
cd src/PatientCrm.Api
dotnet run

# Terminal 2 - Start the Web app
cd src/PatientCrm.Web
dotnet run
```

Or use Visual Studio and set multiple startup projects.

The API will be available at `https://localhost:7001` with Swagger UI at `https://localhost:7001/swagger`.

The Web app will be available at `https://localhost:5001` (or as configured).

### 5. Seed data and demo login

The database is automatically created and seeded on first run with realistic demo data.

**Demo credentials:**

| Role | Email | Password |
|------|-------|---------|
| Super Admin | superadmin@patientcrm.nhs.uk | Admin@2024! |
| GP (NHS) | dr.smith@greenfield.nhs.uk | Doctor@2024! |
| GP (NHS) | dr.jones@greenfield.nhs.uk | Doctor@2024! |
| Nurse | nurse.brown@greenfield.nhs.uk | Nurse@2024! |
| Receptionist | reception@greenfield.nhs.uk | Reception@2024! |
| GP (HSCNI) | dr.oneil@belfast.hscni.net | Doctor@2024! |
| Dentist | dr.white@smiledental.co.uk | Dentist@2024! |
| GP (Health Centre) | dr.patel@riverside.nhs.uk | Doctor@2024! |

---

## Configuration

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | `Server=.;Database=PatientCrm;...` |
| `Jwt__Key` | JWT signing key (32+ chars) | `YourVerySecretJwtKey!` |
| `Jwt__Issuer` | JWT issuer | `PatientCrm` |
| `Jwt__Audience` | JWT audience | `PatientCrm` |
| `Fido2__RelyingPartyId` | WebAuthn RP ID (domain) | `patientcrm.example.com` |
| `ApiBaseUrl` | API base URL (Web project) | `https://api.patientcrm.nhs.uk/` |
| `AllowedOrigins__0` | CORS allowed origin | `https://app.patientcrm.nhs.uk` |

### Database Provider

The API uses SQL Server by default. The connection string is configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PatientCrm;Trusted_Connection=True;"
}
```

---

## Authentication and Security

### Password Login Flow

1. User submits email and password at `/Account/Login`
2. Web app calls `POST /api/auth/login`
3. If 2FA is not enabled, the API returns a JWT and a cookie session is established
4. If 2FA is enabled, the API returns `requiresTwoFactor: true` and a short-lived partial token
5. User is redirected to the 2FA verification page
6. After verifying the TOTP code, the API returns a full JWT

### Two-Factor Authentication Setup

Users can set up 2FA from their profile page:

1. Navigate to **Profile > Security > Enable 2FA**
2. Scan the QR code with an authenticator app (Google Authenticator, Microsoft Authenticator, Authy)
3. Enter the 6-digit code to confirm
4. Save the 10 recovery codes shown (each can only be used once)

### Passkey Login

Passkeys (FIDO2/WebAuthn) allow users to log in with biometrics or a hardware key:

1. Register a passkey from **Profile > Security > Add Passkey**
2. On the login page click **Sign in with Passkey**
3. Authenticate with your device biometrics or security key

Passkeys are stored server-side as `UserCredential` records linked to the user account.

### Roles and Claims

| Role | Description |
|------|-------------|
| `SuperAdmin` | Global system administrator; manages all tenants |
| `TenantAdmin` | Manages users and settings within their own tenant |
| `GP` | Full clinical access: patients, notes, prescriptions, appointments |
| `Dentist` | Dental clinical access: patients, dental records, appointments |
| `Consultant` | Secondary care clinical access |
| `Nurse` | Clinical access without prescribing rights |
| `Receptionist` | Appointment management and patient registration |
| `ReadOnly` | View-only access to records |

Roles are enforced both in the API (`[Authorize(Roles = "...")]`) and in the web front-end. The JWT token carries all role and custom claims so the web app can make UI decisions without additional API calls.

---

## API Reference

Swagger documentation is available at `https://localhost:7001/swagger` in development mode.

### Authentication Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/login` | Password login |
| POST | `/api/auth/2fa/verify` | Verify TOTP code |
| GET | `/api/auth/2fa/setup` | Get 2FA setup QR code |
| POST | `/api/auth/2fa/enable` | Enable 2FA with code |
| POST | `/api/auth/2fa/disable` | Disable 2FA |
| GET | `/api/auth/2fa/recovery-codes` | Regenerate recovery codes |
| POST | `/api/auth/passkey/register-options` | Get passkey registration options |
| POST | `/api/auth/passkey/register` | Complete passkey registration |
| POST | `/api/auth/passkey/assertion-options` | Get passkey assertion options |
| POST | `/api/auth/passkey/login` | Complete passkey login |
| GET | `/api/auth/passkeys` | List registered passkeys |
| DELETE | `/api/auth/passkeys/{id}` | Remove a passkey |
| POST | `/api/auth/change-password` | Change password |
| GET | `/api/auth/profile` | Get current user profile |
| PUT | `/api/auth/theme` | Update UI theme preference |

### Patient Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/patients` | List patients (paginated, searchable) |
| GET | `/api/patients/{id}` | Get patient details |
| POST | `/api/patients` | Create patient |
| PUT | `/api/patients/{id}` | Update patient |
| DELETE | `/api/patients/{id}` | Delete patient |

### Clinical Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/notes` | Get clinical notes for a patient |
| POST | `/api/notes` | Add clinical note |
| GET | `/api/appointments` | List appointments |
| POST | `/api/appointments` | Create appointment |

### Admin Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/admin/stats` | System statistics |
| GET | `/api/admin/tenants` | List tenants/practices |
| POST | `/api/admin/tenants` | Create tenant |
| PUT | `/api/admin/tenants/{id}` | Update tenant |
| POST | `/api/admin/tenants/{id}/toggle` | Toggle tenant active status |
| GET | `/api/admin/users` | List users |

---

## Seed Data

On first run the application seeds:

**Organisations (Tenants):**
- Greenfield Medical Practice (NHS England GP Practice, Manchester)
- Belfast HSCNI Trust (HSCNI Hospital, Belfast)
- Smile Dental Care (Private Dental Practice, London)
- Riverside Health Centre (NHS Health Centre, Leeds)

**Patients (10 realistic patients with full clinical records):**
- NHS England GP patients with diabetes, hypertension, heart disease, asthma, mental health conditions
- HSCNI patients with AF, Type 1 diabetes
- Dental patients with periodontal assessments and treatment history
- Full medication lists, allergy records, emergency contacts

**Clinical Notes:** SOAP-format consultation notes for each patient

**Prescriptions:** Realistic repeat prescriptions with drug names, doses and instructions

**Appointments:** Past, present and future appointments across all practices

**Patient Alerts:** Allergy alerts and clinical recall reminders

---

## Deployment

### Docker

A `Dockerfile` and `docker-compose.yml` can be added to run both the API and Web in containers alongside SQL Server.

### Production Checklist

- Set `Jwt:Key` via environment variables or Azure Key Vault
- Use a full SQL Server instance (not LocalDB)
- Enable HTTPS with a valid certificate
- Set `AllowedOrigins` to your actual web app domain
- Set `Fido2:RelyingPartyId` to your actual domain
- Review and enable HSTS, CSP headers
- Enable SQL Server audit logging
- Back up the database regularly

---

## Compliance

This system is designed for NHS and HSCNI healthcare settings. When deploying for real clinical use, ensure:

- IG Toolkit / Data Security and Protection Toolkit compliance
- Information Governance training for all users
- Data Processing Agreement with your hosting provider
- NHS Data Security Standards adherence
- Clinical Safety Case (DCB0160) if applicable

---

## License

This project is provided for demonstration and development purposes.
