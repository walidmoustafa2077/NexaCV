# NexaCV — Documentation Index
**Version:** Draft v1.0  
**Last Updated:** April 1, 2026

This file is the master index for all NexaCV project documentation. Each entry includes the file path and its purpose.

---

## Documentation Map

![Documentation Map](images/Documentation%20Map.png)

---

## Written Documents (`Documentations/docs/`)

| File | Description | Status |
| :--- | :--- | :--- |
| [System Requirements Document.md](docs/System%20Requirements%20Document.md) | Full SRD: functional, non-functional, security, and UI requirements for NexaCV | ✅ Draft v1.0 |
| [Database Schema (ERD).md](docs/Database%20Schema%20(ERD).md) | Table definitions, column types, constraints, and relationship notes | ✅ Draft v1.0 |
| [API-Endpoints-Reference.md](docs/API-Endpoints-Reference.md) | All REST API endpoints, request/response schemas, and HTTP status codes | ✅ Draft v1.0 |
| [Infrastructure-and-Deployment.md](docs/Infrastructure-and-Deployment.md) | GCP VM setup, Docker Compose config, Nginx, Certbot, firewall rules, backup | ✅ Draft v1.0 |

---

## Diagrams (`Documentations/resources/`)

### Database

| File | Diagram Type | Description |
| :--- | :--- | :--- |
| [resources/database/ERD-Diagram.md](resources/database/ERD-Diagram.md) | `erDiagram` | All 7 database tables with columns and foreign key relationships |

### Architecture

| File | Diagram Type | Description |
| :--- | :--- | :--- |
| [resources/architecture/System-Architecture-Diagram.md](resources/architecture/System-Architecture-Diagram.md) | `graph TB` | Full GCP infrastructure topology and Docker Compose services |
| [resources/architecture/System-Architecture-Diagram.md](resources/architecture/System-Architecture-Diagram.md) | `graph LR` | Component responsibility map (Frontend vs Backend vs DB) |

### Flow Diagrams

| File | Diagram Type | Description |
| :--- | :--- | :--- |
| [resources/flow-diagrams/01-User-States-Flow.md](resources/flow-diagrams/01-User-States-Flow.md) | `stateDiagram-v2` | All 6 application modes and their transitions (matches SRD §3.1) |
| [resources/flow-diagrams/02-Resume-Wizard-Flowchart.md](resources/flow-diagrams/02-Resume-Wizard-Flowchart.md) | `flowchart TD` | 5-step data entry wizard from template selection to AI generation |
| [resources/flow-diagrams/03-AI-Generation-Sequence.md](resources/flow-diagrams/03-AI-Generation-Sequence.md) | `sequenceDiagram` | Initial AI generation: wizard submit → OpenAI → resume preview |
| [resources/flow-diagrams/04-AI-Regeneration-Sequence.md](resources/flow-diagrams/04-AI-Regeneration-Sequence.md) | `sequenceDiagram` + `flowchart` | Per-section regeneration with 3-attempt limit enforcement |
| [resources/flow-diagrams/05-Payment-Sequence.md](resources/flow-diagrams/05-Payment-Sequence.md) | `sequenceDiagram` | Full checkout → Stripe/Paymob → webhook → download unlock flow |

### Pending

| Folder | Content Needed | Priority |
| :--- | :--- | :--- |
| `resources/mockups/` | High-fidelity UI mockups (Figma exports) for wizard steps, resume preview, checkout page | High |
| `resources/wireframes/` | Low-fidelity wireframes for mobile and desktop layouts | Medium |

---

## Technology Stack Quick Reference

| Layer | Technology | Notes |
| :--- | :--- | :--- |
| Frontend | Next.js + Tailwind CSS + Shadcn UI | Multi-step wizard, live cost counter |
| Backend | .NET (C#) | REST API, JWT auth, AI orchestration |
| Database | PostgreSQL 16 + PgBouncer | JSONB for resume data |
| AI | OpenAI GPT-4o-mini | Experience rewriting, skill suggestion, summary gen |
| Payments | Stripe (USD) + Paymob (EGP) | Webhook-based fulfillment |
| Infrastructure | GCP Compute Engine + Docker Compose | Single VM, Nginx reverse proxy |
| Secrets | GCP Secret Manager | API keys never in source control |
| File Storage | GCP VM Disk (Docker volume) | Generated PDFs and DOCX |
