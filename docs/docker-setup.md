
````markdown
# 🐳 Docker Setup – Songbook

## Ziel
Dieses Setup installiert **PostgreSQL** und **Adminer** in Docker-Containern auf dem IONOS-VPS.  
Es dient als Datenbankbasis für das Songbook-Projekt und ermöglicht eine einfache Verwaltung über das Adminer-Webinterface.

---

## Umgebung
**Server:**  
- Ubuntu 22.04 LTS (64-bit)  
- IONOS VPS (Deutschland, DSGVO-konform)  
- Root-Zugriff per SSH  

**Installierte Komponenten:**  
- Docker Engine 28.5.1  
- Docker Compose 2.40.3  

---

## Installation von Docker

```bash
apt update
apt install -y ca-certificates curl gnupg lsb-release
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" \
  > /etc/apt/sources.list.d/docker.list
apt update
apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
````

---

## Projektstruktur

```bash
/srv/songbook/
├── docker-compose.yml
├── .env
└── data/
```

---

## Datei `.env`

```bash
POSTGRES_USER=songuser
POSTGRES_PASSWORD=dein_passwort
POSTGRES_DB=songbook
```

> ⚠️ Hinweis: Verwenden Sie ein starkes, eigenes Passwort.
> Die Datei `.env` darf **nicht** ins GitHub-Repository hochgeladen werden.

---

## Datei `docker-compose.yml`

```yaml
version: '3.9'

services:
  db:
    image: postgres:16
    container_name: songbook_db
    restart: always
    env_file: .env
    volumes:
      - ./data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  adminer:
    image: adminer
    container_name: songbook_adminer
    restart: always
    ports:
      - "8443:8080"
    depends_on:
      - db
```

---

## Container starten

```bash
cd /srv/songbook
docker compose up -d
```

**Status prüfen:**

```bash
docker ps
```

Erwartete Ausgabe (Beispiel):

```
CONTAINER ID   IMAGE         STATUS         PORTS
abc123         adminer       Up             0.0.0.0:8443->8080/tcp
def456         postgres:16   Up             0.0.0.0:5432->5432/tcp
```

---

## Zugriff auf Adminer

Im Browser öffnen:
👉 **[http://217.154.250.169:8443](http://217.154.250.169:8443)**

**Login-Daten:**

| Feld      | Wert         |
| --------- | ------------ |
| System    | PostgreSQL   |
| Server    | db           |
| Benutzer  | songuser     |
| Passwort  | (aus `.env`) |
| Datenbank | songbook     |

---

## Ergebnis

✅ Docker erfolgreich installiert
✅ PostgreSQL-Datenbank läuft
✅ Adminer-Webinterface erreichbar


