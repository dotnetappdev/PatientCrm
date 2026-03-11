# PatientCRM MAUI App

A .NET 10 MAUI cross-platform application for healthcare patient management, designed to run on Android, iOS, macOS, and Windows.

## Features

### Clinician / Admin Mode
- **Dashboard** — Statistics overview (patients, appointments, prescriptions, alerts)
- **Patient Management** — Search, view, create, and edit patient records with full demographics, clinical notes, appointments, prescriptions, and admissions
- **Appointments** — Daily appointment list with date navigation, booking, and video call integration
- **Departments** — Hospital department and ward management
- **Administration** — Tenant and user management (Admin only)

### Patient Portal Mode
- **My Health Dashboard** — Personalised summary of health data
- **My Appointments** — View upcoming and past appointments with video call links
- **My Medications** — Current prescriptions with repeat request capability
- **My Clinical Notes** — Non-confidential notes shared by clinical team
- **My Letters** — View clinical correspondence
- **My Profile** — Personal health record, NHS identifiers, emergency contact, GP info

## Architecture

- **.NET 10 MAUI** — Single codebase targeting Android, iOS, macOS Catalyst, and Windows
- **MVVM Pattern** — Using CommunityToolkit.Mvvm for clean separation of concerns
- **REST API** — Consumes the PatientCRM Web API for all data
- **JWT Authentication** — Tokens stored securely using platform SecureStorage
- **NHS Color Scheme** — Follows NHS digital design guidelines

## Configuration

Set the API base URL before building:

```csharp
// In App.xaml.cs or via user settings
AppSettings.ApiBaseUrl = "https://your-api-server.example.com/";
```

Or update the default in `Services/AppSettings.cs`.

## Running the App

```bash
# Android
dotnet build -t:Run -f net10.0-android

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0

# iOS (macOS only)
dotnet build -t:Run -f net10.0-ios
```

## Role-Based Navigation

The app automatically routes to the appropriate section based on the user's role:
- **Patient** role → Patient Portal (`/portal/dashboard`)
- **All other roles** → Clinician dashboard (`/dashboard`)
