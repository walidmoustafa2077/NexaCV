# Diagram: System Architecture
**Location:** `Documentations/resources/architecture/System-Architecture-Diagram.md`  
**Scope:** Full infrastructure topology and inter-service communication for NexaCV on GCP.

---

## 1. Infrastructure Topology

![Infrastructure Topology](../../images/Infrastructure%20Topology.png)

---

## 2. Component Responsibility Map

![Layered Component Responsibility Map](../../images/Layered%20Component%20Responsibility%20Map.png)

---

## 3. Network & Security Boundaries

| Layer | Protocol | Notes |
| :--- | :--- | :--- |
| User ↔ Nginx | HTTPS / TLS 1.2+ | All PII in transit is encrypted (Req 3.8.1) |
| Nginx ↔ Frontend | HTTP (internal Docker network) | Trusted internal traffic only |
| Frontend ↔ Backend | HTTP (internal Docker network) | REST/JSON, JWT bearer token enforced |
| Backend ↔ PostgreSQL | TCP via PgBouncer (port 6432) | Connection pooling prevents resource exhaustion |
| Backend ↔ OpenAI | HTTPS | API key loaded from GCP Secret Manager at runtime |
| Backend ↔ Stripe/Paymob | HTTPS | Webhook signature verified on receipt |
