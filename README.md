# Songbook

## Projektüberblick

`Songbook` ist eine serverseitige Webanwendung auf Basis von **ASP.NET Core Razor Pages** zur Verwaltung von Liedern und Interpreten.
Der fachliche Schwerpunkt liegt auf:

- CRUD für Songs und Artists
- Sichtbarkeit von Songs (`öffentlich` / `privat`)
- Ownership über ein separates Domain-Benutzerprofil
- ASP.NET Core Identity für Authentifizierung und Rollen
- PostgreSQL + Entity Framework Core (Code First)
- UI-Internationalisierung (de/en/ru)

Das Projekt ist als **Backend-/Fullstack-Portfolio-Projekt** aufgebaut: Der Schwerpunkt liegt auf Datenmodell, Authentifizierung/Autorisierung, Persistenz und sauberer Serverlogik; die UI ist bewusst einfach gehalten (Razor Pages + Bootstrap).

## Architektur & technische Entscheidungen

### Technologie-Stack

- **.NET 8** (`net8.0`)
- **ASP.NET Core Razor Pages**
- **Entity Framework Core 8**
- **PostgreSQL** (Npgsql)
- **ASP.NET Core Identity** (inkl. Rollen)
- **Bootstrap** (UI-Basis)

### Zwei getrennte DbContexts (gleiche Datenbank)

Das Projekt nutzt bewusst **zwei getrennte EF-Core-Kontexte**:

- `AuthDbContext`
  - verwaltet Identity-Tabellen (`AspNetUsers`, `AspNetRoles`, ...)
  - basiert auf `IdentityDbContext<ApplicationUser>`
- `SongbookDbContext`
  - verwaltet die fachlichen Tabellen (`Songs`, `Artists`, `Users`, `Playlists`, ...)

Beide Kontexte verwenden aktuell dieselbe Connection String (`DefaultConnection`), trennen aber Verantwortlichkeiten im Code.

### Domain-Modell und Ownership

Wichtige fachliche Entitäten:

- `Song`
- `Artist`
- `User` (Domain-Profil)
- `Playlist` / `PlaylistSong`
- `Chord` / `SongChord`

Ownership von Songs wird **nicht** direkt über `AspNetUsers` modelliert, sondern über das Domain-Profil:

- `Song.CreatedByUserId` verweist auf `Users.Id`
- damit bleibt die fachliche Logik von Identity-Details getrennt

### Sichtbarkeit von Songs

Songs unterstützen aktuell ein einfaches Sichtbarkeitsmodell:

- `IsPublic = true` → öffentlich sichtbar
- `IsPublic = false` → privat (für den Besitzer)

Diese Logik wird in den Song- und Artist-Detail-/Listenabfragen serverseitig gefiltert.

### Wichtiger Ist-Zustand (technische Korrektheit)

Das README dokumentiert den aktuellen Code-Stand. Dabei ist relevant:

- `Songs/Create` und `Songs/Edit` sind per `[Authorize]` geschützt
- `Songs/Edit` prüft Ownership (`CreatedByUserId == ProfileId`)
- `Songs/Index` und `Songs/Details` filtern nach Sichtbarkeit/Ownership
- `Songs/Delete` hat aktuell **keinen** `[Authorize]`-Schutz und keine Ownership-Prüfung im PageModel (technische Lücke im aktuellen Stand)

## Internationalisierung (UI-Localization)

Die Internationalisierung betrifft aktuell die **UI-Texte**, nicht die Song-Inhalte aus der Datenbank.

### Konfiguration

In `Program.cs` ist konfiguriert:

- `AddLocalization(ResourcesPath = "Resources")`
- `AddViewLocalization()`
- `AddDataAnnotationsLocalization()`
- `RequestLocalizationOptions` mit unterstützten Kulturen:
  - `de`
  - `en`
  - `ru`
- Default Culture: `de`
- Query-String Provider (`?culture=ru`)

### Persistenz der Sprache (Cookie)

Es existiert eine Razor Page `Pages/SetCulture`:

- setzt `CookieRequestCultureProvider.DefaultCookieName`
- Laufzeit des Cookies: **1 Jahr**
- validiert unterstützte Kulturen (`de`, `en`, `ru`)
- redirectet nur auf lokale `returnUrl` (`LocalRedirect`)

Die Sprachwahl im Layout verwendet einen Dropdown (`select`) und ruft `/SetCulture` mit `culture` + `returnUrl` auf.

### Resource-Strategie

Es werden zwei Resource-Arten verwendet:

- **Shared Resources** (`Resources/SharedResource.*.resx`)
  - z. B. Navbar / Login-Partial / gemeinsame UI-Texte
  - Zugriff über `IStringLocalizer<SharedResource>`
- **Page-based Resources** (`Resources/Pages/...`)
  - pro Razor Page eigene `.resx`
  - Zugriff über `IViewLocalizer`

Beispiele im aktuellen Projekt:

- `Resources/Pages/Songs/*.resx`
- `Resources/Pages/Artists/*.resx`
- `Resources/Pages/Index.*.resx`

## Authentifizierung & Autorisierung

### Identity + Domain-Profil-Trennung

Authentifizierung und fachliches Benutzerprofil sind getrennt:

- **Identity (`ApplicationUser`, `AspNetUsers`)**
  - Login / Logout
  - Passwort-Hash
  - Rollenverwaltung (`Admin`)
- **Domain-Profil (`Users`)**
  - fachliche Referenz für Ownership (`Songs`, `Playlists`)
  - `DisplayName`
  - Unique Index auf `IdentityUserId`

