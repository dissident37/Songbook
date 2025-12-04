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
