# Infoeduka

Desktop application for managing courses, lecturers and course notifications at Algebra University College.

Course: Project Approach to Applications Development (PAAD)

Team: Luka Mamić, Sven Valentić

## Features

- Login with email and password
- Main window with courses, notifications and lecturers tabs
- Dialogs for adding and editing lecturers and courses
- Notifications with publish and expiry date
- Administrator manages everything, lecturer only own courses and notifications
- Data saved to a JSON file

## Technologies

- C# / .NET 10
- Windows Forms
- JSON file storage (System.Text.Json)
- xUnit

## Run

```
dotnet run --project Infoeduka
```

Default administrator: `admin@algebra.hr` / `admin`

## Test

```
dotnet test
```
