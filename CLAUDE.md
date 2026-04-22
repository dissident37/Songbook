# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Docker-Umgebung starten (PostgreSQL + Adminer + Web)
cd deploy && docker compose up --build

# Nur lokal entwickeln (DB muss erreichbar sein)
cd src/Songbook.Web
dotnet run

# Veröffentlichen (Linux-Binary)
dotnet publish -c Release -r linux-x64 --self-contained false
```

## Datenbank-Migrationen

```bash
cd src/Songbook.Web

# Neue Migration erstellen
dotnet ef migrations add <Name> --context SongbookDbContext
dotnet ef migrations add <Name> --context AuthDbContext --output-dir Migrations/AuthDb

# Migrationen anwenden
dotnet ef database update --context SongbookDbContext
dotnet ef database update --context AuthDbContext
```

Die Connection String in `appsettings.json` zeigt auf `Host=db` (Docker-Hostname). Für lokale Entwicklung auf `localhost` ändern.

## Architektur

### Zwei DbContexts, eine Datenbank

- **`AuthDbContext`** — erbt von `IdentityDbContext<ApplicationUser>`, verwaltet ausschließlich ASP.NET Identity-Tabellen (`AspNetUsers`, `AspNetRoles`, etc.)
- **`SongbookDbContext`** — verwaltet Geschäftsdaten: `Songs`, `Artists`, `Users` (Domain-Profil), `Chords`, `Playlists`

Der Domain-`User` (in `SongbookDbContext`) ist **nicht** `ApplicationUser` (Identity). Die Verknüpfung erfolgt über `User.IdentityUserId` (unique index). `ProfileClaimsPrincipalFactory` hängt die `DomainUserId` beim Login als Claim ein.

### Ownership-Modell

Songs gehören einem Domain-`User`, nicht direkt `ApplicationUser`. Pages prüfen Ownership über `song.CreatedByUserId == currentUser.GetDomainUserId()` (via `ClaimsExtensions`).

### Autorisierung

- Rolle `Admin` + Policy `AdminOnly` schützt Artists/Create, Edit, Delete
- Songs/Create, Edit: `[Authorize]` + Ownership-Check im Handler
- **Bekannte Lücke:** `Songs/Delete` hat kein `[Authorize]` und keine Ownership-Prüfung (dokumentiertes TODO)

### Lokalisierung

Drei Sprachen: `de` (Standard), `en`, `ru`. Umschalten per Query-String `?culture=ru` oder Cookie (1 Jahr). Ressourcendateien liegen in `src/Songbook.Web/Resources/`.

### Seeding

`IdentitySeed.SeedAsync()` läuft beim Start und legt an (falls nicht vorhanden):
- Rolle `Admin`
- User `admin@songbook.local` / `Admin123!` mit Admin-Rolle

### Song-Felder

- `Content` — Text mit Akkord-Markup
- `ContentPlain` — Text ohne Akkorde
- `IsPublic` — Sichtbarkeit für andere Nutzer
- `IsHiddenByAdmin` / `HiddenReason` — Moderationsfelder (seit Migration `AddSongModerationFields`)
