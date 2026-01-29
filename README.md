# 🎵 Songbook

## Was ist das?
Songbook ist eine Webanwendung zur Speicherung von Liedtexten mit Akkorden.
Ziel des Projekts ist es, Webentwicklungskompetenzen zu demonstrieren und ein nützliches Werkzeug für den persönlichen Gebrauch zu schaffen.

## In 2 Schritten starten
1. Repository klonen (per HTTPS oder GitHub Desktop).
2. Lokalen Server starten (folgt später).

## Roadmap
- [x] Projektstruktur (src/docs/deploy), README, Grundsetup
- [x] PostgreSQL + EF Core (Migrationen)
- [x] CRUD Songs + Visibility (public/private) + Ownership
- [x] ASP.NET Core Identity (Login/Logout) + Domain-Profil `Users`
- [x] Authorization Policies + Rollen (Admin)
- [x] Deployment auf VPS (Docker)
- [ ] Admin-Moderation: Songs ausblenden
- [ ] Playlists
- [ ] Suche/Filter

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

Die folgenden Tabellen werden erstellt (Auszug):

- Songs
- Users
- AspNetUsers, AspNetRoles, AspNetUserRoles, ...
- __EFMigrationsHistory

## Verwendete Technologien

- ASP.NET Core Razor Pages
- Entity Framework Core (Code-First)
- PostgreSQL
- DBeaver
- .NET 8 SDK

## Authentication, Authorization & Benutzerprofile

Das Projekt nutzt **ASP.NET Core Identity** für Authentifizierung (Registrierung/Login/Logout)
und eine **separate Domain-Tabelle `Users`** für fachliche Anwendungslogik.

### 1) Authentifizierungs-Account (ASP.NET Core Identity)
- Tabellen: AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, ...
- Verantwortlich für:
  - Login / Logout
  - Passwort-Hash
  - Cookies & Security

### 2) Domain-Benutzerprofil (eigene Tabelle `Users`)
- Verantwortlich für:
  - Besitzer von Songs/Playlists (Domain-Ownership)
  - Anzeige-/Profil-Daten (z. B. `DisplayName`)
- Wichtig: **Nicht** für Authentifizierung zuständig.

### Verknüpfung Account ↔ Profil
- `AspNetUsers.Id` (string) wird in `Users.IdentityUserId` gespeichert.
- Beim **ersten Login** wird automatisch ein Profil in `Users` erstellt (falls nicht vorhanden).
- Die interne Profil-ID (`Users.Id`) wird als Claim `ProfileId` gesetzt.
- In `Users` existiert ein **Unique Index** auf `IdentityUserId` (Schutz vor doppelten Profilen).

### Authorization (Zugriffsschutz)

#### Songs
- Create/Edit/Delete: nur für eingeloggte Benutzer (Ownership).
- Sichtbarkeit:
  - `IsPublic = true`: sichtbar für alle
  - `IsPublic = false`: privat (nur für den Besitzer)
- Listen und Detailseiten filtern entsprechend.

Der Admin kann Songs **nicht löschen**, sondern nur **ausblenden**
(z. B. bei Spam, Beleidigungen oder Regelverstößen).
Ownership bleibt dabei unverändert.

### Authorization: Artists (Referenzdaten)
- `Index`/`Details`: öffentlich
- `Create`/`Edit`/`Delete`: nur **Admin** (Policy `AdminOnly`)
- UI blendet Admin-Buttons für Nicht-Admins aus, Backend bleibt trotzdem geschützt.

### Prinzip: Ownership > Rolle
Admin hat **keine automatischen Rechte** auf fremde Songs.
Das verhindert unnötige Privilegien (Least Privilege) und hält Ownership konsistent.

### Vorteile dieser Architektur
- klare Trennung von Security (Identity) und Business-Logik (Domain-Profil)
- saubere Ownership-Logik (Songs/Playlists gehören dem Domain-Profil)
- gut erweiterbar (z. B. Rollen/Admin, OAuth)

### Rollen (Admin) & Seeding
- Es existiert die Rolle **Admin**.
- Seed ist **idempotent** (mehrfach ausführbar, ohne Duplikate):
  - Rolle **Admin**
  - Benutzer `admin@songbook.local`
- Rollen werden korrekt in Claims geladen.
  `User.IsInRole("Admin")` funktioniert dank `ProfileClaimsPrincipalFactory` (Fix eines echten Bugs).
- Seeding wird beim Application-Startup ausgeführt (lokale Entwicklung & Deployment).


### Admin als Moderator (bewusst vereinfacht)
Die Admin-Rolle übernimmt zusätzlich Moderationsaufgaben.

Admin kann:
- Artists erstellen, bearbeiten und löschen
- problematische Inhalte bereinigen
- Songs ausblenden (nicht löschen)

Diese Lösung ist bewusst einfach gehalten und ausreichend für den aktuellen Projektumfang.