### Verknüpfung und Claims

Eine eigene `ProfileClaimsPrincipalFactory<TIdentityUser>` erzeugt beim Sign-in Claims und stellt sicher:

- falls noch kein Domain-Profil (`Users`) existiert, wird es angelegt
- die interne Profil-ID wird als Claim `ProfileId` gesetzt

Zusätzlich gibt es `ClaimsExtensions.GetProfileId()` für den Zugriff im PageModel.

### Rollen und Policies

In `Program.cs` ist die Policy `AdminOnly` definiert:

- `policy.RequireRole("Admin")`

Aktuell genutzt für Artists:

- `Artists/Create`, `Artists/Edit`, `Artists/Delete` → `[Authorize(Policy = "AdminOnly")]`
- `Artists/Index`, `Artists/Details` → öffentlich (mit Song-Sichtbarkeitsfilter)

### Seed (Entwicklung/Demo)

Beim Application-Startup wird `IdentitySeed.SeedAsync(...)` ausgeführt:

- legt Rolle `Admin` an (falls nicht vorhanden)
- legt Demo-Admin-User an (falls nicht vorhanden)
- weist Rolle `Admin` zu

Aktuell fest im Code hinterlegt (für Entwicklung/Demo):

- E-Mail: `admin@songbook.local`
- Passwort: `Admin123!`

## Deployment (Docker, PostgreSQL)

### PostgreSQL via Docker Compose

Unter `deploy/docker-compose.yml` sind aktuell definiert:

- `db` (PostgreSQL 16)
- `adminer` (DB-Web-UI)

Damit lässt sich die Datenbank lokal/auf einem Server schnell starten.

### Dockerfile (Anwendung)

Es existiert ein `src/Songbook.Web/Dockerfile` für einen Build-/Runtime-Container.

Wichtiger aktueller Stand:

- Projekt targetet **.NET 8**
- Dockerfile verwendet derzeit **.NET 9 SDK/Runtime Images**

Das ist ein realer Stand im Repository und sollte vor produktivem Einsatz konsistent auf dieselbe Major-Version gebracht werden.

### PostgreSQL als Produktionsdatenbank

Die Anwendung ist durchgängig auf PostgreSQL (Npgsql) ausgelegt:

- `SongbookDbContext` → fachliche Daten
- `AuthDbContext` → Identity-Daten

## Projektstruktur

```text
/
├─ src/
│  └─ Songbook.Web/              # ASP.NET Core Razor Pages Anwendung
│     ├─ Auth/                   # Identity-Seed, Claims, ProfileClaimsPrincipalFactory
│     ├─ Data/                   # DbContexts
│     ├─ Models/                 # Domain-Modelle + ApplicationUser
│     ├─ Migrations/             # EF-Migrationen (SongbookDbContext)
│     ├─ Migrations/AuthDb/      # EF-Migrationen (AuthDbContext)
│     ├─ Pages/                  # Razor Pages (Songs, Artists, Shared, SetCulture)
│     ├─ Resources/              # Shared + page-based .resx Dateien
│     ├─ wwwroot/                # Statische Dateien
│     ├─ Program.cs              # Composition Root / Middleware / Localization
│     └─ Dockerfile              # App-Container (aktueller Stand: .NET 9 Images)
├─ deploy/
│  └─ docker-compose.yml         # PostgreSQL + Adminer
├─ docs/                         # zusätzliche Projektdokumentation
└─ README.md
```

## Setup-Anleitung (kurz & präzise)

### Voraussetzungen

- .NET SDK 8.x
- Docker + Docker Compose (für PostgreSQL)
- optional: `dotnet-ef` CLI Tool

### 1. Datenbank starten (PostgreSQL + Adminer)

```bash
docker compose -f deploy/docker-compose.yml up -d
```

### 2. Connection String prüfen

Standardmäßig ist in `src/Songbook.Web/appsettings.json` bereits eine lokale PostgreSQL-Connection konfiguriert:

- Host `localhost`
- Port `5432`
- DB `songbook`
- User `songuser`

Falls abweichend, `DefaultConnection` anpassen.

### 3. EF-Migrationen anwenden (beide DbContexts)

```bash
dotnet ef database update --project src/Songbook.Web --startup-project src/Songbook.Web --context AuthDbContext
dotnet ef database update --project src/Songbook.Web --startup-project src/Songbook.Web --context SongbookDbContext
```

Hinweis: Wenn das Schema von Modell und DB auseinanderläuft (z. B. nach neuen Properties), zuerst eine passende Migration erstellen.

### 4. Anwendung starten

```bash
dotnet run --project src/Songbook.Web
```

### 5. Login (Demo/Admin)

Der Admin-User wird beim Start per Seed angelegt:

- `admin@songbook.local`
- `Admin123!`

## KI-unterstützte Entwicklung

Dieses Projekt wurde (u. a. in der laufenden Weiterentwicklung) mit **KI-Agent-Unterstützung** bearbeitet.

Einsatzbereiche im aktuellen Stand:

- Refactoring und Pflege von Razor Pages
- Einführung/Erweiterung der UI-Lokalisierung (Shared + page-based `.resx`)
- technische Dokumentation (README-Überarbeitung)
- Fehleranalyse (z. B. Modell-/Migrations-Abgleich)

Rahmenbedingungen für den Einsatz:

- Änderungen werden im Repository nachvollziehbar als Code-Änderungen eingecheckt
- Technische Aussagen werden gegen den tatsächlichen Code-Stand geprüft
- KI-Ausgaben ersetzen keine fachliche oder sicherheitsrelevante Review
