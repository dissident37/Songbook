# 🎵 Songbook

## Was ist das?
Songbook ist eine Webanwendung zur Speicherung von Liedtexten mit Akkorden.
Ziel des Projekts ist es, Webentwicklungskompetenzen zu demonstrieren und ein nützliches Werkzeug für den persönlichen Gebrauch zu schaffen.

## In 2 Schritten starten
1. Repository klonen (per HTTPS oder GitHub Desktop).
2. Lokalen Server starten (folgt später).

## Roadmap
- [ ] Tag 1 – Projektstruktur, README, Kanban-Board
- [ ] Tag 2 – einfache Webseite erstellen
- [ ] Tag 3 – Datenbank anbinden
- [ ] Tag 4 – CRUD (Lieder hinzufügen und bearbeiten)
- [ ] Tag 5 – Deployment auf einem Server (VPS bei IONOS)

## Projektstruktur
/src         → Quellcode der ASP.NET Core Anwendung
/docs        → Dokumentation (z. B. docker-setup.md)
/deploy      → Server- und CI/CD-Konfiguration

## Datenbank- und EF Core Setup (PostgreSQL)

### Datenbankeinrichtung (lokale PostgreSQL-Installation)

Das Projekt verwendet PostgreSQL als primäres Datenbanksystem.
Für die lokale Entwicklung wird folgende Konfiguration benötigt:

### 1. Datenbank erstellen
```sql
CREATE DATABASE songbook;
```

### 2. Anwendungsbenutzer erstellen
```sql
CREATE USER songuser WITH PASSWORD 'songpass';
```

### 3. Berechtigungen zuweisen

Der Benutzer `songuser` benötigt ausreichende Rechte auf die Datenbank und auf das Schema `public`, damit Entity Framework Core Tabellen und Migrationen anlegen kann.

```sql
GRANT ALL PRIVILEGES ON DATABASE songbook TO songuser;
GRANT ALL PRIVILEGES ON SCHEMA public TO songuser;
GRANT USAGE, CREATE ON SCHEMA public TO songuser;

ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA public
GRANT ALL PRIVILEGES ON TABLES TO songuser;
```

### Verbindungszeichenkette (Development)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=songbook;Username=songuser;Password=songpass"
  }
}
```

## Entity Framework Core – Migrationen

Die Version der EF-Tools muss mit der verwendeten .NET SDK-Version kompatibel sein.

### Installiert
```
dotnet-ef 8.0.11
.NET SDK 8.x
```

### Migration erstellen
```bash
dotnet ef migrations add InitialCreate
```

### Migration anwenden
```bash
dotnet ef database update
```

Die folgenden Tabellen werden erstellt:

- Songs
- __EFMigrationsHistory

## Verwendete Technologien

- ASP.NET Core Razor Pages
- Entity Framework Core (Code-First)
- PostgreSQL
- DBeaver
- .NET 8 SDK

## Authentication & Benutzerprofile

Das Projekt verwendet **ASP.NET Core Identity** für Authentifizierung
(Registrierung, Login, Logout) und eine **separate Users-Tabelle**
für die fachliche Anwendungslogik.

### Trennung von Account und Profil

Es existieren bewusst zwei unterschiedliche Konzepte:

### 1) Authentifizierungs-Account (ASP.NET Core Identity)
- Tabellen: `AspNetUsers`, `AspNetRoles`, etc.
- Verantwortlich für:
  - Login / Logout
  - Passwort-Hash
  - Cookies & Security
  - Passwort-Reset, E-Mail-Bestätigung (optional)

Diese Tabellen werden automatisch von ASP.NET Core Identity verwaltet.

### 2) Benutzerprofil (eigene Tabelle `Users`)
- Verantwortlich für:
  - Besitzer von Songs
  - Besitzer von Playlists
  - Anwendungslogik und Beziehungen im Domain-Modell

Diese Tabelle ist **nicht** für Authentifizierung zuständig.

### Verknüpfung zwischen Account und Profil

- `AspNetUsers.Id` (string)
  → gespeichert in `Users.IdentityUserId`
- Beim ersten Login oder bei der Registrierung:
  - wird automatisch ein Eintrag in der Tabelle `Users` erstellt (falls nicht vorhanden)
  - die interne Profil-ID (`Users.Id`) wird als Claim (`ProfileId`) gesetzt

Dadurch kann die Anwendung:
- sicher authentifizieren (Identity)
- gleichzeitig sauber mit eigenen Benutzerprofilen arbeiten

### Vorteile dieser Architektur

- klare Trennung von Security und Business-Logik
- saubere und erweiterbare Architektur
- realistische Production-Struktur
- einfache Erweiterung (z. B. OAuth, Google Login)