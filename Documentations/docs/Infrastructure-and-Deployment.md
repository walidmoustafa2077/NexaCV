# Infrastructure & Deployment Guide
**Location:** `Documentations/docs/Infrastructure-and-Deployment.md`  
**System:** NexaCV — GCP + Docker Compose  
**References:** SRD §3.9, §3.10

---

## 1. Infrastructure Overview

![Infrastructure Topology](../images/Infrastructure%20Topology.png)

---

## 2. VM Specification

| Resource | Recommended | Minimum |
| :--- | :--- | :--- |
| Machine type | `e2-standard-2` (2 vCPU, 8 GB) | `e2-medium` (2 vCPU, 4 GB) |
| OS | Ubuntu 22.04 LTS | — |
| Boot disk | 50 GB SSD | 30 GB SSD |
| Region | `me-central1` (Qatar) or `europe-west1` | — |
| Static IP | Yes — reserve a static external IP | — |

---

## 3. Docker Compose Configuration

```yaml
# docker-compose.yml (reference structure)
version: "3.9"

services:
  frontend:
    image: nexacv/frontend:latest
    restart: unless-stopped
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:5000
    depends_on:
      - backend
    networks:
      - internal

  backend:
    image: nexacv/backend:latest
    restart: unless-stopped
    environment:
      - ConnectionStrings__Postgres=Host=pgbouncer;Port=6432;Database=nexacv;Username=app;Password=${DB_PASSWORD}
      - OpenAI__ApiKey=${OPENAI_API_KEY}
      - Stripe__SecretKey=${STRIPE_SECRET_KEY}
      - Jwt__Secret=${JWT_SECRET}
    volumes:
      - resume_storage:/app/storage
    depends_on:
      - pgbouncer
    networks:
      - internal

  pgbouncer:
    image: bitnami/pgbouncer:latest
    restart: unless-stopped
    environment:
      - POSTGRESQL_HOST=postgres
      - POSTGRESQL_PORT=5432
      - POSTGRESQL_USERNAME=app
      - POSTGRESQL_PASSWORD=${DB_PASSWORD}
      - POSTGRESQL_DATABASE=nexacv
      - PGBOUNCER_POOL_MODE=transaction
      - PGBOUNCER_MAX_CLIENT_CONN=100
      - PGBOUNCER_DEFAULT_POOL_SIZE=20
    depends_on:
      - postgres
    networks:
      - internal

  postgres:
    image: postgres:16
    restart: unless-stopped
    environment:
      - POSTGRES_DB=nexacv
      - POSTGRES_USER=app
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - pg_data:/var/lib/postgresql/data
    networks:
      - internal

volumes:
  pg_data:
  resume_storage:

networks:
  internal:
    driver: bridge
```

> **Security:** Never commit `.env` files. Inject secrets from GCP Secret Manager at deploy time using the `--env-file` flag or a startup script.

---

## 4. Nginx Configuration (Reference)

```nginx
# /etc/nginx/sites-available/nexacv
server {
    listen 80;
    server_name nexacv.com www.nexacv.com;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl;
    server_name nexacv.com www.nexacv.com;

    ssl_certificate     /etc/letsencrypt/live/nexacv.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/nexacv.com/privkey.pem;
    ssl_protocols       TLSv1.2 TLSv1.3;

    # Frontend
    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # Backend API
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

---



---

## 6. Firewall Rules

| Rule | Port | Protocol | Source | Action |
| :--- | :--- | :--- | :--- | :--- |
| Allow HTTPS | 443 | TCP | 0.0.0.0/0 | ALLOW |
| Allow HTTP (redirect) | 80 | TCP | 0.0.0.0/0 | ALLOW |
| Allow SSH (admin only) | 22 | TCP | Your IP only | ALLOW |
| Default | All | All | 0.0.0.0/0 | DENY |

> Internal Docker ports (3000, 5000, 5432, 6432) are **not** exposed to the host network — all traffic is routed through Nginx.

---

## 7. Backup Strategy

| Asset | Method | Frequency |
| :--- | :--- | :--- |
| PostgreSQL data | `pg_dump` + upload to GCP Cloud Storage | Daily |
| Generated PDFs/DOCX | Sync `/app/storage` to GCP Cloud Storage | Daily |
| Environment secrets | Stored in GCP Secret Manager (managed) | On change |
| Docker images | Push to GCP Artifact Registry | On every build |
